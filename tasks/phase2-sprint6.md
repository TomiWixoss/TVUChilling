# PHASE 2 - SPRINT 6: CAPTURE SYSTEM (Tuần 8)
**Mục tiêu:** Chụp ảnh và quay video

---

## BƯỚC 1: CÀI ĐẶT NATCORDER (HOẶC UNITY RECORDER)

### Option A: NatCorder (Recommended)
- [ ] Truy cập Unity Asset Store
- [ ] Tìm "NatCorder" (có free trial)
- [ ] Download và Import
- [ ] Verify: Có thư mục `NatCorder/` trong Assets

### Option B: Unity Recorder (Free)
- [ ] Window > Package Manager
- [ ] Tìm "Unity Recorder"
- [ ] Click Install
- [ ] Verify: Package đã cài

---

## BƯỚC 2: CÀI ĐẶT NATIVE GALLERY

### Download plugin
- [ ] Tìm "NativeGallery" trên GitHub
- [ ] Hoặc Unity Asset Store
- [ ] Download file .unitypackage

### Import
- [ ] Assets > Import Package > Custom Package
- [ ] Chọn NativeGallery.unitypackage
- [ ] Import All

### Verify
- [ ] Có thư mục `Plugins/NativeGallery/`
- [ ] Có file Android manifest

---

## BƯỚC 3: XIN QUYỀN STORAGE

### Thêm vào AndroidManifest.xml
- [ ] Mở `Assets/Plugins/Android/AndroidManifest.xml`
- [ ] Thêm permissions:
```xml
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE"/>
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE"/>
<uses-permission android:name="android.permission.CAMERA"/>
```
- [ ] Save file

### Tạo script PermissionManager.cs
```csharp
using UnityEngine;
using UnityEngine.Android;

public class PermissionManager : MonoBehaviour
{
    void Start()
    {
        RequestPermissions();
    }
    
    void RequestPermissions()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
        
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
    }
}
```
- [ ] Tạo script
- [ ] Gắn vào GameObject persistent (ví dụ: XR Origin)

---

## BƯỚC 4: IMPLEMENT CHỤP ẢNH

### Sửa CaptureController.cs
```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class CaptureController : MonoBehaviour
{
    [SerializeField] private GameObject flashPanel;
    [SerializeField] private AudioClip shutterSound;
    
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }
    
    public void TakePhoto()
    {
        StartCoroutine(CapturePhoto());
    }
    
    IEnumerator CapturePhoto()
    {
        // Flash effect
        flashPanel.SetActive(true);
        
        // Play shutter sound
        if (shutterSound != null)
        {
            audioSource.PlayOneShot(shutterSound);
        }
        
        yield return new WaitForEndOfFrame();
        
        // Capture screenshot
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        
        // Hide flash
        flashPanel.SetActive(false);
        
        // Save to Gallery
        SaveToGallery(screenshot);
        
        // Cleanup
        Destroy(screenshot);
    }
    
    void SaveToGallery(Texture2D texture)
    {
        string filename = $"AR_Graduation_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        NativeGallery.SaveImageToGallery(texture, "AR Graduation", filename, 
            (success, path) => {
                if (success)
                {
                    Debug.Log($"Photo saved: {path}");
                    ShowToast("Ảnh đã lưu vào thư viện");
                }
                else
                {
                    Debug.LogError("Failed to save photo");
                    ShowToast("Lỗi: Không thể lưu ảnh");
                }
            });
        #else
        // Editor mode: Save to Application.persistentDataPath
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Debug.Log($"Photo saved (Editor): {path}");
        #endif
    }
    
    void ShowToast(string message)
    {
        // TODO: Implement toast notification
        Debug.Log($"Toast: {message}");
    }
}
```
- [ ] Update code
- [ ] Gán flashPanel reference
- [ ] Tìm shutter sound effect và gán

---

## BƯỚC 5: TẠO TOAST NOTIFICATION

