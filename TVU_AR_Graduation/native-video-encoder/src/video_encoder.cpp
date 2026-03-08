#include "video_encoder.h"
#include <android/log.h>
#include <cstring>
#include <fcntl.h>
#include <unistd.h>

// EGL extension for presentation time
typedef khronos_stime_nanoseconds_t EGLnsecsANDROID;
typedef EGLBoolean (EGLAPIENTRYP PFNEGLPRESENTATIONTIMEANDROIDPROC)(EGLDisplay dpy, EGLSurface surface, EGLnsecsANDROID time);
static PFNEGLPRESENTATIONTIMEANDROIDPROC eglPresentationTimeANDROID = nullptr;

#define LOG_TAG "NativeVideoEncoder"
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO, LOG_TAG, __VA_ARGS__)
#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

// Vertex shader - simple passthrough
static const char* VERTEX_SHADER = R"(
#version 300 es
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;
out vec2 vTexCoord;
void main() {
    gl_Position = vec4(aPosition, 0.0, 1.0);
    vTexCoord = aTexCoord;
}
)";

// Fragment shader - copy texture
static const char* FRAGMENT_SHADER = R"(
#version 300 es
precision mediump float;
in vec2 vTexCoord;
out vec4 fragColor;
uniform sampler2D uTexture;
void main() {
    fragColor = texture(uTexture, vTexCoord);
}
)";

VideoEncoder::VideoEncoder()
    : width_(0), height_(0), fps_(0), bitrate_(0)
    , mediaCodec_(nullptr), encoderSurface_(nullptr), mediaMuxer_(nullptr)
    , trackIndex_(-1), muxerStarted_(false)
    , eglDisplay_(EGL_NO_DISPLAY), eglContext_(EGL_NO_CONTEXT), eglSurface_(EGL_NO_SURFACE)
    , shaderProgram_(0), vao_(0), vbo_(0)
    , isEncoding_(false), frameCount_(0) {
}

VideoEncoder::~VideoEncoder() {
    Cleanup();
}

bool VideoEncoder::Initialize(int width, int height, int fps, int bitrate, const char* outputPath) {
    width_ = width;
    height_ = height;
    fps_ = fps;
    bitrate_ = bitrate;
    outputPath_ = outputPath;
    frameCount_ = 0;
    
    LOGI("Initializing encoder: %dx%d @ %dfps, %d bps", width, height, fps, bitrate);
    
    // Setup MediaCodec
    if (!SetupMediaCodec()) {
        LOGE("Failed to setup MediaCodec");
        return false;
    }
    
    // Setup EGL context
    if (!SetupEGL()) {
        LOGE("Failed to setup EGL");
        return false;
    }
    
    // Setup Muxer
    if (!SetupMuxer()) {
        LOGE("Failed to setup Muxer");
        return false;
    }
    
    isEncoding_ = true;
    LOGI("Encoder initialized successfully");
    return true;
}

bool VideoEncoder::SetupMediaCodec() {
    // Create H264 encoder
    mediaCodec_ = AMediaCodec_createEncoderByType("video/avc");
    if (!mediaCodec_) {
        LOGE("Failed to create MediaCodec");
        return false;
    }
    
    // Configure format
    AMediaFormat* format = AMediaFormat_new();
    AMediaFormat_setString(format, AMEDIAFORMAT_KEY_MIME, "video/avc");
    AMediaFormat_setInt32(format, AMEDIAFORMAT_KEY_WIDTH, width_);
    AMediaFormat_setInt32(format, AMEDIAFORMAT_KEY_HEIGHT, height_);
    AMediaFormat_setInt32(format, AMEDIAFORMAT_KEY_BIT_RATE, bitrate_);
    AMediaFormat_setInt32(format, AMEDIAFORMAT_KEY_FRAME_RATE, fps_);
    AMediaFormat_setInt32(format, AMEDIAFORMAT_KEY_COLOR_FORMAT, 0x7F000789); // COLOR_FormatSurface
    AMediaFormat_setInt32(format, AMEDIAFORMAT_KEY_I_FRAME_INTERVAL, 1);
    
    // Configure codec
    media_status_t status = AMediaCodec_configure(mediaCodec_, format, nullptr, nullptr, 
                                                   AMEDIACODEC_CONFIGURE_FLAG_ENCODE);
    AMediaFormat_delete(format);
    
    if (status != AMEDIA_OK) {
        LOGE("Failed to configure MediaCodec: %d", status);
        return false;
    }
    
    // Create input surface
    status = AMediaCodec_createInputSurface(mediaCodec_, &encoderSurface_);
    if (status != AMEDIA_OK || !encoderSurface_) {
        LOGE("Failed to create input surface: %d", status);
        return false;
    }
    
    // Start codec
    status = AMediaCodec_start(mediaCodec_);
    if (status != AMEDIA_OK) {
        LOGE("Failed to start MediaCodec: %d", status);
        return false;
    }
    
    LOGI("MediaCodec setup complete");
    return true;
}

