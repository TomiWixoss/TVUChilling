using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ARImageTracker : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;
    private bool isProcessingOCR = false;
    
    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
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
        // Khi phát hiện ảnh mới
        foreach (var trackedImage in eventArgs.added)
        {
            Debug.Log($"Image detected: {trackedImage.referenceImage.name}");
            
            // Chỉ xử lý OCR một lần
            if (!isProcessingOCR && trackedImage.trackingState == TrackingState.Tracking)
            {
                StartCoroutine(CaptureAndProcessOCR());
            }
        }
        
        // Khi ảnh được track lại (sau khi mất)
        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                Debug.Log($"Image tracking: {trackedImage.referenceImage.name}");
            }
        }
    }
    
    IEnumerator CaptureAndProcessOCR()
    {
        isProcessingOCR = true;
        
        Debug.Log("Capturing screenshot for OCR...");
        
        // Chờ 1 frame để camera render xong
        yield return new WaitForEndOfFrame();
        
        // Capture screenshot
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        
        if (screenshot != null)
        {
            Debug.Log($"Screenshot captured: {screenshot.width}x{screenshot.height}");
            
            // Gọi OCRManager để xử lý
            if (OCRManager.Instance != null)
            {
                OCRManager.Instance.ProcessImage(screenshot);
            }
            else
            {
                Debug.LogError("OCRManager.Instance is null!");
            }
            
            // Giải phóng memory
            Destroy(screenshot);
        }
        else
        {
            Debug.LogError("Failed to capture screenshot!");
        }
        
        // Reset flag sau 3 giây (cho phép scan lại)
        yield return new WaitForSeconds(3f);
        isProcessingOCR = false;
    }
}
