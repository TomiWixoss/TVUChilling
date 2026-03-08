# Setup ARTemplate Placement System

## ✅ HOÀN THÀNH
1. ✅ Cài đặt XR Interaction Toolkit package
2. ✅ Import Starter Assets sample
3. ✅ Copy ObjectSpawner.cs từ ARTemplate
4. ✅ Tạo ARPlacementManager.cs wrapper
5. ✅ Update WebViewManager.cs
6. ✅ Compile scripts thành công

## � ĐANG LÀM - Setup Unity Scene

### Bước 1: Thêm ObjectSpawner Component

Trong Unity Editor:
1. Chọn GameObject "ARPlacementManager" trong Hierarchy
2. Trong Inspector, click "Add Component"
3. Tìm và thêm "Object Spawner" (từ XR Interaction Toolkit Starter Assets)
4. Kéo Main Camera vào field "Camera To Face" của ObjectSpawner

### Bước 2: Tạo XR Interaction Group

1. Click chuột phải vào "XR Origin" trong Hierarchy
2. Chọn "Create Empty"
3. Đổi tên thành "XR Interaction Group"
4. Với "XR Interaction Group" được chọn, click "Add Component"
5. Tìm và thêm "XR Interaction Group"

### Bước 3: Tạo Test Prefabs với XRGrabInteractable

Tạo prefab Cube đơn giản:
1. Trong Hierarchy: GameObject → 3D Object → Cube
2. Đổi tên thành "TestCube"
3. Với TestCube được chọn, click "Add Component"
4. Thêm "XR Grab Interactable" component
5. Thêm "Rigidbody" component (required cho XR Grab)
6. Trong Rigidbody: Bỏ check "Use Gravity"
7. Kéo TestCube từ Hierarchy vào folder Assets/Prefabs (tạo folder nếu chưa có)
8. Xóa TestCube khỏi scene (giữ lại prefab)

### Bước 4: Assign References trong ARPlacementManager

1. Chọn "ARPlacementManager" trong Hierarchy
2. Trong Inspector, tìm component "ARPlacementManager"
3. Assign các references:
   - **Object Spawner**: Kéo component ObjectSpawner (cùng GameObject) vào đây
   - **Interaction Group**: Kéo "XR Interaction Group" GameObject vào đây
   - **AR Raycast Manager**: Kéo "XR Origin" GameObject vào (nó có ARRaycastManager component)

4. Trong component "Object Spawner":
   - **Object Prefabs**: Click "+" để thêm slot
   - Kéo prefab "TestCube" vào slot đầu tiên
   - Có thể thêm nhiều prefabs khác (Sphere, Capsule, etc.)

### Bước 5: Wire WebViewManager

1. Chọn "WebViewManager" trong Hierarchy
2. Trong Inspector, tìm component "WebViewManager"
3. Tìm field "Ar Placement Manager"
4. Kéo GameObject "ARPlacementManager" vào field này

### Bước 6: Test Workflow

1. Build và chạy trên Android device
2. WebView UI sẽ gọi:
   - `SelectModel(index)` → chọn prefab từ list
   - `PlaceObject()` → đặt object tại vị trí tap
   - `DeleteSelected()` → xóa object đang chọn
   - `DuplicateSelected()` → nhân bản object đang chọn

## 📋 Kiến trúc hoàn chỉnh

```
WebView UI (React)
    ↓ (postMessage)
WebViewManager.cs
    ↓ (calls)
ARPlacementManager.cs
    ↓ (uses)
ObjectSpawner.cs (XR Interaction Toolkit)
    ↓ (spawns)
Prefabs with XRGrabInteractable
    ↓ (tracked by)
XR Interaction Group
```

## 🎯 Các API có sẵn

Từ WebView, gọi:
```javascript
// Chọn model để đặt (index từ 0)
window.vuplex.postMessage({ type: 'selectModel', index: 0 });

// Đặt object tại vị trí tap
window.vuplex.postMessage({ type: 'placeObject' });

// Xóa object đang chọn
window.vuplex.postMessage({ type: 'deleteSelected' });

// Nhân bản object đang chọn
window.vuplex.postMessage({ type: 'duplicateSelected' });

// Xóa tất cả objects
window.vuplex.postMessage({ type: 'clearAll' });
```

## ⚠️ Lưu ý quan trọng

1. **XRGrabInteractable** bắt buộc phải có **Rigidbody** component
2. **XR Interaction Group** phải là con của **XR Origin** để tracking hoạt động
3. Prefabs phải được assign vào **ObjectSpawner.objectPrefabs** array
4. Main Camera phải được assign vào **ObjectSpawner.cameraToFace**

## 🐛 Troubleshooting

**Lỗi: "Object không spawn"**
- Kiểm tra ObjectSpawner có prefabs trong array không
- Kiểm tra ARRaycastManager có hit được plane không

**Lỗi: "Không grab được object"**
- Kiểm tra prefab có XRGrabInteractable component
- Kiểm tra prefab có Rigidbody component
- Kiểm tra XR Interaction Group đã được assign

**Lỗi: "WebView không gọi được Unity"**
- Kiểm tra WebViewManager.arPlacementManager đã assign
- Kiểm tra console log trong Unity
- Kiểm tra WebView đã load xong chưa
