package com.tvu.argraduation;

import android.media.MediaCodec;
import android.media.MediaCodecInfo;
import android.media.MediaCodecList;
import android.media.MediaFormat;
import android.media.MediaMuxer;
import android.os.Environment;
import android.util.Log;
import com.unity3d.player.UnityPlayer;
import java.io.File;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

/**
 * Video Recorder Plugin - Optimized MediaCodec H264 Encoder
 * Nhận RGB24 frames từ Unity AsyncGPUReadback và encode thành MP4
 */
public class VideoRecorderPlugin {
    private static final String TAG = "VideoRecorder";
    
    // MediaCodec encoding
    private static MediaCodec mediaCodec;
    private static MediaMuxer mediaMuxer;
    private static String currentVideoPath;
    private static boolean isRecording = false;
    private static int trackIndex = -1;
    private static boolean muxerStarted = false;
    private static long startTime = 0;
    private static int frameCount = 0;
    
    // Video settings
    private static int videoWidth;
    private static int videoHeight;
    private static int videoFPS;

    /**
     * Start video recording với hardware encoder
     */
    public static void StartRecording(int width, int height, int fps) {
        if (isRecording) {
            Log.w(TAG, "Already recording!");
            return;
        }

        try {
            videoWidth = width;
            videoHeight = height;
            videoFPS = fps;
            frameCount = 0;
            startTime = System.nanoTime();

            // Create output file
            File dcimDir = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM), "TVU_AR");
            if (!dcimDir.exists()) {
                dcimDir.mkdirs();
            }
            
            String timestamp = new SimpleDateFormat("yyyyMMdd_HHmmss", Locale.getDefault()).format(new Date());
            String filename = "TVU_AR_" + timestamp + ".mp4";
            currentVideoPath = new File(dcimDir, filename).getAbsolutePath();
            
            // Setup MediaCodec với hardware encoder
            MediaFormat format = MediaFormat.createVideoFormat(MediaFormat.MIMETYPE_VIDEO_AVC, width, height);
            
            // Dùng YUV420 flexible format (hardware encoder tự optimize)
            format.setInteger(MediaFormat.KEY_COLOR_FORMAT, MediaCodecInfo.CodecCapabilities.COLOR_FormatYUV420Flexible);
            format.setInteger(MediaFormat.KEY_BIT_RATE, 8000000); // 8 Mbps cho Full HD
            format.setInteger(MediaFormat.KEY_FRAME_RATE, fps);
            format.setInteger(MediaFormat.KEY_I_FRAME_INTERVAL, 1); // I-frame mỗi 1 giây
            
            // Tìm hardware encoder (nhanh hơn software encoder)
            String codecName = selectCodec(MediaFormat.MIMETYPE_VIDEO_AVC);
            if (codecName != null) {
                mediaCodec = MediaCodec.createByCodecName(codecName);
                Log.i(TAG, "Using hardware encoder: " + codecName);
            } else {
                mediaCodec = MediaCodec.createEncoderByType(MediaFormat.MIMETYPE_VIDEO_AVC);
                Log.i(TAG, "Using default encoder");
            }
            
            mediaCodec.configure(format, null, null, MediaCodec.CONFIGURE_FLAG_ENCODE);
            mediaCodec.start();

            // Setup MediaMuxer
            mediaMuxer = new MediaMuxer(currentVideoPath, MediaMuxer.OutputFormat.MUXER_OUTPUT_MPEG_4);
            
            isRecording = true;
            muxerStarted = false;
            trackIndex = -1;

