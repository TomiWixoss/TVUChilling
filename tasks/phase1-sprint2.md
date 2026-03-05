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

## BƯỚC 5: TẠO POPUP XÁC NHẬN (UI TOOLKIT)

### Tạo UXML (UI Structure)
- [ ] Project window > `Assets/UI/` (tạo folder nếu chưa có)
- [ ] Chuột phải > Create > UI Toolkit > UI Document
- [ ] Đặt tên: `ConfirmationPopup.uxml`
- [ ] Double click để mở UI Builder

### Thiết kế UI trong UI Builder
- [ ] Thêm **VisualElement** (root container):
  - [ ] Name: `popup-overlay`
  - [ ] Style:
    - [ ] Position: Absolute
    - [ ] Width/Height: 100%
    - [ ] Background Color: rgba(0, 0, 0, 0.8)
    - [ ] Display: None (ẩn mặc định)

- [ ] Thêm **VisualElement** con (content panel):
  - [ ] Name: `popup-content`
  - [ ] Style:
    - [ ] Width: 800px
    - [ ] Height: 600px
    - [ ] Align: Center
    - [ ] Background Color: White
    - [ ] Border Radius: 20px
    - [ ] Padding: 40px

- [ ] Thêm **Label** (tiêu đề):
  - [ ] Name: `title-label`
  - [ ] Text: "Hệ thống nhận diện bạn là:"
  - [ ] Style:
    - [ ] Font Size: 48px
    - [ ] Text Align: Center
    - [ ] Margin Bottom: 40px

- [ ] Thêm **TextField** (input tên):
  - [ ] Name: `name-input`
  - [ ] Placeholder: "Nhập tên của bạn"
  - [ ] Style:
    - [ ] Font Size: 40px
    - [ ] Height: 80px
    - [ ] Margin Bottom: 40px

- [ ] Thêm **VisualElement** (button container):
  - [ ] Name: `button-container`
  - [ ] Style:
    - [ ] Flex Direction: Row
    - [ ] Justify Content: Space Between

- [ ] Thêm **Button** (Kích hoạt):
  - [ ] Name: `confirm-button`
  - [ ] Text: "Kích hoạt"
  - [ ] Style:
    - [ ] Width: 350px
    - [ ] Height: 100px
    - [ ] Font Size: 36px
    - [ ] Background Color: Green

- [ ] Thêm **Button** (Hủy):
  - [ ] Name: `cancel-button`
  - [ ] Text: "Hủy"
  - [ ] Style:
    - [ ] Width: 350px
    - [ ] Height: 100px
    - [ ] Font Size: 36px
    - [ ] Background Color: Red

- [ ] Save UXML (Ctrl+S)

### Tạo USS (Styling - Optional)
- [ ] Project window > `Assets/UI/`
- [ ] Chuột phải > Create > UI Toolkit > Style Sheet
- [ ] Đặt tên: `ConfirmationPopup.uss`
- [ ] Thêm styles (nếu cần custom thêm):

```css
.popup-overlay {
    position: absolute;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.8);
    align-items: center;
    justify-content: center;
}

.popup-content {
    width: 800px;
    height: 600px;
    background-color: white;
    border-radius: 20px;
    padding: 40px;
}

.title-label {
    font-size: 48px;
    -unity-text-align: middle-center;
    margin-bottom: 40px;
}

.name-input {
    font-size: 40px;
    height: 80px;
    margin-bottom: 40px;
}

.button-container {
    flex-direction: row;
    justify-content: space-between;
}

.confirm-button {
    width: 350px;
    height: 100px;
    font-size: 36px;
    background-color: rgb(76, 175, 80);
}

.cancel-button {
    width: 350px;
    height: 100px;
    font-size: 36px;
    background-color: rgb(244, 67, 54);
}
```
- [ ] Save USS

### Thêm UI Document vào Scene
- [ ] Hierarchy > Chuột phải > UI Toolkit > UI Document
- [ ] Đặt tên: `ConfirmationUI`
- [ ] Inspector > Source Asset: Kéo `ConfirmationPopup.uxml` vào
- [ ] (Optional) Style Sheet: Kéo `ConfirmationPopup.uss` vào nếu có

---

## BƯỚC 6: VIẾT SCRIPT POPUP MANAGER (UI TOOLKIT)

### Tạo script PopupManager.cs
```csharp
using UnityEngine;
using UnityEngine.UIElements;

public class PopupManager : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement popupOverlay;
    private TextField nameInput;
    private Button confirmButton;
    private Button cancelButton;
    
    private string recognizedName;
    
    void Start()
    {
        // Get UI Document
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        
        // Query UI elements
        popupOverlay = root.Q<VisualElement>("popup-overlay");
        nameInput = root.Q<TextField>("name-input");
        confirmButton = root.Q<Button>("confirm-button");
        cancelButton = root.Q<Button>("cancel-button");
        
        // Register button callbacks
        confirmButton.clicked += OnConfirmClicked;
        cancelButton.clicked += OnCancelClicked;
        
        // Subscribe to OCR event
        OCRManager.Instance.OnTextRecognized += ShowPopup;
        
        // Hide popup initially
        HidePopup();
    }
    
    void ShowPopup(string ocrText)
    {
        // Parse tên từ OCR text
        recognizedName = NameParser.ExtractStudentName(ocrText);
        
        // Set text in input field
        nameInput.value = recognizedName;
        
        // Show popup (change display from none to flex)
        popupOverlay.style.display = DisplayStyle.Flex;
    }
    
    void HidePopup()
    {
        popupOverlay.style.display = DisplayStyle.None;
    }
    
    void OnConfirmClicked()
    {
        string finalName = nameInput.value;
        Debug.Log($"User confirmed name: {finalName}");
        
        // TODO: Trigger AR animation với tên này
        
        HidePopup();
    }
    
    void OnCancelClicked()
    {
        Debug.Log("User cancelled");
        HidePopup();
    }
    
    void OnDestroy()
    {
        // Unregister callbacks
        if (confirmButton != null)
            confirmButton.clicked -= OnConfirmClicked;
        if (cancelButton != null)
            cancelButton.clicked -= OnCancelClicked;
    }
}
```
- [ ] Copy code vào `PopupManager.cs`
- [ ] Save file

### Gắn script vào UI Document
- [ ] Chọn GameObject `ConfirmationUI` trong Hierarchy
- [ ] Add Component: `PopupManager`
- [ ] Verify: UIDocument component đã có Source Asset = `ConfirmationPopup.uxml`

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
