package com.tvu.argraduation;

import android.os.Environment;
import android.util.Log;
import java.io.File;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

/**
 * Native Video Encoder - Java wrapper cho C++ plugin
 * GPU-to-GPU encoding: Unity Texture → MediaCodec Surface → MP4
 */
public class NativeVideoEncoder {
    private static final String TAG = "NativeVideoEncoder";
    
    // Load native library
    static {
        try {
            System.loadLibrary("native-video-encoder");
            Log.i(TAG, "Native library loaded successfully");
        } catch (UnsatisfiedLinkError e) {
            Log.e(TAG, "Failed to load native library: " + e.getMessage());
        }
    }
    
    // Native methods
    private static native int nativeInitialize(int width, int height, int fps, int bitrate, String outputPath);
    private static native boolean nativeEncodeFrame(int encoderId, int textureId, long presentationTimeUs);
    private static native void nativeStop(int encoderId);
    private static native boolean nativeIsEncoding(int encoderId);
    
    // Instance variables
    private int encoderId = -1;
    private long startTimeNs = 0;
    private int frameCount = 0;
    
    /**
     * Initialize encoder
     */
    public boolean initialize(int width, int height, int fps, int bitrate) {
        // Generate output path
        File dcimDir = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM), "TVU_AR");
        if (!dcimDir.exists()) {
            dcimDir.mkdirs();
        }
        
        String timestamp = new SimpleDateFormat("yyyyMMdd_HHmmss", Locale.getDefault()).format(new Date());
        String filename = "TVU_AR_" + timestamp + ".mp4";
        String outputPath = new File(dcimDir, filename).getAbsolutePath();
        
        Log.i(TAG, "Initializing encoder: " + width + "x" + height + " @ " + fps + "fps");
        Log.i(TAG, "Output path: " + outputPath);
        
        encoderId = nativeInitialize(width, height, fps, bitrate, outputPath);
        
        if (encoderId < 0) {
            Log.e(TAG, "Failed to initialize native encoder");
            return false;
        }
        
        startTimeNs = System.nanoTime();
        frameCount = 0;
        
        Log.i(TAG, "Encoder initialized with ID: " + encoderId);
        return true;
    }
    
    /**
     * Encode frame from Unity texture
     * @param textureId Unity native texture ID (from Texture.GetNativeTexturePtr())
     */
    public boolean encodeFrame(int textureId) {
        if (encoderId < 0) {
            Log.e(TAG, "Encoder not initialized");
            return false;
        }
        
        // Calculate presentation time
        long presentationTimeUs = (System.nanoTime() - startTimeNs) / 1000;
        
        boolean success = nativeEncodeFrame(encoderId, textureId, presentationTimeUs);
        
        if (success) {
            frameCount++;
            if (frameCount % 30 == 0) {
                Log.d(TAG, "Encoded " + frameCount + " frames");
            }
        } else {
            Log.e(TAG, "Failed to encode frame " + frameCount);
        }
        
        return success;
    }
    
    /**
     * Stop encoding and finalize video
     */
    public void stop() {
        if (encoderId < 0) {
            Log.w(TAG, "Encoder not initialized");
            return;
        }
        
        Log.i(TAG, "Stopping encoder (encoded " + frameCount + " frames)");
        
        nativeStop(encoderId);
        encoderId = -1;
        
        Log.i(TAG, "Encoder stopped");
    }
    
    /**
     * Check if currently encoding
     */
    public boolean isEncoding() {
        if (encoderId < 0) {
            return false;
        }
        return nativeIsEncoding(encoderId);
    }
    
    /**
     * Get frame count
     */
    public int getFrameCount() {
        return frameCount;
    }
}
