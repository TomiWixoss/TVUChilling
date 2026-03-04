# PHASE 2 - SPRINT 5: TIMELINE & VFX (Tuần 7)
**Mục tiêu:** Tạo animation sequence hoành tráng

---

## BƯỚC 1: SETUP UNITY TIMELINE

### Mở Prefab Mode
- [ ] Project > `Assets/Prefabs/DragonAR`
- [ ] Double-click để vào Prefab Mode

### Tạo Timeline Asset
- [ ] Window > Sequencing > Timeline
- [ ] Trong Timeline window, click "Create"
- [ ] Đặt tên: `DragonSequence`
- [ ] Lưu vào `Assets/Timelines/`

### Thêm Playable Director
- [ ] Chọn DragonAR root object
- [ ] Add Component: **Playable Director**
- [ ] Field "Playable": Kéo `DragonSequence` vào
- [ ] Play On Awake: **ON**

---

## BƯỚC 2: TẠO ANIMATION TRACK

### Add Animation Track
- [ ] Timeline window > Click "+"
- [ ] Add > Animation Track
- [ ] Kéo DragonModel vào track (bind)

### Tạo keyframes bay lên
**0s - Vị trí ban đầu:**
- [ ] Timeline cursor ở 0s
- [ ] Chọn DragonModel
- [ ] Position Y: **-0.5** (dưới cúp)
- [ ] Click nút Record (đỏ) trong Timeline
- [ ] Thay đổi Position → Tự động tạo keyframe

**1s - Bay lên:**
- [ ] Timeline cursor ở 1s
- [ ] Position Y: **0.3**
- [ ] Rotation Y: **45**

**2s - Hover:**
- [ ] Timeline cursor ở 2s
- [ ] Position Y: **0.35**
- [ ] Rotation Y: **90**

**3s - Rống:**
- [ ] Timeline cursor ở 3s
- [ ] Scale: **1.1, 1.1, 1.1** (phóng to nhẹ)

**4s - Về bình thường:**
- [ ] Timeline cursor ở 4s
- [ ] Scale: **1, 1, 1**

### Smooth curves
- [ ] Click vào Animation Track
- [ ] Curves view
- [ ] Chọn tất cả keyframes
- [ ] Chuột phải > **Auto** (smooth interpolation)

---

## BƯỚC 3: THÊM VFX TRACK

### Track cho Magic Circle
- [ ] Timeline > Add > Activation Track
- [ ] Đặt tên: "MagicCircle"
- [ ] Kéo MagicCircle GameObject vào track

### Keyframe activation
- [ ] 0s: Active (vòng phép mở)
- [ ] 1.5s: Inactive (vòng phép đóng)

### Track cho Confetti
- [ ] Add > Activation Track
- [ ] Đặt tên: "Confetti"
- [ ] Kéo ConfettiVFX vào track
- [ ] 2s: Active
- [ ] 3s: Inactive

### Track cho Fireworks
- [ ] Add > Activation Track
- [ ] Đặt tên: "Fireworks"
- [ ] Kéo FireworksVFX vào track
- [ ] 3s: Active
- [ ] 5s: Inactive

---

## BƯỚC 4: THÊM AUDIO TRACK

### Import audio clips
- [ ] Verify audio files trong `Assets/Audio/SFX/`
- [ ] Whoosh.wav
- [ ] DragonRoar.wav
- [ ] Explosion.wav

### Add Audio Track
- [ ] Timeline > Add > Audio Track
- [ ] Đặt tên: "SoundEffects"

### Kéo audio vào timeline
**0s - Whoosh:**
- [ ] Kéo Whoosh.wav vào track ở 0s
- [ ] Duration: 1s

**2s - Dragon Roar:**
- [ ] Kéo DragonRoar.wav vào track ở 2s
- [ ] Volume: 0.8

**3s - Explosion:**
- [ ] Kéo Explosion.wav vào track ở 3s
- [ ] Volume: 0.7

### Điều chỉnh volume
- [ ] Click vào audio clip
- [ ] Inspector > Volume curve
- [ ] Fade in/out nếu cần

---

## BƯỚC 5: VIẾT DEPTH MASK SHADER

### Tạo Shader file
- [ ] Project > `Assets/Shaders/` (tạo folder)
- [ ] Chuột phải > Create > Shader > Unlit Shader
- [ ] Đặt tên: `DepthMask`

### Code shader
```shader
Shader "Custom/DepthMask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        
        Pass
        {
            // Chỉ ghi vào Depth Buffer, không render màu
            ZWrite On
            ColorMask 0
        }
    }
}
```
- [ ] Copy code vào file
- [ ] Save

### Tạo Material
- [ ] Chuột phải > Create > Material
- [ ] Đặt tên: `DepthMaskMat`
- [ ] Shader: Chọn `Custom/DepthMask`

---

## BƯỚC 6: TẠO CÚP 3D ĐƠN GIẢN

### Tạo model cúp
- [ ] Hierarchy > 3D Object > Cylinder
- [ ] Đặt tên: `CupMask`
- [ ] Transform:
  - [ ] Scale: Đo kích thước cúp thật (ví dụ: 0.15, 0.2, 0.15)
  - [ ] Position: (0, 0, 0)

