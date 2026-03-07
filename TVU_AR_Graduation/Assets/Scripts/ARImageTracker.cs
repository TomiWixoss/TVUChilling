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
    private bool isProcessingOCR = false;

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
        // Khi phát hiện ảnh mới → Spawn prefab + Trigger OCR
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

            // Trigger OCR + WebView dialog
            if (!isProcessingOCR)
            {
                StartCoroutine(ProcessOCR());
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

        // Khi mất tracking hoàn toàn → Ẩn object
        foreach (var trackedImage in eventArgs.removed)
        {
            string imageName = trackedImage.referenceImage.name;
            Debug.Log($"[AR] Image lost: {imageName}");

            if (spawnedObjects.TryGetValue(imageName, out GameObject obj))
            {
                obj.SetActive(false);
            }
        }
    }

    IEnumerator ProcessOCR()
    {
        isProcessingOCR = true;
        
        // Chờ 1 frame để camera render
        yield return new WaitForEndOfFrame();
        
        // Chụp screenshot
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        
        // Gọi ML Kit OCR
        string ocrResult = PerformOCR(screenshot);
        
        // Giải phóng memory
        Destroy(screenshot);
        
        Debug.Log($"[AR] OCR Result: {ocrResult}");
        
        // Gọi WebView để hiện dialog
        if (webViewManager != null)
        {
            webViewManager.ShowOCRDialog(ocrResult);
        }
        else
        {
            Debug.LogError("[AR] WebViewManager not found!");
        }
        
        // Reset flag sau 3 giây (tránh spam)
        yield return new WaitForSeconds(3f);
        isProcessingOCR = false;
    }
    
    string PerformOCR(Texture2D image)
    {
        // Convert Texture2D to byte array
        byte[] imageBytes = image.EncodeToJPG();
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        // Gọi ML Kit native Android
        return RecognizeTextNative(imageBytes);
        #else
        // Editor mode: Trả về tên giả
        return "NGUYỄN VĂN A";
        #endif
    }
    
    #if UNITY_ANDROID && !UNITY_EDITOR
    string RecognizeTextNative(byte[] imageBytes)
    {
        try
        {
            // Gọi Java ML Kit plugin
            using (AndroidJavaClass mlKitClass = new AndroidJavaClass("com.tvu.argraduation.MLKitTextRecognizer"))
            {
                // Initialize ML Kit
                mlKitClass.CallStatic("Initialize");
                
                // Gọi RecognizeText (async, kết quả sẽ về qua callback)
                mlKitClass.CallStatic("RecognizeText", imageBytes);
                
                // Tạm thời return placeholder (kết quả thật sẽ về qua OnTextRecognitionComplete)
                return "Đang xử lý...";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AR] ML Kit error: {e.Message}");
            return "NGUYỄN VĂN A"; // Fallback
        }
    }
    
    // Callback từ ML Kit (được gọi từ Java)
    void OnTextRecognitionComplete(string result)
    {
        Debug.Log($"[AR] ML Kit callback: {result}");
        
        if (result.StartsWith("ERROR:"))
        {
            Debug.LogError($"[AR] OCR failed: {result}");
            return;
        }
        
        string studentName = ParseStudentName(result);
        
        // Gọi WebView để hiện dialog với tên thật
        if (webViewManager != null)
        {
            webViewManager.ShowOCRDialog(studentName);
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
