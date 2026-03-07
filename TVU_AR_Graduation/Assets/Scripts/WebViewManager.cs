using UnityEngine;
using System.Collections;

/// <summary>
/// WebView Manager - Quản lý React WebView UI
/// Load từ Vite dev server (Editor) hoặc StreamingAssets (Android)
/// </summary>
public class WebViewManager : MonoBehaviour
{
    private WebViewObject webViewObject;
    
    [Header("Development Settings")]
    #pragma warning disable 0414 // Used in Editor only
    [SerializeField] private bool useDevServer = true; // Chỉ dùng trong Editor
    #pragma warning restore 0414
    
    private const string VITE_DEV_SERVER_URL = "http://localhost:5173";
    
    [Header("Camera Controller")]
    [SerializeField] private CameraController cameraController;
    
    void Start()
    {
        // Tự động tìm CameraController nếu chưa gán
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraController>();
            
            // Nếu vẫn không có, tạo mới
            if (cameraController == null)
            {
                Debug.LogWarning("[WebView] CameraController not found in scene. Creating new one...");
                GameObject cameraControllerObj = new GameObject("CameraController");
                cameraController = cameraControllerObj.AddComponent<CameraController>();
            }
        }
        
        StartCoroutine(InitWebView());
    }
    
    IEnumerator InitWebView()
    {
        // Tạo WebViewObject
        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        
        webViewObject.Init(
            cb: (msg) =>
            {
                Debug.Log($"[WebView] Message from JS: {msg}");
                HandleMessageFromJS(msg);
            },
            err: (msg) =>
            {
                Debug.LogError($"[WebView] Error: {msg}");
            },
            started: (msg) =>
            {
                Debug.Log($"[WebView] Started: {msg}");
            },
            hooked: (msg) =>
            {
                Debug.Log($"[WebView] Hooked: {msg}");
            },
            ld: (msg) =>
            {
                Debug.Log($"[WebView] Loaded: {msg}");
                
                // Inject Unity bridge
                InjectUnityBridge();
            },
            enableWKWebView: true,
            transparent: true // UI trong suốt
        );
        
        // Set margins (fullscreen)
        webViewObject.SetMargins(0, 0, 0, 0);
        
        // Load URL
        string url = GetWebViewURL();
        Debug.Log($"[WebView] Loading URL: {url}");
        webViewObject.LoadURL(url);
        
        // Hiện WebView (để show camera controls)
        webViewObject.SetVisibility(true);
        
        yield return null;
    }
    
    string GetWebViewURL()
    {
        #if UNITY_EDITOR
        // Editor: Dùng Vite dev server
        if (useDevServer)
        {
            return VITE_DEV_SERVER_URL;
        }
        #endif
        
        // Android/iOS: Load từ StreamingAssets
        #if UNITY_ANDROID
        return "file:///android_asset/webview/index.html";
        #elif UNITY_IOS
        return Application.streamingAssetsPath + "/webview/index.html";
        #else
        return "file://" + Application.streamingAssetsPath + "/webview/index.html";
        #endif
    }
    
    void InjectUnityBridge()
    {
        // KHÔNG inject gì cả - để WebView tự nhiên
        Debug.Log("[WebView] Skipping bridge injection");
    }
    
    void HandleMessageFromJS(string message)
    {
        Debug.Log($"[WebView] Received: {message}");
        
        try
        {
            // Parse JSON message từ React
            var data = JsonUtility.FromJson<WebViewMessage>(message);
            
            switch (data.method)
            {
                case "onCapturePhoto":
                    OnCapturePhoto();
                    break;
                    
                case "onRecordToggle":
                    OnRecordToggle(data.data);
                    break;
                    
                case "onFlashToggle":
                    OnFlashToggle(data.data);
                    break;
                    
                default:
                    Debug.LogWarning($"[WebView] Unknown method: {data.method}");
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[WebView] Failed to parse message: {e.Message}");
        }
    }
    

    
    void OnCapturePhoto()
    {
        Debug.Log("[WebView] Capture photo requested");
        
        if (cameraController != null)
        {
            cameraController.CapturePhoto();
        }
        else
        {
            Debug.LogError("[WebView] CameraController not found!");
        }
    }
    
    void OnRecordToggle(string state)
    {
        Debug.Log($"[WebView] Record toggle: {state}");
        
        if (cameraController != null)
        {
            cameraController.ToggleRecording(state);
        }
        else
        {
            Debug.LogError("[WebView] CameraController not found!");
        }
    }
    
    void OnFlashToggle(string state)
    {
        Debug.Log($"[WebView] Flash toggle: {state}");
        
        if (cameraController != null)
        {
            cameraController.ToggleFlash(state);
        }
        else
        {
            Debug.LogError("[WebView] CameraController not found!");
        }
    }
    

    
    string EscapeJS(string text)
    {
        return text.Replace("'", "\\'").Replace("\n", "\\n");
    }
    
    void OnDestroy()
    {
        if (webViewObject != null)
        {
            Destroy(webViewObject.gameObject);
        }
    }
}

[System.Serializable]
public class WebViewMessage
{
    public string method;
    public string data;
}