bool VideoEncoder::SetupEGL() {
    // Get EGL display
    eglDisplay_ = eglGetDisplay(EGL_DEFAULT_DISPLAY);
    if (eglDisplay_ == EGL_NO_DISPLAY) {
        LOGE("Failed to get EGL display");
        return false;
    }
    
    // Initialize EGL
    if (!eglInitialize(eglDisplay_, nullptr, nullptr)) {
        LOGE("Failed to initialize EGL");
        return false;
    }
    
    // Choose config
    EGLint configAttribs[] = {
        EGL_RENDERABLE_TYPE, EGL_OPENGL_ES3_BIT,
        EGL_SURFACE_TYPE, EGL_WINDOW_BIT,
        EGL_RED_SIZE, 8,
        EGL_GREEN_SIZE, 8,
        EGL_BLUE_SIZE, 8,
        EGL_ALPHA_SIZE, 8,
        EGL_NONE
    };
    
    EGLConfig config;
    EGLint numConfigs;
    if (!eglChooseConfig(eglDisplay_, configAttribs, &config, 1, &numConfigs)) {
        LOGE("Failed to choose EGL config");
        return false;
    }
    
    // Create EGL context
    EGLint contextAttribs[] = {
        EGL_CONTEXT_CLIENT_VERSION, 3,
        EGL_NONE
    };
    
    eglContext_ = eglCreateContext(eglDisplay_, config, EGL_NO_CONTEXT, contextAttribs);
    if (eglContext_ == EGL_NO_CONTEXT) {
        LOGE("Failed to create EGL context");
        return false;
    }
    
    // Create window surface from MediaCodec surface
    eglSurface_ = eglCreateWindowSurface(eglDisplay_, config, encoderSurface_, nullptr);
    if (eglSurface_ == EGL_NO_SURFACE) {
        LOGE("Failed to create EGL surface");
        return false;
    }
    
    // Make current
    if (!eglMakeCurrent(eglDisplay_, eglSurface_, eglSurface_, eglContext_)) {
        LOGE("Failed to make EGL context current");
        return false;
    }
    
    // Load EGL extension for presentation time
    eglPresentationTimeANDROID = (PFNEGLPRESENTATIONTIMEANDROIDPROC)eglGetProcAddress("eglPresentationTimeANDROID");
    if (!eglPresentationTimeANDROID) {
        LOGE("Failed to get eglPresentationTimeANDROID");
        return false;
    }
    
    // Setup OpenGL shaders and buffers
    if (!SetupOpenGL()) {
        LOGE("Failed to setup OpenGL");
        return false;
    }
    
    LOGI("EGL setup complete");
    return true;
}

