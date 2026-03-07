package com.tvu.argraduation;

import android.media.MediaRecorder;
import android.os.Environment;
import android.util.Log;
import com.unity3d.player.UnityPlayer;
import java.io.File;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

public class VideoRecorderPlugin {
    private static final String TAG = "VideoRecorder";
    private static MediaRecorder mediaRecorder;
    private static String currentVideoPath;
    private static boolean isRecording = false;

    public static void StartRecording(int width, int height, int fps) {
        if (isRecording) {
            Log.w(TAG, "Already recording!");
            return;
        }

        try {
            // Tạo file path
            File dcimDir = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM), "TVU_AR");
            if (!dcimDir.exists()) {
                dcimDir.mkdirs();
            }

            String timestamp = new SimpleDateFormat("yyyyMMdd_HHmmss", Locale.getDefault()).format(new Date());
            String filename = "TVU_AR_" + timestamp + ".mp4";
            currentVideoPath = new File(dcimDir, filename).getAbsolutePath();

            // Setup MediaRecorder
            mediaRecorder = new MediaRecorder();
            mediaRecorder.setAudioSource(MediaRecorder.AudioSource.MIC);
            mediaRecorder.setVideoSource(MediaRecorder.VideoSource.SURFACE);
            mediaRecorder.setOutputFormat(MediaRecorder.OutputFormat.MPEG_4);
            mediaRecorder.setOutputFile(currentVideoPath);
            mediaRecorder.setVideoEncoder(MediaRecorder.VideoEncoder.H264);
            mediaRecorder.setAudioEncoder(MediaRecorder.AudioEncoder.AAC);
            mediaRecorder.setVideoSize(width, height);
            mediaRecorder.setVideoFrameRate(fps);
            mediaRecorder.setVideoEncodingBitRate(5000000); // 5 Mbps

            mediaRecorder.prepare();
            mediaRecorder.start();
            isRecording = true;

            Log.i(TAG, "Recording started: " + currentVideoPath);
            SendMessageToUnity("OnRecordingStarted", currentVideoPath);

        } catch (IOException e) {
            Log.e(TAG, "Failed to start recording: " + e.getMessage());
            SendMessageToUnity("OnRecordingError", e.getMessage());
        }
    }

    public static void StopRecording() {
        if (!isRecording || mediaRecorder == null) {
            Log.w(TAG, "Not recording!");
            return;
        }

        try {
            mediaRecorder.stop();
            mediaRecorder.release();
            mediaRecorder = null;
            isRecording = false;

            Log.i(TAG, "Recording stopped: " + currentVideoPath);
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
