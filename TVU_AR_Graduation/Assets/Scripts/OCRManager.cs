using UnityEngine;
using System;

public class OCRManager : MonoBehaviour
{
    public static OCRManager Instance { get; private set; }
    
    public event Action<string> OnTextRecognized;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        #if UNITY_EDITOR
        // Auto-trigger OCR sau 2 giây trong Editor để test UI
        Invoke(nameof(TestOCRInEditor), 2f);
        #endif
    }
    
    #if UNITY_EDITOR
    void TestOCRInEditor()
    {
        Debug.Log("Testing OCR in Editor...");
        string mockOCRText = @"TRƯỜNG ĐẠI HỌC TRÀ VINH
CỬ NHÂN
NGUYỄN VĂN A
KHOA CÔNG NGHỆ THÔNG TIN";
        OnTextRecognitionComplete(mockOCRText);
    }
    #endif
    
    public void ProcessImage(Texture2D image)
    {
        // Convert Texture2D to byte array
        byte[] imageBytes = image.EncodeToJPG();
        
        // Call native Android ML Kit
        #if UNITY_ANDROID && !UNITY_EDITOR
        RecognizeTextNative(imageBytes);
        #else
        // Test mode in Editor - Mock OCR result
        string mockOCRText = @"TRƯỜNG ĐẠI HỌC TRÀ VINH
CỬ NHÂN
NGUYỄN VĂN A
KHOA CÔNG NGHỆ THÔNG TIN";
        OnTextRecognitionComplete(mockOCRText);
        #endif
    }
    
    #if UNITY_ANDROID && !UNITY_EDITOR
    private void RecognizeTextNative(byte[] imageBytes)
    {
        try
        {
            // Call Java plugin
            using (AndroidJavaClass mlkitClass = new AndroidJavaClass("com.tvu.argraduation.MLKitTextRecognizer"))
            {
                mlkitClass.CallStatic("RecognizeText", imageBytes);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to call ML Kit plugin: {e.Message}");
            // Fallback to mock
            OnTextRecognitionComplete("ERROR: ML Kit plugin failed");
        }
    }
    #endif
    
    // Callback từ native code
    public void OnTextRecognitionComplete(string recognizedText)
    {
        Debug.Log($"OCR Result: {recognizedText}");
        OnTextRecognized?.Invoke(recognizedText);
    }
}
