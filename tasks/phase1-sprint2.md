# PHASE 1 - SPRINT 2: OCR INTEGRATION (Tuần 4)
**Mục tiêu:** Đọc tên sinh viên trên cúp

---

## BƯỚC 1: CÀI ĐẶT ML KIT PLUGIN

### Download plugin
- [ ] Tìm "Unity ML Kit Text Recognition" trên GitHub
- [ ] Hoặc dùng plugin "NativeGallery" + wrapper ML Kit
- [ ] Download file .unitypackage

### Import vào Unity
- [ ] Assets > Import Package > Custom Package
- [ ] Chọn file .unitypackage vừa download
- [ ] Click "Import All"
- [ ] Verify: Có thư mục `Plugins/Android/mlkit`

---

## BƯỚC 2: CAPTURE FRAME KHI DETECT IMAGE

### Sửa script ARImageTracker.cs

```csharp
// Thêm vào đầu file
using System.Collections;

// Thêm biến
private bool isProcessingOCR = false;

// Sửa method OnTrackedImagesChanged
void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
{
    foreach (var trackedImage in eventArgs.added)
    {
        if (!isProcessingOCR)
        {
            StartCoroutine(CaptureAndProcessOCR());
        }
    }
}

// Thêm method mới
IEnumerator CaptureAndProcessOCR()
{
    isProcessingOCR = true;
    
    // Chờ 1 frame để camera render
    yield return new WaitForEndOfFrame();
    
    // Capture screenshot
    Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
    
    // Gọi OCR (sẽ implement ở bước tiếp)
    ProcessOCR(screenshot);
    
    Destroy(screenshot); // Giải phóng memory
}
```
- [ ] Copy code trên vào `ARImageTracker.cs`
- [ ] Save file

---

## BƯỚC 3: GỌI ML KIT OCR

### Tạo script OCRManager.cs
- [ ] `Assets/Scripts/` > Chuột phải > Create > C# Script
- [ ] Đặt tên: `OCRManager`

### Viết code OCR
```csharp
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
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ProcessImage(Texture2D image)
    {
        // Convert Texture2D to byte array
        byte[] imageBytes = image.EncodeToJPG();
        
        // Call native Android ML Kit
        #if UNITY_ANDROID && !UNITY_EDITOR
        RecognizeTextNative(imageBytes);
        #else
        // Test mode in Editor
        OnTextRecognized?.Invoke("NGUYỄN VĂN A");
        #endif
    }
    
    #if UNITY_ANDROID && !UNITY_EDITOR
    private void RecognizeTextNative(byte[] imageBytes)
    {
        // TODO: Implement native call to ML Kit
        // Sẽ cần viết Java/Kotlin plugin
    }
    #endif
    
    // Callback từ native code
    public void OnTextRecognitionComplete(string recognizedText)
    {
        Debug.Log($"OCR Result: {recognizedText}");
        OnTextRecognized?.Invoke(recognizedText);
    }
}
```
- [ ] Copy code vào `OCRManager.cs`
- [ ] Save file

### Tạo GameObject cho OCRManager
- [ ] Hierarchy > Chuột phải > Create Empty
- [ ] Đặt tên: `OCRManager`
- [ ] Add Component: `OCRManager` script

---

## BƯỚC 4: PARSE TÊN SINH VIÊN

### Tạo script NameParser.cs
- [ ] `Assets/Scripts/` > Create > C# Script
- [ ] Đặt tên: `NameParser`

### Viết logic parse
```csharp
using System.Text.RegularExpressions;

public static class NameParser
{
    public static string ExtractStudentName(string ocrText)
    {
        // Tách thành các dòng
        string[] lines = ocrText.Split('\n');
        
        // Tìm dòng chứa "Cử nhân" hoặc "CỬ NHÂN"
        for (int i = 0; i < lines.Length - 1; i++)
        {
            string line = lines[i].ToUpper();
            if (line.Contains("CỬ NHÂN") || line.Contains("CU NHAN"))
            {
                // Lấy dòng tiếp theo (thường là tên)
                string nameCandidate = lines[i + 1].Trim();
                
                // Validate: Tên phải có ít nhất 2 từ
                if (nameCandidate.Split(' ').Length >= 2)
                {
                    return nameCandidate;
                }
            }
        }
        
        // Fallback: Trả về dòng dài nhất (thường là tên)
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
```
- [ ] Copy code vào `NameParser.cs`
- [ ] Save file

---

## BƯỚC 5: TẠO POPUP XÁC NHẬN

### Tạo Canvas UI
- [ ] Hierarchy > Chuột phải > UI > Canvas
- [ ] Đặt tên: `ConfirmationUI`
- [ ] Canvas Scaler:
  - [ ] UI Scale Mode: **Scale With Screen Size**
  - [ ] Reference Resolution: **1080 x 1920**

### Tạo Panel background
- [ ] Chuột phải vào Canvas > UI > Panel
- [ ] Đặt tên: `PopupPanel`
- [ ] Rect Transform: Stretch (full screen)
- [ ] Image component:
  - [ ] Color: Đen, Alpha = 200 (tối mờ)

