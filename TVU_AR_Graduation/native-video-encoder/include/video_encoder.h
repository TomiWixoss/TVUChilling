#ifndef VIDEO_ENCODER_H
#define VIDEO_ENCODER_H

#include <android/native_window.h>
#include <media/NdkMediaCodec.h>
#include <media/NdkMediaMuxer.h>
#include <EGL/egl.h>
#include <GLES3/gl3.h>
#include <string>

/**
 * Native Video Encoder - GPU-to-GPU encoding
 * Unity OpenGL Texture → EGL Surface → MediaCodec → MP4
 */
class VideoEncoder {
public:
    VideoEncoder();
    ~VideoEncoder();
    
    // Initialize encoder với MediaCodec Surface
    bool Initialize(int width, int height, int fps, int bitrate, const char* outputPath);
    
    // Encode frame từ Unity texture (GPU-to-GPU)
    bool EncodeFrame(GLuint textureId, int64_t presentationTimeUs);
    
    // Get frame count
    int64_t GetFrameCount() const { return frameCount_; }
    
    // Stop encoding và finalize file
    void Stop();
    
    // Check if currently encoding
    bool IsEncoding() const { return isEncoding_; }
    
private:
    // Setup EGL context cho rendering
    bool SetupEGL();
    
    // Setup OpenGL shaders and buffers
    bool SetupOpenGL();
    
    // Setup MediaCodec encoder
    bool SetupMediaCodec();
    
    // Setup MediaMuxer
    bool SetupMuxer();
    
    // Render Unity texture vào encoder surface
    bool RenderTextureToSurface(GLuint textureId);
    
    // Drain encoder output
    void DrainEncoder(bool endOfStream);
    
    // Cleanup resources
    void Cleanup();
    
    // Video parameters
    int width_;
    int height_;
    int fps_;
    int bitrate_;
    std::string outputPath_;
    
    // MediaCodec
    AMediaCodec* mediaCodec_;
    ANativeWindow* encoderSurface_;
    AMediaMuxer* mediaMuxer_;
    int trackIndex_;
    bool muxerStarted_;
    
    // EGL context
    EGLDisplay eglDisplay_;
    EGLContext eglContext_;
    EGLSurface eglSurface_;
    
    // OpenGL resources
    GLuint shaderProgram_;
    GLuint vao_;
    GLuint vbo_;
    
    // State
    bool isEncoding_;
    int64_t frameCount_;
};

#endif // VIDEO_ENCODER_H
