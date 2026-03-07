using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AR Image Tracker - Tích hợp OCR + WebView
/// Phát hiện image target → Chụp ảnh → OCR → Hiện dialog xác nhận tên
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class ARImageTracker : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject trackedPrefab;

    [Header("WebView Integration")]
    [SerializeField] private WebViewManager webViewManager;

    private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, float> trackingStartTime = new Dictionary<string, float>();
    private bool isProcessingOCR = false;
    
    [Header("OCR Settings")]
    [SerializeField] private float minTrackingDuration = 1.5f; // Chờ 1.5s tracking ổn định
    [SerializeField] private float maxWaitTime = 4f; // Tối đa 4s

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
        
        // Tự động tìm WebViewManager nếu chưa gán
        if (webViewManager == null)
        {
            webViewManager = FindObjectOfType<WebViewManager>();
        }
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Khi phát hiện ảnh mới → Spawn prefab + Bắt đầu monitor tracking quality
        foreach (var trackedImage in eventArgs.added)
        {
            string imageName = trackedImage.referenceImage.name;
            Debug.Log($"[AR] Image detected: {imageName}");

            if (!spawnedObjects.ContainsKey(imageName))
            {
                GameObject obj = Instantiate(trackedPrefab, trackedImage.transform);
                obj.SetActive(true);
                spawnedObjects[imageName] = obj;
                Debug.Log($"[AR] Spawned object for: {imageName}");
            }

            // Bắt đầu monitor tracking quality
            if (!isProcessingOCR && !trackingStartTime.ContainsKey(imageName))
            {
                trackingStartTime[imageName] = Time.time;
                StartCoroutine(MonitorTrackingQuality(trackedImage));
            }
        }

        // Khi ảnh được update → Cập nhật visibility
        foreach (var trackedImage in eventArgs.updated)
        {
            string imageName = trackedImage.referenceImage.name;

            if (spawnedObjects.TryGetValue(imageName, out GameObject obj))
            {
                bool isTracking = trackedImage.trackingState == TrackingState.Tracking;
                obj.SetActive(isTracking);
            }
        }

        // Khi mất tracking hoàn toàn → Ẩn object + Reset timer
        foreach (var trackedImage in eventArgs.removed)
        {
            string imageName = trackedImage.referenceImage.name;
            Debug.Log($"[AR] Image lost: {imageName}");

            if (spawnedObjects.TryGetValue(imageName, out GameObject obj))
            {
                obj.SetActive(false);
            }
            
            trackingStartTime.Remove(imageName);
        }
    }
    
    IEnumerator MonitorTrackingQuality(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        float startTime = Time.time;
        float stableTrackingStart = 0f;
        bool hasStableTracking = false;
        
        Debug.Log($"[AR] Monitoring tracking quality for: {imageName}");
        
        while (Time.time - startTime < maxWaitTime)
        {
            // Check tracking state
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                // Bắt đầu đếm thời gian tracking ổn định
                if (!hasStableTracking)
                {
                    stableTrackingStart = Time.time;
                    hasStableTracking = true;
                    Debug.Log($"[AR] Stable tracking started for: {imageName}");
                }
                
                // Nếu đã tracking ổn định đủ lâu → Chụp ngay
                float stableDuration = Time.time - stableTrackingStart;
                if (stableDuration >= minTrackingDuration)
                {
                    Debug.Log($"[AR] Quality good! Stable for {stableDuration:F2}s. Capturing...");
                    trackingStartTime.Remove(imageName);
                    StartCoroutine(ProcessOCR());
                    yield break;
                }
            }
            else
            {
                // Mất tracking → Reset timer
                if (hasStableTracking)
                {
                    Debug.Log($"[AR] Lost tracking, resetting timer");
                    hasStableTracking = false;
                }
            }
            
            yield return new WaitForSeconds(0.1f); // Check mỗi 100ms
        }
        
        // Timeout: Nếu vẫn đang tracking thì chụp luôn, không thì bỏ qua
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            Debug.Log($"[AR] Timeout reached but still tracking. Capturing anyway...");
            trackingStartTime.Remove(imageName);
            StartCoroutine(ProcessOCR());
        }
        else
        {
            Debug.LogWarning($"[AR] Timeout reached with poor tracking quality. Skipping OCR.");
            trackingStartTime.Remove(imageName);
        }
    }

    IEnumerator ProcessOCR()
    {
        isProcessingOCR = true;
        
        // Chờ 1 frame để camera render
        yield return new WaitForEndOfFrame();
        
        // Chụp CHỈ camera AR (không bao gồm UI layer)
        Texture2D screenshot = CaptureARCameraOnly();
        
        if (screenshot == null)
        {
            Debug.LogError("[AR] Failed to capture camera texture");
            isProcessingOCR = false;
            yield break;
        }
        
        // Gọi ML Kit OCR (async, kết quả sẽ về qua callback)
        PerformOCR(screenshot);
        
        // Giải phóng memory
        Destroy(screenshot);
        
        Debug.Log("[AR] OCR processing started...");
        
        // Reset flag sau 5 giây (tránh spam)
        yield return new WaitForSeconds(5f);
        isProcessingOCR = false;
    }
    
    Texture2D CaptureARCameraOnly()
    {
        Camera arCamera = Camera.main;
        if (arCamera == null)
        {
            Debug.LogError("[AR] Main camera not found!");
            return null;
        }
        
        // Tạo RenderTexture tạm
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        
        // Render camera vào texture (chỉ camera layer, không có UI)
        arCamera.targetTexture = renderTexture;
        arCamera.Render();
        
        // Đọc pixels từ RenderTexture
        RenderTexture.active = renderTexture;
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();
        
        // Cleanup
        arCamera.targetTexture = null;
        RenderTexture.active = currentRT;
        Destroy(renderTexture);
        
        Debug.Log($"[AR] Captured AR camera only: {screenshot.width}x{screenshot.height}");
        return screenshot;
    }
    
    void PerformOCR(Texture2D image)
    {
        // Convert Texture2D to byte array
        byte[] imageBytes = image.EncodeToJPG();
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        // Gọi ML Kit native Android (async)
        RecognizeTextNative(imageBytes);
        #else
        // Editor mode: Log warning
        Debug.LogWarning("[AR] OCR only works on Android device. Build APK to test.");
        #endif
    }
    
    #if UNITY_ANDROID && !UNITY_EDITOR
    void RecognizeTextNative(byte[] imageBytes)
    {
        try
        {
            // Gọi Java ML Kit plugin
            using (AndroidJavaClass mlKitClass = new AndroidJavaClass("com.tvu.argraduation.MLKitTextRecognizer"))
            {
                // Initialize ML Kit
                mlKitClass.CallStatic("Initialize");
                
                // Gọi RecognizeText (async, kết quả sẽ về qua callback OnTextRecognitionComplete)
                mlKitClass.CallStatic("RecognizeText", imageBytes);
                
                Debug.Log("[AR] ML Kit processing...");
                
                // Hiện dialog "Đang xử lý..." để user biết
                if (webViewManager != null)
                {
                    webViewManager.ShowOCRDialog("Đang xử lý OCR...", "Đang chờ ML Kit...");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AR] ML Kit error: {e.Message}");
            Debug.LogError($"[AR] Stack trace: {e.StackTrace}");
            
            // Hiện dialog lỗi
            if (webViewManager != null)
            {
                webViewManager.ShowOCRDialog($"LỖI: {e.Message}", $"Exception: {e.Message}\n{e.StackTrace}");
            }
        }
    }
    
    // Callback từ ML Kit (được gọi từ Java)
    void OnTextRecognitionComplete(string result)
    {
        Debug.Log($"[AR] ML Kit callback received: {result}");
        
        if (string.IsNullOrEmpty(result))
        {
            Debug.LogError("[AR] OCR result is empty!");
            if (webViewManager != null)
            {
                webViewManager.ShowOCRDialog("LỖI: Không nhận diện được chữ", "");
            }
            return;
        }
        
        if (result.StartsWith("ERROR:"))
        {
            Debug.LogError($"[AR] OCR failed: {result}");
            if (webViewManager != null)
            {
                webViewManager.ShowOCRDialog(result, result);
            }
            return;
        }
        
        string studentName = ParseStudentName(result);
        Debug.Log($"[AR] Parsed name: {studentName}");
        
        // Gửi cả tên parsed VÀ raw OCR text để debug
        if (webViewManager != null)
        {
            webViewManager.ShowOCRDialog(studentName, result);
        }
        else
        {
            Debug.LogError("[AR] WebViewManager not found!");
        }
    }
    #endif
    
    string ParseStudentName(string ocrText)
    {
        // Parse tên từ OCR text
        // Tìm dòng sau "CỬ NHÂN" hoặc "CU NHAN"
        string[] lines = ocrText.Split('\n');
        
        for (int i = 0; i < lines.Length - 1; i++)
        {
            string line = lines[i].ToUpper();
            if (line.Contains("CỬ NHÂN") || line.Contains("CU NHAN"))
            {
                // Lấy dòng tiếp theo
                string nameCandidate = lines[i + 1].Trim();
                
                // Validate: Tên phải có ít nhất 2 từ
                if (nameCandidate.Split(' ').Length >= 2)
                {
                    return nameCandidate;
                }
            }
        }
        
        // Fallback: Trả về dòng dài nhất
        string longestLine = "";
        foreach (string line in lines)
        {
            if (line.Length > longestLine.Length)
            {
                longestLine = line;
            }
        }
        
        return longestLine.Trim();
    }
}
