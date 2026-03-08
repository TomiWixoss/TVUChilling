using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Rendering;
using System.Collections;
using System.IO;

/// <summary>
/// Camera Controller - Flash (Unity AR), Photo (AsyncGPU), Video (Native C++)
/// </summary>
public class CameraController : MonoBehaviour
{
    private bool flashEnabled = false;
    private Camera arCamera;
    private ARCameraManager arCameraManager;
    private NativeVideoRecorder videoRecorder;
    
    void Awake()
    {
        arCamera = Camera.main;
        arCameraManager = FindFirstObjectByType<ARCameraManager>();
        
        if (arCameraManager == null)
        {
            Debug.LogWarning("[Camera] ARCameraManager not found!");
        }
        
        // Setup native video recorder
        videoRecorder = gameObject.AddComponent<NativeVideoRecorder>();
        
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
        
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);
        }
        #endif
    }
    
    public void CapturePhoto()
    {
        Debug.Log("[Camera] Capturing photo with AsyncGPUReadback...");
        StartCoroutine(CapturePhotoAsync());
    }
    
    IEnumerator CapturePhotoAsync()
    {
        // Wait for end of frame
        yield return new WaitForEndOfFrame();
        
        // Capture screenshot
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        
        // Request async GPU readback (KHÔNG block main thread)
        AsyncGPUReadback.Request(screenshot, 0, (request) =>
        {
            if (request.hasError)
            {
                Debug.LogError("[Camera] AsyncGPUReadback error!");
                Destroy(screenshot);
                return;
            }
            
            // Save ảnh trong background thread
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string filename = $"TVU_AR_{timestamp}.jpg";
                    
                    #if UNITY_ANDROID && !UNITY_EDITOR
                    string dcimPath = "/storage/emulated/0/DCIM/TVU_AR";
                    if (!Directory.Exists(dcimPath))
                    {
                        Directory.CreateDirectory(dcimPath);
                    }
                    string fullPath = Path.Combine(dcimPath, filename);
                    #else
                    string fullPath = Path.Combine(Application.persistentDataPath, filename);
                    #endif
                    
                    // Encode to JPEG
                    byte[] bytes = screenshot.EncodeToJPG(90);
                    File.WriteAllBytes(fullPath, bytes);
                    
                    // Callback to main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        Debug.Log($"[Camera] Photo saved: {fullPath}");
                        ShowToast("Đã lưu ảnh");
                        Destroy(screenshot);
                        
                        #if UNITY_ANDROID && !UNITY_EDITOR
                        RefreshAndroidGallery(fullPath);
                        #endif
                    });
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Camera] Failed to save photo: {e.Message}");
                    UnityMainThreadDispatcher.Instance().Enqueue(() => Destroy(screenshot));
                }
            });
        });
    }
    
    public void ToggleRecording(string state)
    {
        if (state == "start")
        {
            bool success = videoRecorder.StartRecording();
            if (success)
            {
                ShowToast("Bắt đầu quay video");
            }
            else
            {
                ShowToast("Lỗi khởi động video");
            }
        }
        else
        {
            videoRecorder.StopRecording();
            ShowToast("Đã lưu video");
        }
    }
    
    public void ToggleFlash(string state)
    {
        flashEnabled = (state == "on");
        Debug.Log($"[Camera] Flash: {(flashEnabled ? "ON" : "OFF")}");
        
        // Dùng Unity AR Foundation XRCameraSubsystem
        var loader = XRGeneralSettings.Instance?.Manager?.activeLoader;
        if (loader != null)
        {
            var cameraSubsystem = loader.GetLoadedSubsystem<XRCameraSubsystem>();
            if (cameraSubsystem != null && cameraSubsystem.running)
            {
                // Check if torch is supported
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
                    Debug.LogWarning("[Camera] Camera does not support torch");
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
        
        // Stop recording nếu đang quay
        if (videoRecorder != null && videoRecorder.IsRecording())
        {
            videoRecorder.StopRecording();
        }
    }
}