bool VideoEncoder::SetupOpenGL() {
    // Compile vertex shader
    GLuint vertexShader = glCreateShader(GL_VERTEX_SHADER);
    glShaderSource(vertexShader, 1, &VERTEX_SHADER, nullptr);
    glCompileShader(vertexShader);
    
    GLint success;
    glGetShaderiv(vertexShader, GL_COMPILE_STATUS, &success);
    if (!success) {
        char infoLog[512];
        glGetShaderInfoLog(vertexShader, 512, nullptr, infoLog);
        LOGE("Vertex shader compilation failed: %s", infoLog);
        return false;
    }
    
    // Compile fragment shader
    GLuint fragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
    glShaderSource(fragmentShader, 1, &FRAGMENT_SHADER, nullptr);
    glCompileShader(fragmentShader);
    
    glGetShaderiv(fragmentShader, GL_COMPILE_STATUS, &success);
    if (!success) {
        char infoLog[512];
        glGetShaderInfoLog(fragmentShader, 512, nullptr, infoLog);
        LOGE("Fragment shader compilation failed: %s", infoLog);
        return false;
    }
    
    // Link shader program
    shaderProgram_ = glCreateProgram();
    glAttachShader(shaderProgram_, vertexShader);
    glAttachShader(shaderProgram_, fragmentShader);
    glLinkProgram(shaderProgram_);
    
    glGetProgramiv(shaderProgram_, GL_LINK_STATUS, &success);
    if (!success) {
        char infoLog[512];
        glGetProgramInfoLog(shaderProgram_, 512, nullptr, infoLog);
        LOGE("Shader program linking failed: %s", infoLog);
        return false;
    }
    
    glDeleteShader(vertexShader);
    glDeleteShader(fragmentShader);
    
    // Setup fullscreen quad
    float vertices[] = {
        // positions   // texCoords
        -1.0f,  1.0f,  0.0f, 1.0f,  // top left
        -1.0f, -1.0f,  0.0f, 0.0f,  // bottom left
         1.0f, -1.0f,  1.0f, 0.0f,  // bottom right
        -1.0f,  1.0f,  0.0f, 1.0f,  // top left
         1.0f, -1.0f,  1.0f, 0.0f,  // bottom right
         1.0f,  1.0f,  1.0f, 1.0f   // top right
    };
    
    glGenVertexArrays(1, &vao_);
    glGenBuffers(1, &vbo_);
    
    glBindVertexArray(vao_);
    glBindBuffer(GL_ARRAY_BUFFER, vbo_);
    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);
    
    // Position attribute
    glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, 4 * sizeof(float), (void*)0);
    glEnableVertexAttribArray(0);
    
    // TexCoord attribute
    glVertexAttribPointer(1, 2, GL_FLOAT, GL_FALSE, 4 * sizeof(float), (void*)(2 * sizeof(float)));
    glEnableVertexAttribArray(1);
    
    glBindVertexArray(0);
    
    LOGI("OpenGL setup complete");
    return true;
}

bool VideoEncoder::RenderTextureToSurface(GLuint textureId) {
    // Set viewport
    glViewport(0, 0, width_, height_);
    glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
    glClear(GL_COLOR_BUFFER_BIT);
    
    // Use shader program
    glUseProgram(shaderProgram_);
    
    // Bind Unity texture
    glActiveTexture(GL_TEXTURE0);
    glBindTexture(GL_TEXTURE_2D, textureId);
    glUniform1i(glGetUniformLocation(shaderProgram_, "uTexture"), 0);
    
    // Draw fullscreen quad
    glBindVertexArray(vao_);
    glDrawArrays(GL_TRIANGLES, 0, 6);
    glBindVertexArray(0);
    
    // Check for errors
    GLenum error = glGetError();
    if (error != GL_NO_ERROR) {
        LOGE("OpenGL error: 0x%x", error);
        return false;
    }
    
    return true;
}

void VideoEncoder::DrainEncoder(bool endOfStream) {
    if (endOfStream) {
        // Signal end of stream
        AMediaCodec_signalEndOfInputStream(mediaCodec_);
    }
    
    // Drain output buffers
    while (true) {
        AMediaCodecBufferInfo bufferInfo;
        ssize_t bufferIndex = AMediaCodec_dequeueOutputBuffer(mediaCodec_, &bufferInfo, 
                                                               endOfStream ? 10000 : 0);
        
        if (bufferIndex == AMEDIACODEC_INFO_OUTPUT_FORMAT_CHANGED) {
            // Add track to muxer
            if (!muxerStarted_) {
                AMediaFormat* format = AMediaCodec_getOutputFormat(mediaCodec_);
                trackIndex_ = AMediaMuxer_addTrack(mediaMuxer_, format);
                AMediaFormat_delete(format);
                
                AMediaMuxer_start(mediaMuxer_);
                muxerStarted_ = true;
                LOGI("Muxer started, track index: %d", trackIndex_);
            }
        } else if (bufferIndex >= 0) {
            // Write sample to muxer
            if (muxerStarted_ && bufferInfo.size > 0) {
                size_t bufferSize;
                uint8_t* buffer = AMediaCodec_getOutputBuffer(mediaCodec_, bufferIndex, &bufferSize);
                
                if (buffer) {
                    AMediaMuxer_writeSampleData(mediaMuxer_, trackIndex_, buffer, &bufferInfo);
                }
            }
            
            AMediaCodec_releaseOutputBuffer(mediaCodec_, bufferIndex, false);
            
            if (bufferInfo.flags & AMEDIACODEC_BUFFER_FLAG_END_OF_STREAM) {
                break;
            }
        } else {
            break;
        }
    }
}

