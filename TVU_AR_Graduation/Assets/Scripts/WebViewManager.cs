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
    [SerializeField] private string viteDevServerURL = "http://localhost:5173";
    [SerializeField] private bool useDevServer = true; // Chỉ dùng trong Editor
    
    void Start()
    {
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
                
                // Sau khi load xong, inject Unity bridge
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
        
        // Ẩn WebView ban đầu, chỉ hiện khi cần
        webViewObject.SetVisibility(false);
        
        yield return null;
    }
    
    string GetWebViewURL()
    {
        #if UNITY_EDITOR
        // Editor: Dùng Vite dev server
        if (useDevServer)
        {
            return viteDevServerURL;
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
        // Inject Unity bridge để JS có thể gọi Unity
        string js = @"
            window.Unity = {
                call: function(method, data) {
                    window.unity.call(JSON.stringify({method: method, data: data}));
                }
            };
        ";
        webViewObject.EvaluateJS(js);
    }
    
    void HandleMessageFromJS(string message)
    {
        try
        {
            // Parse JSON message từ React
            var data = JsonUtility.FromJson<WebViewMessage>(message);
            
            switch (data.method)
            {
                case "onNameConfirmed":
                    OnNameConfirmed(data.data);
                    break;
                    
                case "onDialogClosed":
                    Debug.Log("[WebView] Dialog closed");
                    HideWebView(); // Ẩn WebView khi đóng dialog
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
    
    public void ShowOCRDialog(string studentName)
    {
        if (webViewObject == null)
        {
            Debug.LogError("[WebView] WebViewObject not initialized!");
            return;
        }
        
        // Hiện WebView
        webViewObject.SetVisibility(true);
        
        // Gọi JS function để hiện dialog
        string js = $"window.showOCRDialog('{EscapeJS(studentName)}')";
        Debug.Log($"[WebView] Calling JS: {js}");
        webViewObject.EvaluateJS(js);
    }
    
    public void HideWebView()
    {
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false);
        }
    }
    
    void OnNameConfirmed(string confirmedName)
    {
        Debug.Log($"[WebView] Name confirmed: {confirmedName}");
        
        // TODO: Lưu tên vào database hoặc xử lý tiếp
        // Ví dụ: Hiện 3D model với tên này
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