### Tạo Toast UI
- [ ] Canvas > Chuột phải > UI > Panel
- [ ] Đặt tên: `ToastPanel`
- [ ] Rect Transform:
  - [ ] Anchor: Bottom Center
  - [ ] Width: 600, Height: 100
  - [ ] Pos Y: 200
- [ ] Image: Đen, Alpha: 200

### Thêm Text
- [ ] Chuột phải vào ToastPanel > UI > Text - TextMeshPro
- [ ] Đặt tên: `ToastText`
- [ ] Font Size: 32
- [ ] Alignment: Center
- [ ] Color: Trắng

### Ẩn mặc định
- [ ] ToastPanel > Disable

### Script ToastManager.cs
```csharp
using UnityEngine;
using TMPro;
using System.Collections;

public class ToastManager : MonoBehaviour
{
    public static ToastManager Instance { get; private set; }
    
    [SerializeField] private GameObject toastPanel;
    [SerializeField] private TextMeshProUGUI toastText;
    
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
    
    public void Show(string message, float duration = 2f)
    {
        StartCoroutine(ShowToast(message, duration));
    }
    
    IEnumerator ShowToast(string message, float duration)
    {
        toastText.text = message;
        toastPanel.SetActive(true);
        
        yield return new WaitForSeconds(duration);
        
        toastPanel.SetActive(false);
    }
}
```
- [ ] Tạo script
- [ ] Gắn vào Canvas
- [ ] Gán references

### Update CaptureController
```csharp
void ShowToast(string message)
{
    ToastManager.Instance.Show(message);
}
```
- [ ] Update method

---

## BƯỚC 6: IMPLEMENT QUAY VIDEO

### Sử dụng NatCorder
```csharp
using NatCorder;
using NatCorder.Clocks;
using NatCorder.Inputs;

public class CaptureController : MonoBehaviour
{
    private MediaRecorder recorder;
    private CameraInput cameraInput;
    private bool isRecording = false;
    
    public void StartRecording()
    {
        if (isRecording) return;
        
        // Create recorder
        var clock = new RealtimeClock();
        recorder = MediaRecorder.Create(
            MediaRecorder.Format.MP4,
            Screen.width,
            Screen.height,
            30, // FPS
            AudioSettings.outputSampleRate,
            (int)AudioSettings.speakerMode,
            OnRecordingComplete
        );
        
        // Create camera input
        cameraInput = new CameraInput(recorder, clock, Camera.main);
        
        // Start recording
        isRecording = true;
        Debug.Log("Recording started");
        
        // Update UI
        UpdateRecordingUI(true);
    }
    
    public void StopRecording()
    {
        if (!isRecording) return;
        
        // Stop camera input
        cameraInput.Dispose();
        
        // Stop recorder
        recorder.Dispose();
        
        isRecording = false;
        Debug.Log("Recording stopped");
        
        // Update UI
        UpdateRecordingUI(false);
    }
    
    void OnRecordingComplete(string path)
    {
        Debug.Log($"Video saved: {path}");
        
        // Save to Gallery
        #if UNITY_ANDROID && !UNITY_EDITOR
        NativeGallery.SaveVideoToGallery(path, "AR Graduation", 
            Path.GetFileName(path),
            (success, galleryPath) => {
                if (success)
                {
                    ShowToast("Video đã lưu vào thư viện");
                }
                else
                {
                    ShowToast("Lỗi: Không thể lưu video");
                }
            });
        #endif
    }
    
    void UpdateRecordingUI(bool recording)
    {
        // Đổi màu shutter button
        shutterButtonImage.color = recording ? Color.red : Color.white;
        
        // Hiện/ẩn recording indicator
        recordingIndicator.SetActive(recording);
    }
}
```
- [ ] Update code
- [ ] Test compile

---

## BƯỚC 7: TẠO RECORDING INDICATOR

### Tạo UI indicator
- [ ] Canvas > UI > Image
- [ ] Đặt tên: `RecordingIndicator`
- [ ] Rect Transform:
  - [ ] Anchor: Top Right
  - [ ] Width: 30, Height: 30
  - [ ] Pos: (-50, -50)
- [ ] Image: Vòng tròn đỏ
- [ ] Disable mặc định