void VideoEncoder::Stop() {
    if (!isEncoding_) {
        return;
    }
    
    LOGI("Stopping encoder (encoded %lld frames)", (long long)frameCount_);
    
    // Drain remaining frames
    DrainEncoder(true);
    
    isEncoding_ = false;
    Cleanup();
    
    LOGI("Encoder stopped");
}

bool VideoEncoder::SetupMuxer() {
    // Open file descriptor for muxer
    int fd = open(outputPath_.c_str(), O_WRONLY | O_CREAT | O_TRUNC, 0644);
    if (fd < 0) {
        LOGE("Failed to open output file: %s", outputPath_.c_str());
        return false;
    }
    
    // Create muxer with file descriptor
    mediaMuxer_ = AMediaMuxer_new(fd, AMEDIAMUXER_OUTPUT_FORMAT_MPEG_4);
    close(fd); // Muxer takes ownership, can close fd
    
    if (!mediaMuxer_) {
        LOGE("Failed to create MediaMuxer");
        return false;
    }
    
    trackIndex_ = -1;
    muxerStarted_ = false;
    
    LOGI("MediaMuxer setup complete");
    return true;
}

bool VideoEncoder::EncodeFrame(GLuint textureId, int64_t presentationTimeUs) {
    if (!isEncoding_) {
        LOGE("Encoder not initialized");
        return false;
    }
    
    // Make EGL context current
    if (!eglMakeCurrent(eglDisplay_, eglSurface_, eglSurface_, eglContext_)) {
        LOGE("Failed to make EGL context current");
        return false;
    }
    
    // Render Unity texture to encoder surface
    if (!RenderTextureToSurface(textureId)) {
        LOGE("Failed to render texture");
        return false;
    }
    
    // Set presentation time
    eglPresentationTimeANDROID(eglDisplay_, eglSurface_, presentationTimeUs * 1000);
    
    // Swap buffers (submit frame to MediaCodec)
    if (!eglSwapBuffers(eglDisplay_, eglSurface_)) {
        LOGE("Failed to swap buffers");
        return false;
    }
    
    // Drain encoder output
    DrainEncoder(false);
    
    frameCount_++;
    
    if (frameCount_ % 30 == 0) {
        LOGI("Encoded %lld frames", (long long)frameCount_);
    }
    
    return true;
}

void VideoEncoder::Cleanup() {
    // Cleanup OpenGL resources
    if (shaderProgram_ != 0) {
        glDeleteProgram(shaderProgram_);
        shaderProgram_ = 0;
    }
    
    if (vao_ != 0) {
        glDeleteVertexArrays(1, &vao_);
        vao_ = 0;
    }
    
    if (vbo_ != 0) {
        glDeleteBuffers(1, &vbo_);
        vbo_ = 0;
    }
    
    // Cleanup EGL
    if (eglDisplay_ != EGL_NO_DISPLAY) {
        eglMakeCurrent(eglDisplay_, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
        
        if (eglSurface_ != EGL_NO_SURFACE) {
            eglDestroySurface(eglDisplay_, eglSurface_);
            eglSurface_ = EGL_NO_SURFACE;
        }
        
        if (eglContext_ != EGL_NO_CONTEXT) {
            eglDestroyContext(eglDisplay_, eglContext_);
            eglContext_ = EGL_NO_CONTEXT;
        }
        
        eglTerminate(eglDisplay_);
        eglDisplay_ = EGL_NO_DISPLAY;
    }
    
    // Stop and release MediaCodec
    if (mediaCodec_) {
        AMediaCodec_stop(mediaCodec_);
        AMediaCodec_delete(mediaCodec_);
        mediaCodec_ = nullptr;
    }
    
    // Release surface
    if (encoderSurface_) {
        ANativeWindow_release(encoderSurface_);
        encoderSurface_ = nullptr;
    }
    
    // Stop and release Muxer
    if (mediaMuxer_) {
        if (muxerStarted_) {
            AMediaMuxer_stop(mediaMuxer_);
        }
        AMediaMuxer_delete(mediaMuxer_);
        mediaMuxer_ = nullptr;
    }
}
