# UNITY SCENE SETUP - SPRINT 2

## Checklist Setup Scene

### 1. Add ARImageTracker Component

**Mục đích:** Detect ảnh cúp và trigger OCR

**Steps:**
1. Mở scene `Assets/Scenes/ARTestScene.unity`
2. Trong Hierarchy, chọn GameObject **XR Origin**
3. Inspector → Click **Add Component**
4. Search: `ARImageTracker`
5. Click để add script
6. Verify: Script hiện trong Inspector với icon C#

**Expected Result:**
- XR Origin có 2 components:
  - AR Tracked Image Manager (đã có từ Sprint 1)
  - AR Image Tracker (mới thêm)

---

### 2. Verify OCRManager GameObject

**Mục đích:** Singleton quản lý OCR processing

**Steps:**
1. Trong Hierarchy, tìm GameObject tên **OCRManager**
2. Nếu **CHƯA CÓ**:
   - Hierarchy → Right click → **Create Empty**
   - Đặt tên: `OCRManager`
   - Chọn GameObject vừa tạo
   - Inspector → **Add Component** → Search `OCRManager`
   - Click để add script

**Expected Result:**
- Hierarchy có GameObject `OCRManager` (root level)
- Inspector có component `OCRManager (Script)`

---

### 3. Verify UI Document Setup

**Mục đích:** Hiển thị popup xác nhận tên

**Steps:**
1. Trong Hierarchy, tìm GameObject có **UIDocument** component
   - Có thể tên là: `ConfirmationUI` hoặc `UI Document`
2. Nếu **CHƯA CÓ**:
   - Hierarchy → Right click → **UI Toolkit** → **UI Document**
   - Đặt tên: `ConfirmationUI`
3. Chọn GameObject đó, trong Inspector:
   - **UIDocument** component:
     - Source Asset: Kéo file `Assets/UI/ConfirmationPopup.uxml` vào
     - Panel Settings: `DefaultPanelSettings` (nếu có)
   - **Add Component** → Search `PopupManager`
   - Click để add script

**Expected Result:**
- Hierarchy có GameObject với UIDocument
- Inspector có 2 components:
  - UI Document (Source Asset = ConfirmationPopup.uxml)
  - Popup Manager (Script)

---

### 4. Scene Hierarchy Final Check

Sau khi setup xong, Hierarchy phải có:

```
ARTestScene
├── XR Origin
│   ├── Camera Offset
│   │   └── Main Camera
│   ├── AR Tracked Image Manager (Component)
│   └── AR Image Tracker (Component) ← MỚI THÊM
├── AR Session
├── OCRManager ← VERIFY CÓ
└── ConfirmationUI (hoặc UI Document) ← VERIFY CÓ
    ├── UI Document (Component)
    └── Popup Manager (Component)
```

---

### 5. Test trong Unity Editor

**Steps:**
1. Click **Play** button
2. Quan sát Console log:
   - `Testing OCR in Editor...` (sau 2 giây)
   - `OCR Result: TRƯỜNG ĐẠI HỌC...`
3. Popup phải hiện với:
   - Title: "Xác nhận danh tính"
   - TextField có text: "NGUYỄN VĂN A"
   - 2 buttons: "Kích hoạt" và "Hủy"
4. Click "Kích hoạt" → Console log: `User confirmed name: NGUYỄN VĂN A`
5. Popup đóng

**Expected Result:**
- Không có lỗi trong Console
- Popup hiện và hoạt động đúng
- Buttons responsive

---

### 6. Build and Test trên Android

**Steps:**
1. File → Build Settings → Android
2. Click **Build and Run**
3. Chọn nơi lưu APK
4. Đợi build xong, app tự động cài và chạy
5. Quét ảnh cúp TVU bằng camera
6. Quan sát:
   - AR cube đỏ spawn (từ Sprint 1)
   - Popup hiện sau vài giây
   - TextField có tên được OCR

**Expected Result:**
- App không crash
- AR tracking hoạt động
- OCR detect text từ ảnh cúp
- Popup hiện với tên đã parse

---

## Troubleshooting

### Lỗi: "OCRManager.Instance is null"
**Fix:** Verify GameObject `OCRManager` tồn tại trong scene và có script attached

### Lỗi: "UIDocument component not found"
**Fix:** Verify GameObject có UIDocument component và PopupManager cùng GameObject

### Popup không hiện trong Editor
**Fix:** 
- Check Console có log "OCR Result: ..."
- Verify UXML file path đúng trong UIDocument
- Verify element names trong UXML match với PopupManager code

### Build lỗi: "Gradle build failed"
**Fix:**
- Unity → Preferences → External Tools → Android SDK path phải đúng
- Verify `build.gradle` file syntax đúng
- Clean build: Delete `Library/Bee` folder và build lại

---

## Next: Test Real OCR

Sau khi verify flow hoạt động với mock data:
1. Build APK
2. Quét ảnh cúp thật có chữ
3. Verify OCR đọc được tên
4. Nếu sai → Sửa trong TextField → Confirm
5. Ready cho Sprint 3 (AR effects)
