using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ARImageTracker : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;
    private bool isProcessingOCR = false;
    private bool isWaitingForGoodTracking = false;
    
    [Header("Tracking Quality Settings")]
    [SerializeField] private float minTrackingQualityTime = 0.5f; // Đợi 0.5s tracking tốt
    
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
            
            // Bắt đầu đợi tracking tốt
            if (!isProcessingOCR && !isWaitingForGoodTracking)
            {
                Debug.Log("Waiting for good tracking quality...");
                isWaitingForGoodTracking = true;
                StartCoroutine(WaitForGoodTrackingAndCapture(trackedImage));
            }
        }
        
        // Khi ảnh được track lại
        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                // Nếu đang đợi tracking tốt, check quality
                if (isWaitingForGoodTracking && !isProcessingOCR)
                {
                    // ARFoundation không expose tracking quality trực tiếp
                    // Nhưng ta có thể dùng trackingState == Tracking như indicator
                    Debug.Log($"Image tracking: {trackedImage.referenceImage.name}");
                }
            }
        }
    }
    
    IEnumerator WaitForGoodTrackingAndCapture(ARTrackedImage trackedImage)
    {
        Debug.Log("Waiting for stable tracking...");
        
        // Đợi một chút để tracking ổn định
        yield return new WaitForSeconds(minTrackingQualityTime);
        
        // Check xem ảnh vẫn đang được track không
        if (trackedImage != null && trackedImage.trackingState == TrackingState.Tracking)
        {
            Debug.Log("Good tracking detected! Capturing now...");
            isWaitingForGoodTracking = false;
            isProcessingOCR = true;
            
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
            
            // Reset flag sau 5 giây (cho phép scan lại)
            yield return new WaitForSeconds(5f);
            isProcessingOCR = false;
        }
        else
        {
            Debug.LogWarning("Tracking lost before capture!");
            isWaitingForGoodTracking = false;
        }
    }
}
