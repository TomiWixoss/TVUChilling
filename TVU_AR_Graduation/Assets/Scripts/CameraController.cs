using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using System.IO;

/// <summary>
/// Camera Controller - Xử lý chụp ảnh, quay video, flash
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Recording Settings")]
    #pragma warning disable 0414 // Suppress "assigned but never used" warning (used in Android build)
    [SerializeField] private int videoWidth = 1920;
    [SerializeField] private int videoHeight = 1080;
    [SerializeField] private int videoFPS = 30;
    #pragma warning restore 0414
    
    private bool isRecording = false;
    private bool flashEnabled = false;
    private Camera arCamera;
    private ARCameraManager arCameraManager;
    private float recordingStartTime;
    private Coroutine recordingCoroutine;
    private System.Collections.Generic.List<Texture2D> recordedFrames;
    
    void Awake()
    {
        arCamera = Camera.main;
        arCameraManager = FindFirstObjectByType<ARCameraManager>();
        
        if (arCameraManager == null)
        {
            Debug.LogWarning("[Camera] ARCameraManager not found! Flash will not work.");
        }
        
        // Tạo folder lưu ảnh/video
        CreateSaveFolders();
        
        // Request permissions
        RequestPermissions();
    }
    
    void RequestPermissions()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
        }
        
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
        }
        
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);
        }
        #endif
    }
    
    void CreateSaveFolders()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        // Android: Lưu vào DCIM public (hiện trong Gallery)
        // Sử dụng Android MediaStore API
        Debug.Log("[Camera] Using public DCIM folder");
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
        // Lưu vào public DCIM folder
        string dcimPath = "/storage/emulated/0/DCIM/TVU_AR";
        
        // Tạo folder nếu chưa có
        try
        {
            using (AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment"))
            using (AndroidJavaObject dcimDir = environment.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", "DCIM"))
            {
                string dcimDirPath = dcimDir.Call<string>("getAbsolutePath");
                dcimPath = Path.Combine(dcimDirPath, "TVU_AR");
                
                if (!Directory.Exists(dcimPath))
                {
                    Directory.CreateDirectory(dcimPath);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Camera] Failed to create DCIM folder: {e.Message}");
            dcimPath = "/storage/emulated/0/DCIM/TVU_AR";
        }
        
        string fullPath = Path.Combine(dcimPath, filename);
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
        recordingStartTime = Time.time;
        Debug.Log("[Camera] Start recording video...");
        ShowToast("Bắt đầu quay video");
        
        // Video recording sẽ được implement sau
        // Hiện tại chỉ track state
    }
    
    void StopRecording()
    {
        if (!isRecording)
        {
            Debug.LogWarning("[Camera] Not recording!");
            return;
        }
        
        isRecording = false;
        float duration = Time.time - recordingStartTime;
        Debug.Log($"[Camera] Stop recording video (duration: {duration:F1}s)...");
        ShowToast($"Đã dừng quay video ({duration:F0}s)");
        
        // Video recording sẽ được implement sau
    }
    
    // Callbacks từ Android plugin
    void OnRecordingStarted(string filePath)
    {
        Debug.Log($"[Camera] Recording started: {filePath}");
        ShowToast("Bắt đầu quay video");
    }
    
    void OnRecordingStopped(string filePath)
    {
        Debug.Log($"[Camera] Recording stopped: {filePath}");
        ShowToast($"Đã lưu video");
    }
    
    void OnRecordingError(string error)
    {
        Debug.LogError($"[Camera] Recording error: {error}");
        isRecording = false;
        ShowToast($"Lỗi: {error}");
    }
    
    public void ToggleFlash(string state)
    {
        flashEnabled = (state == "on");
        Debug.Log($"[Camera] Flash: {(flashEnabled ? "ON" : "OFF")}");
        
        // AR Foundation 6.x: Dùng XRCameraSubsystem để control torch
        var loader = XRGeneralSettings.Instance?.Manager?.activeLoader;
        if (loader != null)
        {
            var cameraSubsystem = loader.GetLoadedSubsystem<XRCameraSubsystem>();
            if (cameraSubsystem != null && cameraSubsystem.running)
            {
                if (cameraSubsystem.DoesCurrentCameraSupportTorch())
                {
                    cameraSubsystem.requestedCameraTorchMode = flashEnabled 
                        ? XRCameraTorchMode.On 
                        : XRCameraTorchMode.Off;
                    
                    ShowToast(flashEnabled ? "Flash bật" : "Flash tắt");
                    Debug.Log($"[Camera] Torch mode set to: {cameraSubsystem.requestedCameraTorchMode}");
                }
                else
                {
                    Debug.LogWarning("[Camera] Current camera does not support torch");
                    ShowToast("Camera không hỗ trợ flash");
                }
            }
            else
            {
                Debug.LogWarning("[Camera] XRCameraSubsystem not running");
                ShowToast("Flash không khả dụng");
            }
        }
        else
        {
            Debug.LogWarning("[Camera] XR Loader not found");
            ShowToast("Flash không khả dụng");
        }
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
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaClass toast = new AndroidJavaClass("android.widget.Toast");
                AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
                AndroidJavaObject toastObject = toast.CallStatic<AndroidJavaObject>("makeText", context, message, 0);
                toastObject.Call("show");
            }));
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
