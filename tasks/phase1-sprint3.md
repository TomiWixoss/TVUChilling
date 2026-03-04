# PHASE 1 - SPRINT 3: 3D CONTENT PIPELINE (Tuần 5)
**Mục tiêu:** Import và optimize model Rồng

---

## BƯỚC 1: IMPORT MODEL VÀO UNITY

### Copy file vào project
- [ ] Mở thư mục `Assets/Models/Dragon/`
- [ ] Copy file FBX/OBJ đã download vào đây

### Cấu hình Import Settings
- [ ] Click vào model trong Unity
- [ ] Inspector > Model tab:
  - [ ] Scale Factor: **1**
  - [ ] Mesh Compression: **Medium**
  - [ ] Read/Write Enabled: **OFF** (tiết kiệm RAM)
  - [ ] Optimize Mesh: **ON**
  - [ ] Generate Colliders: **OFF** (không cần)
- [ ] Click "Apply"

---

## BƯỚC 2: OPTIMIZE TEXTURE

### Kiểm tra texture
- [ ] Mở thư mục `Assets/Models/Dragon/Materials/`
- [ ] Xem các file texture (diffuse, normal, etc.)

### Nén texture
- [ ] Click vào từng texture
- [ ] Inspector:
  - [ ] Max Size: **512** (hoặc 1024 nếu cần)
  - [ ] Compression: **Normal Quality**
  - [ ] Generate Mip Maps: **ON**
  - [ ] Filter Mode: **Bilinear**
- [ ] Click "Apply"
- [ ] Lặp lại cho tất cả texture

---

## BƯỚC 3: KIỂM TRA POLY COUNT

### Xem thống kê model
- [ ] Chọn model trong Project
- [ ] Inspector > Model tab
- [ ] Xem "Vertices" và "Triangles"
- [ ] **Mục tiêu:** < 10,000 triangles

### Nếu poly quá cao
- [ ] Mở Blender (download miễn phí nếu chưa có)
- [ ] Import model FBX
- [ ] Add Modifier: Decimate
- [ ] Ratio: 0.5 (giảm 50% poly)
- [ ] Apply modifier
- [ ] Export lại FBX
- [ ] Re-import vào Unity

---

## BƯỚC 4: SETUP ANIMATION

### Kiểm tra animation có sẵn
- [ ] Click vào model
- [ ] Tab "Animation"
- [ ] Xem có animation clips không

### Nếu có animation
- [ ] Tick "Import Animation"
- [ ] Xem preview animation
- [ ] Apply

### Nếu không có animation - Dùng DOTween
- [ ] Window > Package Manager
- [ ] Tìm "DOTween" (hoặc download từ Asset Store)
- [ ] Import DOTween (Free)

---

## BƯỚC 5: TẠO PREFAB RỒNG

### Kéo model vào scene
- [ ] Kéo model Dragon từ Project vào Hierarchy
- [ ] Đặt tên: `DragonModel`
- [ ] Transform:
  - [ ] Position: (0, 0, 0)
  - [ ] Rotation: (0, 0, 0)
  - [ ] Scale: (0.1, 0.1, 0.1) - điều chỉnh cho phù hợp

### Thêm Animator (nếu có animation)
- [ ] Chọn DragonModel
- [ ] Add Component: **Animator**
- [ ] Tạo Animator Controller:
  - [ ] Project > Chuột phải > Create > Animator Controller
  - [ ] Đặt tên: `DragonController`
- [ ] Gán vào field "Controller" của Animator

### Tạo animation bay đơn giản (nếu không có)
```csharp
// Tạo script: DragonAnimation.cs
using UnityEngine;
using DG.Tweening;

public class DragonAnimation : MonoBehaviour
{
    void Start()
    {
        // Bay lên từ dưới
        transform.DOMoveY(0.3f, 1f).SetEase(Ease.OutQuad);
        
        // Xoay nhẹ
        transform.DORotate(new Vector3(0, 360, 0), 3f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
        
        // Hover lên xuống
        transform.DOMoveY(0.35f, 1.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}
```
- [ ] Tạo script `DragonAnimation.cs`
- [ ] Copy code trên
- [ ] Gắn vào DragonModel

---

## BƯỚC 6: THÊM VFX VÀ AUDIO

### Import VFX
- [ ] Copy Particle System assets vào `Assets/VFX/`
- [ ] Kéo Confetti prefab vào Hierarchy
- [ ] Đặt làm child của DragonModel
- [ ] Position: (0, 0.2, 0)
- [ ] Play On Awake: **OFF** (sẽ trigger bằng code)

