# ML KIT TEXT RECOGNITION SETUP

## Bước 1: Thêm ARImageTracker vào Scene

1. Mở scene `ARTestScene.unity`
2. Chọn GameObject `XR Origin` trong Hierarchy
3. Add Component → Search "ARImageTracker"
4. Script sẽ tự động attach vào XR Origin

## Bước 2: Verify OCRManager GameObject

1. Trong Hierarchy, tìm GameObject `OCRManager`
2. Nếu chưa có:
   - Hierarchy → Right click → Create Empty
   - Đặt tên: `OCRManager`
   - Add Component → `OCRManager` script

## Bước 3: Verify UI Document

1. Trong Hierarchy, tìm GameObject có UIDocument component
2. Verify:
   - Source Asset = `ConfirmationPopup.uxml`
   - Có component `PopupManager` attached

## Bước 4: Build Settings

### Android Build Settings
1. File → Build Settings → Android
2. Player Settings → Other Settings:
   - Minimum API Level: **Android 10.0 (API 29)**
   - Target API Level: **Android 13.0 (API 33)** hoặc cao hơn
   - Scripting Backend: **IL2CPP**
   - Target Architectures: **ARM64** (bỏ tick ARMv7)

### Graphics Settings
1. Player Settings → Graphics:
   - Graphics API: **OpenGLES3** only (remove Vulkan)

## Bước 5: Gradle Dependencies

File `Assets/Plugins/Android/build.gradle` đã có:
```gradle
dependencies {
    implementation 'com.google.mlkit:text-recognition:16.0.0'
}
```

Unity sẽ tự động merge file này vào build.

## Bước 6: Test Flow

### Test trong Unity Editor (Mock Mode)
1. Play mode
2. Sau 2 giây, popup sẽ tự động hiện với tên mock
3. Verify UI hoạt động

### Test trên thiết bị Android
1. Build and Run
2. Quét ảnh cúp TVU
3. Flow:
   - AR detect ảnh → Log "Image detected"
   - Capture screenshot → Log "Screenshot captured"
   - OCR xử lý → Log "OCR Result: ..."
   - Popup hiện với tên đã parse
4. Sửa tên nếu cần → Click "Kích hoạt"

## Troubleshooting

### Lỗi: "ML Kit plugin failed"
- Verify Minimum API Level >= 29
- Verify build.gradle có ML Kit dependency
- Check Logcat: `adb logcat -s Unity`

### Lỗi: "No text detected"
- Ảnh cúp quá mờ hoặc góc chụp xấu
- Thử quét lại với góc khác
- Check lighting (ánh sáng đủ)

### Popup không hiện
- Check Console log: "OCR Result: ..."
- Verify OCRManager.Instance != null
- Verify PopupManager đã subscribe event

## Architecture Flow

```
AR Camera
    ↓
ARTrackedImageManager (detect cúp)
    ↓
ARImageTracker.OnTrackedImagesChanged()
    ↓
Capture Screenshot (Texture2D)
    ↓
OCRManager.ProcessImage()
    ↓ (Android)
MLKitTextRecognizer.java (native)
    ↓
OCRManager.OnTextRecognitionComplete()
    ↓
NameParser.ExtractStudentName()
    ↓
PopupManager.ShowPopup()
    ↓
UI Toolkit Popup (user confirm)
```

## Next Steps

Sau khi Sprint 2 hoàn thành:
- Sprint 3: Hiệu ứng AR (Phượng Hoàng + Pháo hoa)
- Sprint 4: UI Toolkit cho màn hình chính
- Sprint 5: Audio + Polish
- Sprint 6: Testing + Optimization