### Tạo Popup content
- [ ] Chuột phải vào PopupPanel > UI > Panel
- [ ] Đặt tên: `ContentPanel`
- [ ] Rect Transform:
  - [ ] Width: 800
  - [ ] Height: 600
  - [ ] Anchor: Center
- [ ] Image: Trắng, Alpha = 255

### Thêm Text tiêu đề
- [ ] Chuột phải vào ContentPanel > UI > Text - TextMeshPro
- [ ] Đặt tên: `TitleText`
- [ ] Text: "Hệ thống nhận diện bạn là:"
- [ ] Font Size: 48
- [ ] Alignment: Center
- [ ] Position: Top của ContentPanel

### Thêm InputField
- [ ] Chuột phải vào ContentPanel > UI > InputField - TextMeshPro
- [ ] Đặt tên: `NameInputField`
- [ ] Placeholder: "Nhập tên của bạn"
- [ ] Font Size: 40
- [ ] Position: Center của ContentPanel

### Thêm Button "Kích hoạt"
- [ ] Chuột phải vào ContentPanel > UI > Button - TextMeshPro
- [ ] Đặt tên: `ConfirmButton`
- [ ] Text: "Kích hoạt"
- [ ] Font Size: 36
- [ ] Position: Bottom left của ContentPanel

### Thêm Button "Hủy"
- [ ] Duplicate ConfirmButton
- [ ] Đặt tên: `CancelButton`
- [ ] Text: "Hủy"
- [ ] Position: Bottom right của ContentPanel

### Ẩn popup mặc định
- [ ] Chọn `PopupPanel`
- [ ] Inspector: Bỏ tick ở checkbox bên cạnh tên (disable)

---

## BƯỚC 6: VIẾT SCRIPT POPUP MANAGER

### Tạo script PopupManager.cs
```csharp
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    private string recognizedName;
    
    void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        
        // Subscribe to OCR event
        OCRManager.Instance.OnTextRecognized += ShowPopup;
    }
    
    void ShowPopup(string ocrText)
    {
        // Parse tên từ OCR text
        recognizedName = NameParser.ExtractStudentName(ocrText);
        
        // Hiển thị trong InputField
        nameInputField.text = recognizedName;
        
        // Show popup
        popupPanel.SetActive(true);
    }
    
    void OnConfirmClicked()
    {
        string finalName = nameInputField.text;
        Debug.Log($"User confirmed name: {finalName}");
        
        // TODO: Trigger AR animation với tên này
        
        // Hide popup
        popupPanel.SetActive(false);
    }
    
    void OnCancelClicked()
    {
        Debug.Log("User cancelled");
        popupPanel.SetActive(false);
    }
}
```
- [ ] Copy code vào `PopupManager.cs`

### Gắn script và references
- [ ] Chọn Canvas `ConfirmationUI`
- [ ] Add Component: `PopupManager`
- [ ] Kéo các UI elements vào fields:
  - [ ] Popup Panel: `PopupPanel`
  - [ ] Name Input Field: `NameInputField`
  - [ ] Confirm Button: `ConfirmButton`
  - [ ] Cancel Button: `CancelButton`

---

## BƯỚC 7: TÍCH HỢP VÀO AR TRACKER

### Sửa ARImageTracker.cs
```csharp
// Thêm vào method CaptureAndProcessOCR
IEnumerator CaptureAndProcessOCR()
{
    isProcessingOCR = true;
    yield return new WaitForEndOfFrame();
    
    Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
    
    // Gọi OCRManager
    OCRManager.Instance.ProcessImage(screenshot);
    
    Destroy(screenshot);
}
```
- [ ] Update code
- [ ] Save file

---

## BƯỚC 8: TEST

### Test trong Unity Editor
- [ ] Play mode
- [ ] Verify: Popup hiện với tên test "NGUYỄN VĂN A"
- [ ] Sửa tên trong InputField
- [ ] Click "Kích hoạt" → Popup đóng
- [ ] Check Console: Log "User confirmed name: ..."

### Test trên thiết bị (Mock OCR)
- [ ] Build and Run
- [ ] Quét ảnh cúp
- [ ] Popup phải hiện với tên test
- [ ] Test UI: InputField, buttons hoạt động

### Test OCR thật (nếu đã có plugin)
- [ ] Quét cúp thật có tên
- [ ] Verify: OCR đọc được tên
- [ ] Nếu sai: Sửa trong InputField
- [ ] Confirm

---

## CHECKLIST SPRINT 2

- [ ] ML Kit plugin đã cài (hoặc mock sẵn sàng)
- [ ] ARImageTracker capture screenshot khi detect
- [ ] OCRManager xử lý ảnh và trả về text
- [ ] NameParser parse tên từ OCR text
- [ ] Popup UI đã tạo với InputField và buttons
- [ ] PopupManager hiển thị popup với tên OCR
- [ ] User có thể sửa tên và confirm
- [ ] Không có lỗi trong Console
- [ ] Test thành công trên thiết bị

### Commit code
```bash
git add .
git commit -m "Sprint 2 complete: OCR integration with confirmation popup"
git push origin dev
```
- [ ] Chạy lệnh commit

**✅ Sprint 2 hoàn thành → Chuyển sang `phase1-sprint3.md`**