### Gán Material
- [ ] Chọn CupMask
- [ ] Kéo `DepthMaskMat` vào Mesh Renderer

### Đặt làm child của DragonAR
- [ ] Kéo CupMask vào DragonAR prefab
- [ ] Position: Căn chỉnh để trùng với cúp thật

### Test occlusion
- [ ] Play Timeline
- [ ] Verify: Rồng bay ra sau cúp phải bị che
- [ ] Nếu không che: Kiểm tra Render Queue

---

## BƯỚC 7: TEXTMESHPRO 3D CHO TÊN

### Tạo TextMeshPro 3D
- [ ] Hierarchy > 3D Object > Text - TextMeshPro
- [ ] Đặt tên: `StudentNameText`
- [ ] Đặt làm child của DragonAR

### Cấu hình Text
- [ ] Text: "NGUYỄN VĂN A" (placeholder)
- [ ] Font: Bold
- [ ] Font Size: **0.05**
- [ ] Alignment: Center
- [ ] Transform:
  - [ ] Position: (0, 0.4, 0) - phía trên cúp
  - [ ] Rotation: (0, 0, 0)

### Tạo Material vàng kim
- [ ] Chuột phải > Create > Material
- [ ] Đặt tên: `GoldTextMat`
- [ ] Shader: TextMeshPro/Distance Field
- [ ] Face Color: Vàng (#FFD700)
- [ ] Outline: Nâu đậm, Width: 0.2
- [ ] Glow: Vàng nhạt, Power: 0.3

### Gán Material
- [ ] Chọn StudentNameText
- [ ] Material: Kéo `GoldTextMat` vào

---

## BƯỚC 8: ANIMATION CHO TEXT

### Tạo script TextAnimation.cs
```csharp
using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextAnimation : MonoBehaviour
{
    private TextMeshPro textMesh;
    
    void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
        
        // Ẩn ban đầu
        transform.localScale = Vector3.zero;
        textMesh.alpha = 0;
    }
    
    public void ShowText(string studentName)
    {
        // Set tên
        textMesh.text = studentName;
        
        // Animation: Scale từ 0 → 1
        transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack);
        
        // Fade in
        textMesh.DOFade(1f, 0.5f);
        
        // Bounce nhẹ
        transform.DOMoveY(transform.position.y + 0.05f, 0.8f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}
```
- [ ] Tạo script
- [ ] Gắn vào StudentNameText

---

## BƯỚC 9: TÍCH HỢP VỚI POPUP MANAGER

### Sửa PopupManager.cs
```csharp
// Thêm reference
[SerializeField] private TextAnimation studentNameText;

void OnConfirmClicked()
{
    string finalName = nameInputField.text;
    
    // Trigger text animation
    studentNameText.ShowText(finalName);
    
    // Hide popup
    popupPanel.SetActive(false);
}
```
- [ ] Update code
- [ ] Gán reference StudentNameText

---

## BƯỚC 10: TEST TIMELINE

### Test trong Prefab Mode
- [ ] Chọn DragonAR prefab
- [ ] Timeline window > Play
- [ ] Verify:
  - [ ] Rồng bay lên mượt
  - [ ] VFX xuất hiện đúng thời điểm
  - [ ] Audio phát đồng bộ
  - [ ] Occlusion hoạt động
  - [ ] Text xuất hiện sau timeline

### Test trong Scene
- [ ] Exit Prefab Mode
- [ ] Play ARScene
- [ ] Quét ảnh cúp
- [ ] Confirm tên trong popup
- [ ] Verify: Timeline chạy hoàn chỉnh

### Test trên thiết bị
- [ ] Build and Run
- [ ] Test đầy đủ flow
- [ ] Ghi nhận FPS (phải >= 30)

---

## BƯỚC 11: FINE-TUNING

### Điều chỉnh timing
- [ ] Nếu quá nhanh: Kéo dài timeline
- [ ] Nếu quá chậm: Rút ngắn
- [ ] Mục tiêu: 4-5 giây tổng thời gian

### Điều chỉnh camera angle
- [ ] Nếu text bị che: Đổi position
- [ ] Nếu VFX ra ngoài frame: Scale nhỏ lại

### Optimize performance
- [ ] Nếu lag: Giảm particle count
- [ ] Nếu audio bị delay: Preload audio clips

---

## CHECKLIST SPRINT 5

- [ ] Unity Timeline đã setup với Playable Director
- [ ] Animation Track: Rồng bay lên mượt mà
- [ ] VFX Tracks: Magic Circle, Confetti, Fireworks
- [ ] Audio Track: Whoosh, Roar, Explosion đồng bộ
- [ ] Depth Mask Shader hoạt động (occlusion)
- [ ] CupMask 3D che vật thể phía sau
- [ ] TextMeshPro 3D với material vàng kim
- [ ] Text animation (scale, fade, bounce)
- [ ] Tích hợp với PopupManager
- [ ] Timeline chạy hoàn chỉnh trên thiết bị
- [ ] FPS >= 30 với tất cả VFX

### Commit code
```bash
git add .
git commit -m "Sprint 5 complete: Timeline with VFX, audio, depth mask, 3D text"
git push origin dev
```

**✅ Sprint 5 hoàn thành → Chuyển sang `phase2-sprint6.md`**
