# PHASE 1 - SPRINT 1: AR IMAGE TRACKING Cơ BẢN (Tuần 3)
**Mục tiêu:** Camera nhận diện cúp và spawn object 3D

---

## BƯỚC 1: SETUP AR SCENE

### Tạo scene mới
- [ ] File > New Scene > Basic (Built-in)
- [ ] Đặt tên: `ARScene`
- [ ] File > Save As: `Assets/Scenes/ARScene.unity`

### Xóa camera mặc định
- [ ] Chọn "Main Camera" trong Hierarchy
- [ ] Delete (phím Delete hoặc chuột phải > Delete)

### Thêm XR Origin
- [ ] Hierarchy > Chuột phải > XR > XR Origin (Action-based)
- [ ] Verify: XR Origin có các component:
  - [ ] XR Origin
  - [ ] AR Camera Manager
  - [ ] AR Session Origin

### Thêm AR Session
- [ ] Hierarchy > Chuột phải > XR > AR Session
- [ ] Verify: AR Session có component AR Session

### Verify Scene hierarchy
```
Hierarchy:
├── XR Origin
│   ├── Camera Offset
│   │   └── Main Camera
│   └── ...
└── AR Session
```
- [ ] Cấu trúc giống như trên

---

## BƯỚC 2: TẠO REFERENCE IMAGE LIBRARY

### Tạo library (nếu chưa có từ Phase 0)
- [ ] Project window > Chuột phải
- [ ] Create > XR > Reference Image Library
- [ ] Đặt tên: `CupImageLibrary`

### Thêm ảnh cúp
- [ ] Click vào `CupImageLibrary`
- [ ] Inspector > Click "Add Image"
- [ ] Kéo ảnh cúp từ `Assets/Images/Targets/` vào field "Texture"
- [ ] Nhập "Physical Size": (ví dụ: `0.15` nếu cúp rộng 15cm)
- [ ] Name: `Cup_Front`
- [ ] Verify: Quality = "Good" hoặc "Excellent"

---

## BƯỚC 3: SETUP AR TRACKED IMAGE MANAGER

### Thêm component vào XR Origin
- [ ] Chọn "XR Origin" trong Hierarchy
- [ ] Inspector > Add Component
- [ ] Tìm: "AR Tracked Image Manager"
- [ ] Click để thêm

### Gán Image Library
- [ ] Trong AR Tracked Image Manager component
- [ ] Field "Serialized Library": Kéo `CupImageLibrary` vào
- [ ] Max Number Of Moving Images: `1`

### Tạo Prefab test đơn giản
- [ ] Hierarchy > Chuột phải > 3D Object > Cube
- [ ] Đặt tên: `TestCube`
- [ ] Transform:
  - [ ] Position: (0, 0, 0)
  - [ ] Rotation: (0, 0, 0)
  - [ ] Scale: (0.1, 0.1, 0.1)
- [ ] Inspector > Add Component > Material
- [ ] Chọn màu đỏ (hoặc màu nổi bật)
- [ ] Kéo `TestCube` từ Hierarchy vào thư mục `Assets/Prefabs/`
- [ ] Xóa `TestCube` khỏi Hierarchy

### Gán Prefab vào AR Tracked Image Manager
- [ ] Chọn "XR Origin" trong Hierarchy
- [ ] AR Tracked Image Manager > Field "Tracked Image Prefab"
- [ ] Kéo `TestCube` prefab vào field này

---

## BƯỚC 4: TEST TRÊN THIẾT BỊ THẬT

### Chuẩn bị ảnh cúp in ra
- [ ] In ảnh cúp ra giấy A4 (màu, chất lượng cao)
- [ ] Hoặc hiển thị ảnh trên màn hình máy tính/tablet

### Build Settings
- [ ] File > Build Settings
- [ ] Xóa scene cũ (nếu có)
- [ ] Click "Add Open Scenes" (thêm ARScene)
- [ ] Verify: ARScene ở index 0
- [ ] Platform: Android (có icon Unity bên cạnh)

### Build and Run
- [ ] Tick "Development Build"
- [ ] Tick "Autoconnect Profiler" (để debug)
- [ ] Click "Build And Run"
- [ ] Lưu: `Builds/sprint1_v0.1.apk`
- [ ] Chờ build

