package com.tvu.argraduation;

import android.Manifest;
import android.content.Context;
import android.content.pm.PackageManager;
import android.graphics.ImageFormat;
import android.hardware.camera2.*;
import android.media.Image;
import android.media.ImageReader;
import android.media.MediaCodec;
import android.media.MediaCodecInfo;
import android.media.MediaFormat;
import android.media.MediaMuxer;
import android.os.Environment;
import android.os.Handler;
import android.os.HandlerThread;
import android.util.Log;
import android.view.Surface;
import androidx.core.app.ActivityCompat;
import com.unity3d.player.UnityPlayer;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.text.SimpleDateFormat;
import java.util.Arrays;
import java.util.Date;
import java.util.Locale;

public class VideoRecorderPlugin {
    private static final String TAG = "VideoRecorder";
    
    // Camera2 API
    private static CameraManager cameraManager;
    private static CameraDevice cameraDevice;
    private static CameraCaptureSession captureSession;
    private static String cameraId;
    private static HandlerThread backgroundThread;
    private static Handler backgroundHandler;
    
    // Recording with MediaCodec
    private static MediaCodec mediaCodec;
    private static MediaMuxer mediaMuxer;
    private static String currentVideoPath;
    private static boolean isRecording = false;
    private static int trackIndex = -1;
    private static boolean muxerStarted = false;
    private static long startTime = 0;
    private static int frameCount = 0;
    
    // Photo capture
    private static ImageReader imageReader;

    // Initialize camera
    public static void InitCamera() {
        try {
            Context context = UnityPlayer.currentActivity;
            cameraManager = (CameraManager) context.getSystemService(Context.CAMERA_SERVICE);
            
            // Get back camera
            for (String id : cameraManager.getCameraIdList()) {
                CameraCharacteristics characteristics = cameraManager.getCameraCharacteristics(id);
                Integer facing = characteristics.get(CameraCharacteristics.LENS_FACING);
                if (facing != null && facing == CameraCharacteristics.LENS_FACING_BACK) {
                    cameraId = id;
                    break;
                }
            }
            
            // Start background thread
            backgroundThread = new HandlerThread("CameraBackground");
            backgroundThread.start();
            backgroundHandler = new Handler(backgroundThread.getLooper());
            
            Log.i(TAG, "Camera initialized: " + cameraId);
            
        } catch (Exception e) {
            Log.e(TAG, "Failed to init camera: " + e.getMessage());
        }
    }
    
    // Open camera
    private static void openCamera() {
        try {
            Context context = UnityPlayer.currentActivity;
            if (ActivityCompat.checkSelfPermission(context, Manifest.permission.CAMERA) != PackageManager.PERMISSION_GRANTED) {
                Log.e(TAG, "Camera permission not granted");
                return;
            }
            
            cameraManager.openCamera(cameraId, new CameraDevice.StateCallback() {
                @Override
                public void onOpened(CameraDevice camera) {
                    cameraDevice = camera;
                    Log.i(TAG, "Camera opened");
                }
                
                @Override
                public void onDisconnected(CameraDevice camera) {
                    camera.close();
                    cameraDevice = null;
                }
                
                @Override
                public void onError(CameraDevice camera, int error) {
                    camera.close();
                    cameraDevice = null;
                    Log.e(TAG, "Camera error: " + error);
                }
            }, backgroundHandler);
            
        } catch (Exception e) {
            Log.e(TAG, "Failed to open camera: " + e.getMessage());
        }
    }
    
    // Capture photo
    public static void CapturePhoto() {
        if (cameraDevice == null) {
            openCamera();
            // Retry after 500ms
            backgroundHandler.postDelayed(() -> capturePhotoInternal(), 500);
        } else {
            capturePhotoInternal();
        }
    }
    
    private static void capturePhotoInternal() {
        try {
            // Setup ImageReader for JPEG capture (Full HD)
            imageReader = ImageReader.newInstance(1920, 1080, ImageFormat.JPEG, 1);
            imageReader.setOnImageAvailableListener(reader -> {
                Image image = null;
                try {
                    image = reader.acquireLatestImage();
                    if (image != null) {
                        ByteBuffer buffer = image.getPlanes()[0].getBuffer();
                        byte[] bytes = new byte[buffer.remaining()];
                        buffer.get(bytes);
                        
                        // Save to file
                        File dcimDir = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM), "TVU_AR");
                        if (!dcimDir.exists()) {
                            dcimDir.mkdirs();
                        }
                        
                        String timestamp = new SimpleDateFormat("yyyyMMdd_HHmmss", Locale.getDefault()).format(new Date());
                        String filename = "TVU_AR_" + timestamp + ".jpg";
                        File file = new File(dcimDir, filename);
                        
                        FileOutputStream output = new FileOutputStream(file);
                        output.write(bytes);
                        output.close();
                        
                        Log.i(TAG, "Photo saved: " + file.getAbsolutePath());
                        SendMessageToUnity("OnPhotoSaved", file.getAbsolutePath());
                        RefreshGallery(file.getAbsolutePath());
                    }
                } catch (Exception e) {
                    Log.e(TAG, "Failed to save photo: " + e.getMessage());
                } finally {
                    if (image != null) {
                        image.close();
                    }
                }
            }, backgroundHandler);
            
