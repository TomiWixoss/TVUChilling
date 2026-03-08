using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Native Video Recorder - Unity C# wrapper
/// GPU-to-GPU video encoding với zero CPU overhead
/// </summary>
public class NativeVideoRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private int videoWidth = 1920;
    [SerializeField] private int videoHeight = 1080;
    [SerializeField] private int videoFPS = 30;
    [SerializeField] private int videoBitrate = 8000000; // 8 Mbps
    
    private RenderTexture recordingRenderTexture;
    private Camera targetCamera;
    private AndroidJavaObject nativeEncoder;
    private Coroutine recordingCoroutine;
    private bool isRecording = false;
    
    void Awake()
    {
        targetCamera = Camera.main;
    }
    
    /// <summary>
    /// Start recording video
    /// </summary>
    public bool StartRecording()
    {
        if (isRecording)
        {
            Debug.LogWarning("[NativeVideoRecorder] Already recording!");
            return false;
        }
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // Create native encoder instance
            nativeEncoder = new AndroidJavaObject("com.tvu.argraduation.NativeVideoEncoder");
            
            // Initialize encoder
            bool success = nativeEncoder.Call<bool>("initialize", videoWidth, videoHeight, videoFPS, videoBitrate);
            if (!success)
            {
                Debug.LogError("[NativeVideoRecorder] Failed to initialize native encoder");
                return false;
            }
            
            // Create RenderTexture
            recordingRenderTexture = new RenderTexture(videoWidth, videoHeight, 24, RenderTextureFormat.ARGB32);
            recordingRenderTexture.Create();
            
            // Start recording coroutine
            isRecording = true;
            recordingCoroutine = StartCoroutine(RecordingLoop());
            
            Debug.Log($"[NativeVideoRecorder] Recording started: {videoWidth}x{videoHeight} @ {videoFPS}fps");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[NativeVideoRecorder] Failed to start recording: {e.Message}");
            return false;
        }
        #else
        Debug.LogWarning("[NativeVideoRecorder] Native recording only works on Android!");
        return false;
        #endif
    }
    
    /// <summary>
    /// Stop recording video
    /// </summary>
    public void StopRecording()
    {
        if (!isRecording)
        {
            Debug.LogWarning("[NativeVideoRecorder] Not recording!");
            return;
        }
        
        isRecording = false;
        
        if (recordingCoroutine != null)
        {
            StopCoroutine(recordingCoroutine);
            recordingCoroutine = null;
        }
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        if (nativeEncoder != null)
        {
            int frameCount = nativeEncoder.Call<int>("getFrameCount");
            Debug.Log($"[NativeVideoRecorder] Stopping recording ({frameCount} frames)");
            
            nativeEncoder.Call("stop");
            nativeEncoder.Dispose();
            nativeEncoder = null;
        }
        #endif
        
        if (recordingRenderTexture != null)
        {
            recordingRenderTexture.Release();
            Destroy(recordingRenderTexture);
            recordingRenderTexture = null;
        }
        
        Debug.Log("[NativeVideoRecorder] Recording stopped");
    }
    
    /// <summary>
    /// Recording loop - capture frames and send to native encoder
    /// </summary>
    IEnumerator RecordingLoop()
    {
        WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        float frameInterval = 1f / videoFPS;
        
        while (isRecording)
        {
            yield return waitForEndOfFrame;
            
            // Render camera to RenderTexture
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = recordingRenderTexture;
            
            targetCamera.targetTexture = recordingRenderTexture;
            targetCamera.Render();
            targetCamera.targetTexture = null;
            
            RenderTexture.active = currentRT;
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            // Get native texture pointer
            IntPtr texturePtr = recordingRenderTexture.GetNativeTexturePtr();
            int textureId = texturePtr.ToInt32();
            
            // Send to native encoder (GPU-to-GPU)
            if (nativeEncoder != null)
            {
                bool success = nativeEncoder.Call<bool>("encodeFrame", textureId);
                if (!success)
                {
                    Debug.LogError("[NativeVideoRecorder] Failed to encode frame");
                }
            }
            #endif
            
            // Wait to maintain FPS
            yield return new WaitForSeconds(frameInterval);
        }
    }
    
    /// <summary>
    /// Check if currently recording
    /// </summary>
    public bool IsRecording()
    {
        return isRecording;
    }
    
    void OnDestroy()
    {
        if (isRecording)
        {
            StopRecording();
        }
    }
}