### Test trên điện thoại
- [ ] App tự động mở
- [ ] Chĩa camera vào ảnh cúp in ra
- [ ] **Kỳ vọng:** Cube đỏ xuất hiện trên ảnh cúp
- [ ] Di chuyển camera: Cube phải theo ảnh
- [ ] Che ảnh: Cube phải biến mất

### Ghi nhận kết quả
- [ ] ✅ Cube xuất hiện đúng vị trí
- [ ] ✅ Cube theo tracking ổn định
- [ ] ✅ Không có lỗi crash
- [ ] ❌ Nếu có vấn đề: Ghi chú vào phần Troubleshooting

---

## BƯỚC 5: VIẾT SCRIPT QUẢN LÝ TRACKING

### Tạo script mới
- [ ] Project window > `Assets/Scripts/` (tạo folder nếu chưa có)
- [ ] Chuột phải > Create > C# Script
- [ ] Đặt tên: `ARImageTracker`

### Viết code cơ bản
```csharp
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARImageTracker : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;

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
        // Image mới được detect
        foreach (var trackedImage in eventArgs.added)
        {
            Debug.Log($"Image detected: {trackedImage.referenceImage.name}");
        }

        // Image đang được track (update position)
        foreach (var trackedImage in eventArgs.updated)
        {
            Debug.Log($"Image tracking: {trackedImage.referenceImage.name} - State: {trackedImage.trackingState}");
        }

        // Image mất dấu
        foreach (var trackedImage in eventArgs.removed)
        {
            Debug.Log($"Image lost: {trackedImage.referenceImage.name}");
        }
    }
}
```

- [ ] Copy code trên vào `ARImageTracker.cs`
- [ ] Save file (Ctrl+S)

### Gắn script vào XR Origin
- [ ] Chọn "XR Origin" trong Hierarchy
- [ ] Inspector > Add Component
- [ ] Tìm: "ARImageTracker"
- [ ] Click để thêm

### Test với Logcat
- [ ] Cài ADB (Android Debug Bridge) nếu chưa có
- [ ] Mở CMD/Terminal, chạy: `adb logcat -s Unity`
- [ ] Build and Run lại app
- [ ] Chĩa camera vào ảnh cúp
- [ ] **Kỳ vọng:** Console hiện log:
  ```
  Image detected: Cup_Front
  Image tracking: Cup_Front - State: Tracking
  ```

---

## BƯỚC 6: VERIFY VÀ COMMIT

### Checklist Sprint 1
- [x] AR Scene đã setup với XR Origin + AR Session
- [x] Reference Image Library đã tạo và gán đúng
- [x] AR Tracked Image Manager hoạt động
- [x] Cube test xuất hiện khi quét ảnh cúp
- [x] ARBackgroundRendererFeature đã thêm vào URP Renderer (fix màu vàng)
- [x] Camera feed hiển thị đúng
- [x] Build APK thành công và test trên thiết bị thật

### Commit code
```bash
git add .
git commit -m "Sprint 1 complete: AR Image Tracking basic"
git push origin dev
```
- [ ] Chạy lệnh commit

---

## TROUBLESHOOTING

### Cube không xuất hiện
**Nguyên nhân có thể:**
- [ ] Ảnh in ra quá mờ/nhòe → In lại chất lượng cao hơn
- [ ] Physical Size sai → Đo lại và nhập đúng kích thước thật
- [ ] ARCore chưa enable → Verify XR Plug-in Management
- [ ] Prefab chưa gán → Check AR Tracked Image Manager

### Cube xuất hiện rồi biến mất ngay
**Nguyên nhân:**
- [ ] Ánh sáng kém → Test ở nơi sáng hơn
- [ ] Ảnh bị lóa/phản chiếu → Đổi góc chiếu sáng
- [ ] Camera di chuyển quá nhanh → Giữ camera ổn định

### Lỗi: "No active AR Session"
**Giải pháp:**
- [ ] Verify có GameObject "AR Session" trong scene
- [ ] Verify AR Session có component "AR Session"

### Lỗi: "ARCore not installed"
**Giải pháp:**
- [ ] Mở Google Play Store trên điện thoại
- [ ] Tìm "Google Play Services for AR"
- [ ] Cài đặt/Cập nhật

---

**✅ Sprint 1 hoàn thành → Chuyển sang `phase1-sprint2.md`**