            // Create capture request
            CaptureRequest.Builder captureBuilder = cameraDevice.createCaptureRequest(CameraDevice.TEMPLATE_STILL_CAPTURE);
            captureBuilder.addTarget(imageReader.getSurface());
            captureBuilder.set(CaptureRequest.CONTROL_MODE, CameraMetadata.CONTROL_MODE_AUTO);
            
            // Create capture session
            cameraDevice.createCaptureSession(Arrays.asList(imageReader.getSurface()), 
                new CameraCaptureSession.StateCallback() {
                    @Override
                    public void onConfigured(CameraCaptureSession session) {
                        try {
                            session.capture(captureBuilder.build(), null, backgroundHandler);
                        } catch (Exception e) {
                            Log.e(TAG, "Failed to capture: " + e.getMessage());
                        }
                    }
                    
                    @Override
                    public void onConfigureFailed(CameraCaptureSession session) {
                        Log.e(TAG, "Capture session config failed");
                    }
                }, backgroundHandler);
            
        } catch (Exception e) {
            Log.e(TAG, "Failed to capture photo: " + e.getMessage());
            SendMessageToUnity("OnRecordingError", e.getMessage());
        }
    }
    
    // Start video recording
    public static void StartRecording(int width, int height, int fps) {
        if (isRecording) {
            Log.w(TAG, "Already recording!");
            return;
        }

        try {
            frameCount = 0;
            startTime = System.nanoTime();

            // Create file path
            File dcimDir = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM), "TVU_AR");
            if (!dcimDir.exists()) {
                dcimDir.mkdirs();
            }
            
            String timestamp = new SimpleDateFormat("yyyyMMdd_HHmmss", Locale.getDefault()).format(new Date());
            String filename = "TVU_AR_" + timestamp + ".mp4";
            currentVideoPath = new File(dcimDir, filename).getAbsolutePath();
            
            // Setup MediaCodec for H264 encoding (YUV420 color format)
            MediaFormat format = MediaFormat.createVideoFormat(MediaFormat.MIMETYPE_VIDEO_AVC, width, height);
            format.setInteger(MediaFormat.KEY_COLOR_FORMAT, MediaCodecInfo.CodecCapabilities.COLOR_FormatYUV420Flexible);
            format.setInteger(MediaFormat.KEY_BIT_RATE, 8000000); // 8 Mbps
            format.setInteger(MediaFormat.KEY_FRAME_RATE, fps);
            format.setInteger(MediaFormat.KEY_I_FRAME_INTERVAL, 1);

            mediaCodec = MediaCodec.createEncoderByType(MediaFormat.MIMETYPE_VIDEO_AVC);
            mediaCodec.configure(format, null, null, MediaCodec.CONFIGURE_FLAG_ENCODE);
            mediaCodec.start();

            // Setup MediaMuxer
            mediaMuxer = new MediaMuxer(currentVideoPath, MediaMuxer.OutputFormat.MUXER_OUTPUT_MPEG_4);
            
            isRecording = true;
            muxerStarted = false;
            trackIndex = -1;

            Log.i(TAG, "Recording started: " + currentVideoPath + " (" + width + "x" + height + " @ " + fps + "fps)");
            SendMessageToUnity("OnRecordingStarted", currentVideoPath);

        } catch (IOException e) {
            Log.e(TAG, "Failed to start recording: " + e.getMessage());
            SendMessageToUnity("OnRecordingError", e.getMessage());
        }
    }

    // Stop video recording
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
                MediaCodec.BufferInfo bufferInfo = new MediaCodec.BufferInfo();
                while (true) {
                    int outputBufferIndex = mediaCodec.dequeueOutputBuffer(bufferInfo, 10000);
                    if (outputBufferIndex >= 0) {
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
            SendMessageToUnity("OnRecordingStopped", currentVideoPath);

            // Refresh gallery
            RefreshGallery(currentVideoPath);

        } catch (Exception e) {
            Log.e(TAG, "Failed to stop recording: " + e.getMessage());
            SendMessageToUnity("OnRecordingError", e.getMessage());
        }
    }
    
    public static boolean IsRecording() {
        return isRecording;
    }
    
    // Toggle flashlight
    public static void SetFlashlight(boolean enabled) {
        if (cameraDevice == null) {
            Log.w(TAG, "Camera not opened");
            return;
        }
        
        try {
            CaptureRequest.Builder builder = cameraDevice.createCaptureRequest(CameraDevice.TEMPLATE_PREVIEW);
            builder.set(CaptureRequest.FLASH_MODE, enabled ? CameraMetadata.FLASH_MODE_TORCH : CameraMetadata.FLASH_MODE_OFF);
            
            if (captureSession != null) {
                captureSession.setRepeatingRequest(builder.build(), null, backgroundHandler);
            }
            
            Log.i(TAG, "Flashlight: " + (enabled ? "ON" : "OFF"));
            
        } catch (Exception e) {
            Log.e(TAG, "Failed to set flashlight: " + e.getMessage());
        }
    }
    
    // Cleanup
    public static void Cleanup() {
        try {
            if (captureSession != null) {
                captureSession.close();
                captureSession = null;
            }
            
            if (cameraDevice != null) {
                cameraDevice.close();
                cameraDevice = null;
            }
            
            if (backgroundThread != null) {
                backgroundThread.quitSafely();
                backgroundThread.join();
                backgroundThread = null;
                backgroundHandler = null;
            }
            
            if (imageReader != null) {
                imageReader.close();
                imageReader = null;
            }
            
        } catch (Exception e) {
            Log.e(TAG, "Failed to cleanup: " + e.getMessage());
        }
    }

    // Nhận frame từ Unity (RGB24 byte array)
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
                    
                    // Convert RGB24 to YUV420 (NV21)
                    byte[] yuvData = RGB24ToYUV420(frameData, width, height);
                    inputBuffer.put(yuvData);

                    // Calculate presentation time
                    long presentationTimeUs = (System.nanoTime() - startTime) / 1000;

                    mediaCodec.queueInputBuffer(inputBufferIndex, 0, yuvData.length, presentationTimeUs, 0);
                    frameCount++;
                }
            }

            // Get output buffer
            MediaCodec.BufferInfo bufferInfo = new MediaCodec.BufferInfo();
            int outputBufferIndex = mediaCodec.dequeueOutputBuffer(bufferInfo, 0);

            if (outputBufferIndex == MediaCodec.INFO_OUTPUT_FORMAT_CHANGED) {
                // Add track to muxer
                MediaFormat format = mediaCodec.getOutputFormat();
                trackIndex = mediaMuxer.addTrack(format);
                mediaMuxer.start();
                muxerStarted = true;
                Log.i(TAG, "Muxer started, track index: " + trackIndex);
            } else if (outputBufferIndex >= 0) {
                ByteBuffer outputBuffer = mediaCodec.getOutputBuffer(outputBufferIndex);
                if (outputBuffer != null && bufferInfo.size > 0 && muxerStarted) {
                    outputBuffer.position(bufferInfo.offset);
                    outputBuffer.limit(bufferInfo.offset + bufferInfo.size);
                    mediaMuxer.writeSampleData(trackIndex, outputBuffer, bufferInfo);
                }
                mediaCodec.releaseOutputBuffer(outputBufferIndex, false);
            }

        } catch (Exception e) {
            Log.e(TAG, "Failed to encode frame: " + e.getMessage());
        }
    }

    // Convert RGB24 to YUV420 (NV21)
    private static byte[] RGB24ToYUV420(byte[] rgb, int width, int height) {
        int frameSize = width * height;
        byte[] yuv = new byte[frameSize * 3 / 2];

        int yIndex = 0;
        int uvIndex = frameSize;

        for (int j = 0; j < height; j++) {
            for (int i = 0; i < width; i++) {
                int index = (j * width + i) * 3;
                int R = rgb[index] & 0xff;
                int G = rgb[index + 1] & 0xff;
                int B = rgb[index + 2] & 0xff;

                // Y
                int Y = ((66 * R + 129 * G + 25 * B + 128) >> 8) + 16;
                yuv[yIndex++] = (byte) Math.max(0, Math.min(255, Y));

                // U and V (subsample 2x2)
                if (j % 2 == 0 && i % 2 == 0) {
                    int U = ((-38 * R - 74 * G + 112 * B + 128) >> 8) + 128;
                    int V = ((112 * R - 94 * G - 18 * B + 128) >> 8) + 128;
                    yuv[uvIndex++] = (byte) Math.max(0, Math.min(255, V));
                    yuv[uvIndex++] = (byte) Math.max(0, Math.min(255, U));
                }
            }
        }

        return yuv;
    }

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

    private static void SendMessageToUnity(String method, String message) {
        UnityPlayer.UnitySendMessage("CameraController", method, message);
    }
}