### Import Audio
- [ ] Copy file audio vào `Assets/Audio/SFX/`
- [ ] Click vào file audio:
  - [ ] Load Type: **Compressed In Memory**
  - [ ] Compression Format: **Vorbis**
  - [ ] Quality: **70%**
- [ ] Apply

### Thêm Audio Source
- [ ] Chọn DragonModel
- [ ] Add Component: **Audio Source**
- [ ] Kéo audio clip vào field "AudioClip"
- [ ] Play On Awake: **OFF**
- [ ] Volume: **0.7**

---

## BƯỚC 7: TẠO PREFAB HOÀN CHỈNH

### Tạo Empty GameObject container
- [ ] Hierarchy > Chuột phải > Create Empty
- [ ] Đặt tên: `DragonAR`
- [ ] Position: (0, 0, 0)

### Sắp xếp hierarchy
```
DragonAR
├── DragonModel (model + animator + audio)
├── ConfettiVFX (particle system)
└── (sẽ thêm text 3D sau)
```
- [ ] Kéo DragonModel làm child của DragonAR
- [ ] Kéo ConfettiVFX làm child của DragonAR

### Tạo Prefab
- [ ] Kéo `DragonAR` từ Hierarchy vào `Assets/Prefabs/`
- [ ] Xóa `DragonAR` khỏi Hierarchy

---

## BƯỚC 8: THAY THẾ CUBE TEST

### Sửa AR Tracked Image Manager
- [ ] Chọn "XR Origin" trong Hierarchy
- [ ] AR Tracked Image Manager component
- [ ] Field "Tracked Image Prefab":
  - [ ] Xóa TestCube
  - [ ] Kéo DragonAR prefab vào

### Test trong Editor (mock)
- [ ] Play mode
- [ ] Manually instantiate DragonAR để xem
- [ ] Verify: Animation chạy, audio phát

---

## BƯỚC 9: ĐO PERFORMANCE

### Build Development Build
- [ ] File > Build Settings
- [ ] Tick "Development Build"
- [ ] Tick "Autoconnect Profiler"
- [ ] Build and Run

### Mở Profiler
- [ ] Window > Analysis > Profiler
- [ ] Profiler tự động connect với app

### Đo FPS
- [ ] Quét ảnh cúp để spawn Rồng
- [ ] Quan sát Profiler:
  - [ ] CPU Usage
  - [ ] GPU Usage
  - [ ] FPS (mục tiêu: >= 30)
  - [ ] Memory

### Ghi nhận kết quả
- [ ] FPS trung bình: _____ fps
- [ ] CPU time: _____ ms
- [ ] Memory: _____ MB
- [ ] Draw calls: _____ calls

### Nếu FPS < 30
**Tối ưu:**
- [ ] Giảm poly count model xuống 5000 triangles
- [ ] Giảm texture size xuống 256x256
- [ ] Giảm số lượng particles
- [ ] Tắt shadows: Model > Cast Shadows: OFF

---

## BƯỚC 10: TEST TRÊN THIẾT BỊ THẬT

### Build APK final Sprint 3
- [ ] Build Settings > Build and Run
- [ ] Lưu: `Builds/sprint3_v0.3.apk`

### Test checklist
- [ ] Quét ảnh cúp
- [ ] Rồng xuất hiện (thay vì Cube)
- [ ] Animation chạy mượt
- [ ] Audio phát ra
- [ ] VFX hiển thị
- [ ] FPS >= 30 (dùng app FPS counter)
- [ ] Không crash

---

## CHECKLIST SPRINT 3

- [ ] Model Rồng đã import và optimize
- [ ] Texture đã nén xuống 512x512
- [ ] Poly count < 10,000 triangles
- [ ] Animation hoạt động (có sẵn hoặc dùng DOTween)
- [ ] VFX (Confetti) đã thêm
- [ ] Audio đã thêm và phát được
- [ ] Prefab DragonAR hoàn chỉnh
- [ ] AR Tracked Image Manager đã gán DragonAR
- [ ] Performance >= 30 FPS trên máy test
- [ ] Test thành công trên thiết bị thật

### Commit code
```bash
git add .
git commit -m "Sprint 3 complete: 3D Dragon model with animation and VFX"
git push origin dev
```
- [ ] Chạy lệnh commit

**✅ Sprint 3 hoàn thành → Phase 1 xong → Chuyển sang `phase2-sprint4.md`**
