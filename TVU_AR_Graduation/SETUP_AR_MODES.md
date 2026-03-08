# Setup 3 Chế độ AR - TVU AR Graduation

## Tổng quan

**2 Modes chính:**
1. **Image Tracking Mode** - Quét cúp hoặc bằng tốt nghiệp
   - Cúp TVU (dọc) → Phoenix bay circular path
   - Bằng TVU (ngang) → Dancer nhảy tại chỗ
2. **Placement Mode** - Đặt/di chuyển/xóa/scale objects 3D

## Cấu trúc đã tạo

```
TVU_AR_Graduation/
├── Assets/
│   ├── Models/
│   │   ├── Phoenix/          # Import phoenix_bird.glb
│   │   └── Dancer/           # Import dancing_character.fbx
│   ├── Prefabs/
│   │   ├── ImageTracking/    # PhoenixAR, DancerAR
│   │   └── Placement/        # Cube, Sphere, Cylinder
│   ├── Images/Targets/
│   │   ├── Cup_TVU.png       # Image target cúp (đã có)
│   │   └── Diploma_TVU.jpg   # Image target bằng (✅ đã copy)
│   └── Scripts/
│       ├── ARImageTracker.cs           # ✅ Detect 2 images
│       ├── ImageTrackingController.cs  # ✅ Phoenix flight path
│       ├── PlacementController.cs      # ✅ Placement system
│       └── WebViewManager.cs           # ✅ Mode switching
└── webview-ui/
    └── src/
        ├── App.tsx                      # ✅ Mode routing
        └── components/
            ├── ModeSelector.tsx         # ✅ 2 mode buttons
            ├── CameraControls.tsx       # ✅ Flash/Photo/Video
            └── PlacementControls.tsx    # ✅ Prefab selector + actions
```

## Unity Setup

### 1. Import Models

**Phoenix:**
```
1. Copy phoenix_bird.glb → Assets/Models/Phoenix/
2. Unity auto-convert GLB → FBX
3. Select model → Inspector:
   - Rig → Animation Type: Generic
   - Apply
```

**Dancer:**
```
1. Copy dancing_character.fbx → Assets/Models/Dancer/
2. Select model → Inspector:
   - Rig → Animation Type: Humanoid
   - Apply
```

### 2. Setup Image Library

**AR → CupImageLibrary:**
```
1. Select CupImageLibrary asset
2. Add Reference Images:
   - Cup_TVU (đã có)
   - Diploma_TVU (mới thêm)
3. Set texture quality: High
4. Apply
```

### 3. Tạo Prefabs

**PhoenixAR.prefab:**
```
PhoenixAR (Empty GameObject)
├── phoenix_bird (Model)
│   └── Animator
└── ImageTrackingController (Script)
    - Flight Radius: 2
    - Flight Height: 3
    - Flight Speed: 1
    - Rotation Speed: 5
```

**DancerAR.prefab:**
```
DancerAR (Empty GameObject)
└── dancing_character (Model)
    └── Animator (với animation clip từ FBX)
```

**Placement Prefabs:**
```
Placement/
├── Cube.prefab (Unity primitive + Material)
├── Sphere.prefab
└── Cylinder.prefab
```

### 4. Setup ARTestScene

**Hierarchy:**
```
ARTestScene
├── XR Origin
│   └── Camera
├── AR Session
├── AR Tracked Image Manager
│   ├── Reference Image Library: CupImageLibrary
│   ├── Max Number Moving Images: 2
│   └── Tracked Image Prefab: (leave empty, handled by script)
├── AR Plane Manager (disabled by default)
├── AR Raycast Manager
├── ARImageTracker (Script)
│   ├── Phoenix Prefab: PhoenixAR
│   └── Dancer Prefab: DancerAR
├── PlacementController (Script, disabled by default)
│   ├── AR Raycast Manager: (assign)
│   ├── AR Plane Manager: (assign)
│   ├── AR Camera: Main Camera
│   └── Placement Prefabs: [Cube, Sphere, Cylinder]
├── CameraController
└── WebViewManager
    ├── Camera Controller: (assign)
    ├── Image Tracker: AR Tracked Image Manager
    └── Placement Controller: (assign)
```

## React UI Flow

```
App.tsx
├── currentMode === 'none'
│   └── ModeSelector
│       ├── Button: Cúp & Bằng → onModeSelect('imageTracking')
│       └── Button: Trang trí 3D → onModeSelect('placement')
│
└── currentMode !== 'none'
    ├── Back Button → setCurrentMode('none')
    ├── CameraControls (Flash/Photo/Video) - Luôn hiện
    └── PlacementControls (chỉ hiện khi mode === 'placement')
        ├── Prefab selector (Cube/Sphere/Cylinder)
        ├── Delete button
        └── Clear all button
```

## Unity ↔ React Messages

**Mode Selection:**
```typescript
window.Unity.call(JSON.stringify({
  method: 'onModeSelect',
  data: 'imageTracking' | 'placement' | 'none'
}))
```

**Placement Controls:**
```typescript
// Select prefab
window.Unity.call(JSON.stringify({
  method: 'onPrefabSelect',
  data: '0' | '1' | '2'  // Cube/Sphere/Cylinder
}))

// Delete selected
window.Unity.call(JSON.stringify({
  method: 'onPlacementDelete',
  data: ''
}))

// Clear all
window.Unity.call(JSON.stringify({
  method: 'onPlacementClear',
  data: ''
}))
```

## Testing

### Mode 1: Image Tracking
1. Chọn "Cúp & Bằng"
2. Point camera tại cúp TVU → Phoenix bay circular path
3. Point camera tại bằng TVU → Dancer nhảy
4. Test: Flash/Photo/Video hoạt động

### Mode 2: Placement
1. Chọn "Trang trí 3D"
2. Scan mặt phẳng (bàn/sàn)
3. Chọn prefab (Cube/Sphere/Cylinder)
4. Tap để đặt object
5. Tap object để select (màu vàng)
6. Drag để di chuyển
7. Pinch 2 fingers để scale
8. Tap Delete để xóa selected
9. Tap Clear All để xóa tất cả
10. Test: Flash/Photo/Video hoạt động

## Build React UI

```bash
cd webview-ui
npm install
npm run build
```

Output → `TVU_AR_Graduation/Assets/StreamingAssets/webview/`

## Notes

- **CameraController/NativeVideoRecorder**: Không thay đổi
- **Flash/Photo/Video**: Hoạt động ở cả 2 modes
- **Back button**: Reset về mode selection
- **Image Tracking**: Tự động detect cúp hoặc bằng, spawn prefab tương ứng
- **Placement**: Pinch to scale, drag to move, tap to select/place
