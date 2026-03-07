using UnityEngine;
using System.Collections;
using System.IO;

/// <summary>
/// Camera Controller - Xử lý chụp ảnh, quay video, flash
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private string photoSavePath = "DCIM/TVU_AR";
    [SerializeField] private string videoSavePath = "DCIM/TVU_AR";
    
    private bool isRecording = false;
    private bool flashEnabled = false;
    private Camera arCamera;
    
    void Awake()
    {
        arCamera = Camera.main;
        
        // Tạo folder lưu ảnh/video
        CreateSaveFolders();
    }
    
    void CreateSaveFolders()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        // Android: Lưu vào DCIM
        string photoPath = Path.Combine(Application.persistentDataPath, photoSavePath);
        string videoPath = Path.Combine(Application.persistentDataPath, videoSavePath);
        
        if (!Directory.Exists(photoPath))
        {
            Directory.CreateDirectory(photoPath);
        }
        
        if (!Directory.Exists(videoPath))
        {
            Directory.CreateDirectory(videoPath);
        }
        
        Debug.Log($"[Camera] Photo path: {photoPath}");
        Debug.Log($"[Camera] Video path: {videoPath}");
        #endif
    }
    
    public void CapturePhoto()
    {
        Debug.Log("[Camera] Capturing photo...");
        StartCoroutine(CapturePhotoCoroutine());
    }
    
    IEnumerator CapturePhotoCoroutine()
    {
        // Chờ end of frame để capture
        yield return new WaitForEndOfFrame();
        
        // Capture screenshot
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        
        // Lưu file
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"TVU_AR_{timestamp}.jpg";
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        string fullPath = Path.Combine(Application.persistentDataPath, photoSavePath, filename);
        byte[] bytes = screenshot.EncodeToJPG(90);
        File.WriteAllBytes(fullPath, bytes);
        
        Debug.Log($"[Camera] Photo saved: {fullPath}");
        
        // Thông báo Android Media Scanner để ảnh hiện trong Gallery
        RefreshAndroidGallery(fullPath);
        #else
        // Editor: Lưu vào Assets
        string fullPath = Path.Combine(Application.dataPath, filename);
        byte[] bytes = screenshot.EncodeToJPG(90);
        File.WriteAllBytes(fullPath, bytes);
        Debug.Log($"[Camera] Photo saved (Editor): {fullPath}");
        #endif
        
        // Cleanup
        Destroy(screenshot);
        
        // Hiện toast notification
        ShowToast($"Đã lưu ảnh: {filename}");
    }
    
    public void ToggleRecording(string state)
    {
        if (state == "start")
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }
    
    void StartRecording()
    {
        if (isRecording)
        {
            Debug.LogWarning("[Camera] Already recording!");
            return;
        }
        
        isRecording = true;
        Debug.Log("[Camera] Start recording video...");
        
        // TODO: Implement video recording
        // Unity không có built-in video recording, cần dùng plugin như:
        // - NatCorder (paid)
        // - Unity Recorder (free, nhưng không support mobile tốt)
        // - Custom implementation với MediaRecorder (Android native)
        
        ShowToast("Bắt đầu quay video");
    }
    
    void StopRecording()
    {
        if (!isRecording)
        {
            Debug.LogWarning("[Camera] Not recording!");
            return;
        }
        
        isRecording = false;
        Debug.Log("[Camera] Stop recording video...");
        
        // TODO: Stop video recording và lưu file
        
        ShowToast("Đã dừng quay video");
    }
    
    public void ToggleFlash(string state)
    {
        flashEnabled = (state == "on");
        Debug.Log($"[Camera] Flash: {(flashEnabled ? "ON" : "OFF")}");
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        // Android: Bật/tắt đèn flash
        SetAndroidFlashlight(flashEnabled);
        #endif
    }
    
    #if UNITY_ANDROID && !UNITY_EDITOR
    void SetAndroidFlashlight(bool enabled)
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            using (AndroidJavaObject cameraManager = context.Call<AndroidJavaObject>("getSystemService", "camera"))
            {
                // Get camera ID
                string[] cameraIds = cameraManager.Call<string[]>("getCameraIdList");
                if (cameraIds.Length > 0)
                {
                    string cameraId = cameraIds[0];
                    cameraManager.Call("setTorchMode", cameraId, enabled);
                    Debug.Log($"[Camera] Flashlight {(enabled ? "enabled" : "disabled")}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Camera] Failed to toggle flashlight: {e.Message}");
        }
    }
    
    void RefreshAndroidGallery(string filePath)
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            {
                // Broadcast intent để refresh gallery
                using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.MEDIA_SCANNER_SCAN_FILE"))
                using (AndroidJavaClass uri = new AndroidJavaClass("android.net.Uri"))
                {
                    using (AndroidJavaObject file = new AndroidJavaObject("java.io.File", filePath))
                    using (AndroidJavaObject fileUri = uri.CallStatic<AndroidJavaObject>("fromFile", file))
                    {
                        intent.Call<AndroidJavaObject>("setData", fileUri);
                        context.Call("sendBroadcast", intent);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Camera] Failed to refresh gallery: {e.Message}");
        }
    }
    #endif
    
    void ShowToast(string message)
    {
        Debug.Log($"[Camera] Toast: {message}");
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    using (AndroidJavaClass toast = new AndroidJavaClass("android.widget.Toast"))
                    {
                        using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                        {
                            using (AndroidJavaObject toastObject = toast.CallStatic<AndroidJavaObject>("makeText", context, message, 0))
                            {
                                toastObject.Call("show");
                            }
                        }
                    }
                }));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Camera] Failed to show toast: {e.Message}");
        }
        #endif
    }
    
    void OnDestroy()
    {
        // Tắt flash khi destroy
        if (flashEnabled)
        {
            ToggleFlash("off");
        }
    }
}