### Thêm animation nhấp nháy
```csharp
// Script RecordingIndicator.cs
using UnityEngine;
using UnityEngine.UI;

public class RecordingIndicator : MonoBehaviour
{
    private Image image;
    
    void Start()
    {
        image = GetComponent<Image>();
    }
    
    void Update()
    {
        // Blink effect
        float alpha = Mathf.PingPong(Time.time * 2, 1);
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
```
- [ ] Tạo script
- [ ] Gắn vào RecordingIndicator

---

## BƯỚC 8: TÍCH HỢP VỚI SHUTTER BUTTON

### Update ShutterButton logic
```csharp
using UnityEngine.EventSystems;

public class ShutterButtonHandler : MonoBehaviour, 
    IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private CaptureController captureController;
    
    private float pressStartTime;
    private const float LONG_PRESS_THRESHOLD = 0.5f;
    private bool isLongPress = false;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        pressStartTime = Time.time;
        isLongPress = false;
        
        // Start checking for long press
        StartCoroutine(CheckLongPress());
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        
        if (isLongPress)
        {
            // Stop recording
            captureController.StopRecording();
        }
        else
        {
            // Take photo
            captureController.TakePhoto();
        }
    }
    
    IEnumerator CheckLongPress()
    {
        yield return new WaitForSeconds(LONG_PRESS_THRESHOLD);
        
        // Long press detected
        isLongPress = true;
        captureController.StartRecording();
    }
}
```
- [ ] Tạo script mới hoặc update existing
- [ ] Gắn vào ShutterButton
- [ ] Gán CaptureController reference

---

## BƯỚC 9: TEST CAPTURE SYSTEM

### Test chụp ảnh
- [ ] Build and Run
- [ ] Quét ảnh cúp
- [ ] Tap ShutterButton
- [ ] Verify:
  - [ ] Flash effect xuất hiện
  - [ ] Shutter sound phát
  - [ ] Toast "Ảnh đã lưu" hiện
  - [ ] Ảnh có trong Gallery

### Test quay video
- [ ] Giữ ShutterButton > 0.5s
- [ ] Verify:
  - [ ] Button chuyển đỏ
  - [ ] Recording indicator nhấp nháy
  - [ ] Thả tay → Recording dừng
  - [ ] Toast "Video đã lưu" hiện
  - [ ] Video có trong Gallery
  - [ ] Video có âm thanh

### Test edge cases
- [ ] Chụp 10 ảnh liên tiếp → Không crash
- [ ] Quay video 30s → Không lag
- [ ] Chụp ảnh khi đang quay → Không conflict

---

## BƯỚC 10: OPTIMIZE

### Giảm kích thước file
```csharp
// Trong SaveToGallery
byte[] bytes = texture.EncodeToJPG(75); // Quality 75%
```
- [ ] Update code

### Giảm resolution video nếu cần
```csharp
// Trong StartRecording
int width = Screen.width / 2;
int height = Screen.height / 2;
```
- [ ] Chỉ làm nếu performance kém

---

## CHECKLIST SPRINT 6

- [ ] NatCorder hoặc Unity Recorder đã cài
- [ ] NativeGallery plugin đã cài
- [ ] Permissions (Storage, Camera) đã xin
- [ ] Chụp ảnh hoạt động
- [ ] Flash effect và shutter sound
- [ ] Ảnh lưu vào Gallery thành công
- [ ] Quay video hoạt động
- [ ] Video có âm thanh
- [ ] Video lưu vào Gallery thành công
- [ ] Toast notification hiển thị
- [ ] Recording indicator nhấp nháy
- [ ] ShutterButton: Tap = ảnh, Hold = video
- [ ] Test thành công trên thiết bị thật
- [ ] Không crash khi chụp/quay liên tục

### Commit code
```bash
git add .
git commit -m "Sprint 6 complete: Photo capture and video recording"
git push origin dev
```

**✅ Sprint 6 hoàn thành → Phase 2 xong → Chuyển sang `phase3-sprint7.md`**
