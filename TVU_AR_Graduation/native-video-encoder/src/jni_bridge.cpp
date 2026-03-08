#include <jni.h>
#include <android/log.h>
#include "video_encoder.h"
#include <map>
#include <mutex>

#define LOG_TAG "NativeVideoEncoderJNI"
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO, LOG_TAG, __VA_ARGS__)
#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

// Global encoder instance map (support multiple encoders)
static std::map<int, VideoEncoder*> g_encoders;
static std::mutex g_mutex;
static int g_nextEncoderId = 1;

extern "C" {

/**
 * Initialize video encoder
 * Returns encoder ID (handle)
 */
JNIEXPORT jint JNICALL
Java_com_tvu_argraduation_NativeVideoEncoder_nativeInitialize(
    JNIEnv* env, jclass clazz,
    jint width, jint height, jint fps, jint bitrate, jstring outputPath) {
    
    const char* pathStr = env->GetStringUTFChars(outputPath, nullptr);
    
    LOGI("Initializing encoder: %dx%d @ %dfps, %dbps, path=%s", 
         width, height, fps, bitrate, pathStr);
    
    VideoEncoder* encoder = new VideoEncoder();
    if (!encoder->Initialize(width, height, fps, bitrate, pathStr)) {
        LOGE("Failed to initialize encoder");
        delete encoder;
        env->ReleaseStringUTFChars(outputPath, pathStr);
        return -1;
    }
    
    env->ReleaseStringUTFChars(outputPath, pathStr);
    
    // Store encoder and return ID
    std::lock_guard<std::mutex> lock(g_mutex);
    int encoderId = g_nextEncoderId++;
    g_encoders[encoderId] = encoder;
    
    LOGI("Encoder initialized with ID: %d", encoderId);
    return encoderId;
}

/**
 * Encode frame from Unity texture
 */
JNIEXPORT jboolean JNICALL
Java_com_tvu_argraduation_NativeVideoEncoder_nativeEncodeFrame(
    JNIEnv* env, jclass clazz,
    jint encoderId, jint textureId, jlong presentationTimeUs) {
    
    std::lock_guard<std::mutex> lock(g_mutex);
    
    auto it = g_encoders.find(encoderId);
    if (it == g_encoders.end()) {
        LOGE("Invalid encoder ID: %d", encoderId);
        return JNI_FALSE;
    }
    
    VideoEncoder* encoder = it->second;
    bool success = encoder->EncodeFrame((GLuint)textureId, presentationTimeUs);
    
    return success ? JNI_TRUE : JNI_FALSE;
}

/**
 * Stop encoding and finalize video
 */
JNIEXPORT void JNICALL
Java_com_tvu_argraduation_NativeVideoEncoder_nativeStop(
    JNIEnv* env, jclass clazz, jint encoderId) {
    
    std::lock_guard<std::mutex> lock(g_mutex);
    
    auto it = g_encoders.find(encoderId);
    if (it == g_encoders.end()) {
        LOGE("Invalid encoder ID: %d", encoderId);
        return;
    }
    
    LOGI("Stopping encoder ID: %d", encoderId);
    
    VideoEncoder* encoder = it->second;
    encoder->Stop();
    delete encoder;
    
    g_encoders.erase(it);
    
    LOGI("Encoder stopped and removed");
}

/**
 * Check if encoder is currently encoding
 */
JNIEXPORT jboolean JNICALL
Java_com_tvu_argraduation_NativeVideoEncoder_nativeIsEncoding(
    JNIEnv* env, jclass clazz, jint encoderId) {
    
    std::lock_guard<std::mutex> lock(g_mutex);
    
    auto it = g_encoders.find(encoderId);
    if (it == g_encoders.end()) {
        return JNI_FALSE;
    }
    
    return it->second->IsEncoding() ? JNI_TRUE : JNI_FALSE;
}

} // extern "C"