            Log.i(TAG, "Recording started: " + currentVideoPath + " (" + width + "x" + height + " @ " + fps + "fps)");

        } catch (IOException e) {
            Log.e(TAG, "Failed to start recording: " + e.getMessage());
            e.printStackTrace();
        }
    }

    /**
     * Stop video recording
     */
    public static void StopRecording() {
        if (!isRecording) {
            Log.w(TAG, "Not recording!");
            return;
        }

        try {
            // Signal end of stream
            if (mediaCodec != null) {
                int inputBufferIndex = mediaCodec.dequeueInputBuffer(10000);
                if (inputBufferIndex >= 0) {
                    mediaCodec.queueInputBuffer(inputBufferIndex, 0, 0, 0, MediaCodec.BUFFER_FLAG_END_OF_STREAM);
                }

                // Drain remaining output
                drainEncoder(true);

                mediaCodec.stop();
                mediaCodec.release();
                mediaCodec = null;
            }

            if (mediaMuxer != null) {
                if (muxerStarted) {
                    mediaMuxer.stop();
                }
                mediaMuxer.release();
                mediaMuxer = null;
            }

            isRecording = false;
            muxerStarted = false;

            Log.i(TAG, "Recording stopped: " + currentVideoPath + " (" + frameCount + " frames)");

            // Refresh gallery
            RefreshGallery(currentVideoPath);

        } catch (Exception e) {
            Log.e(TAG, "Failed to stop recording: " + e.getMessage());
            e.printStackTrace();
        }
    }
    
    /**
     * Check if currently recording
     */
    public static boolean IsRecording() {
        return isRecording;
    }

    /**
     * Encode frame từ Unity (RGB24 byte array)
     * Được gọi từ AsyncGPUReadback callback
     */
    public static void EncodeFrame(byte[] frameData, int width, int height) {
        if (!isRecording || mediaCodec == null) {
            return;
        }

        try {
            // Get input buffer
            int inputBufferIndex = mediaCodec.dequeueInputBuffer(10000);
            if (inputBufferIndex >= 0) {
                ByteBuffer inputBuffer = mediaCodec.getInputBuffer(inputBufferIndex);
                if (inputBuffer != null) {
                    inputBuffer.clear();
                    
                    // Convert RGB24 to YUV420 (NV12 format - hardware encoder prefer)
                    byte[] yuvData = RGB24ToNV12(frameData, width, height);
                    inputBuffer.put(yuvData);

                    // Calculate presentation time
                    long presentationTimeUs = (System.nanoTime() - startTime) / 1000;

                    mediaCodec.queueInputBuffer(inputBufferIndex, 0, yuvData.length, presentationTimeUs, 0);
                    frameCount++;
                }
            }

            // Drain encoder output
            drainEncoder(false);

        } catch (Exception e) {
            Log.e(TAG, "Failed to encode frame: " + e.getMessage());
        }
    }

    /**
     * Drain encoder output buffers
     */
    private static void drainEncoder(boolean endOfStream) {
        MediaCodec.BufferInfo bufferInfo = new MediaCodec.BufferInfo();
        
        while (true) {
            int outputBufferIndex = mediaCodec.dequeueOutputBuffer(bufferInfo, endOfStream ? 10000 : 0);

            if (outputBufferIndex == MediaCodec.INFO_OUTPUT_FORMAT_CHANGED) {
                // Add track to muxer (chỉ gọi 1 lần)
                if (!muxerStarted) {
                    MediaFormat format = mediaCodec.getOutputFormat();
                    trackIndex = mediaMuxer.addTrack(format);
                    mediaMuxer.start();
                    muxerStarted = true;
                    Log.i(TAG, "Muxer started, track index: " + trackIndex);
                }
            } else if (outputBufferIndex >= 0) {
                ByteBuffer outputBuffer = mediaCodec.getOutputBuffer(outputBufferIndex);
                
                if (outputBuffer != null && bufferInfo.size > 0 && muxerStarted) {
                    outputBuffer.position(bufferInfo.offset);
                    outputBuffer.limit(bufferInfo.offset + bufferInfo.size);
                    mediaMuxer.writeSampleData(trackIndex, outputBuffer, bufferInfo);
                }
                
                mediaCodec.releaseOutputBuffer(outputBufferIndex, false);

                if ((bufferInfo.flags & MediaCodec.BUFFER_FLAG_END_OF_STREAM) != 0) {
                    break;
                }
            } else {
                break;
            }
        }
    }

    /**
     * Convert RGB24 to NV12 (YUV420 format cho hardware encoder)
     * NV12 = Y plane + interleaved UV plane
     */
    private static byte[] RGB24ToNV12(byte[] rgb, int width, int height) {
        int frameSize = width * height;
        byte[] nv12 = new byte[frameSize * 3 / 2];

        int yIndex = 0;
        int uvIndex = frameSize;

        for (int j = 0; j < height; j++) {
            for (int i = 0; i < width; i++) {
                int index = (j * width + i) * 3;
                
                // RGB values (Unity RGB24 format)
                int R = rgb[index] & 0xff;
                int G = rgb[index + 1] & 0xff;
                int B = rgb[index + 2] & 0xff;

                // Y component (ITU-R BT.601 conversion)
                int Y = ((66 * R + 129 * G + 25 * B + 128) >> 8) + 16;
                nv12[yIndex++] = (byte) clamp(Y, 0, 255);

                // UV components (subsample 2x2)
                if (j % 2 == 0 && i % 2 == 0) {
                    int U = ((-38 * R - 74 * G + 112 * B + 128) >> 8) + 128;
                    int V = ((112 * R - 94 * G - 18 * B + 128) >> 8) + 128;
                    
                    // NV12 format: interleaved UV
                    nv12[uvIndex++] = (byte) clamp(U, 0, 255);
                    nv12[uvIndex++] = (byte) clamp(V, 0, 255);
                }
            }
        }

        return nv12;
    }

    /**
     * Clamp value between min and max
     */
    private static int clamp(int value, int min, int max) {
        return Math.max(min, Math.min(max, value));
    }

    /**
     * Select hardware encoder (nhanh hơn software encoder)
     */
    private static String selectCodec(String mimeType) {
        MediaCodecList codecList = new MediaCodecList(MediaCodecList.REGULAR_CODECS);
        MediaCodecInfo[] codecInfos = codecList.getCodecInfos();
        
        for (MediaCodecInfo codecInfo : codecInfos) {
            if (!codecInfo.isEncoder()) {
                continue;
            }
            
            String[] types = codecInfo.getSupportedTypes();
            for (String type : types) {
                if (type.equalsIgnoreCase(mimeType)) {
                    // Prefer hardware encoder (tên không chứa "OMX.google")
                    String name = codecInfo.getName();
                    if (!name.contains("OMX.google")) {
                        return name;
                    }
                }
            }
        }
        return null;
    }

    /**
     * Refresh Android Gallery
     */
    private static void RefreshGallery(String filePath) {
        try {
            UnityPlayer.currentActivity.sendBroadcast(
                new android.content.Intent(
                    android.content.Intent.ACTION_MEDIA_SCANNER_SCAN_FILE,
                    android.net.Uri.fromFile(new File(filePath))
                )
            );
        } catch (Exception e) {
            Log.e(TAG, "Failed to refresh gallery: " + e.getMessage());
        }
    }
}
