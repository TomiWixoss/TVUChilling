---
inclusion: auto
---

# TVU AR Graduation - Project Structure

## Tổng quan
App AR tốt nghiệp TVU sử dụng Unity AR Foundation + React WebView UI.

## Tech Stack
- **Unity 6000.3.10f1**: AR Foundation, Image Tracking
- **React 19 + Vite 7**: WebView UI
- **TypeScript**: Type safety
- **Tailwind CSS 4**: Styling
- **ShadcnUI (Nova preset)**: UI components
- **Lucide React**: Icons (SVG, không dùng emoji)
- **unity-webview**: Bridge Unity ↔ React

## Cấu trúc thư mục

### Unity Project
```
TVU_AR_Graduation/
├── Assets/
│   ├── Scripts/
│   │   ├── ARImageTracker.cs       # AR image tracking + spawn prefab
│   │   ├── CameraController.cs     # Chụp ảnh, quay video, flash
│   │   └── WebViewManager.cs       # Quản lý WebView + Unity ↔ JS bridge
│   ├── Plugins/
│   │   └── Android/
│   │       ├── mainTemplate.gradle # Android dependencies
│   │       └── (unity-webview plugin files)
│   ├── Scenes/
│   │   └── ARTestScene.unity       # Main AR scene
│   ├── Prefabs/
│   │   └── TestCube.prefab         # AR tracked object
│   ├── AR/
│   │   └── CupImageLibrary.asset   # AR image library
│   └── StreamingAssets/
│       └── webview/                # React build output
```

### React WebView UI
```
webview-ui/
├── src/
│   ├── components/
│   │   └── CameraControls.tsx      # Camera UI (chụp/quay/flash)
│   ├── App.tsx                     # Main app
│   └── index.css                   # Transparent background
├── vite.config.ts                  # Build vào StreamingAssets
└── package.json
```

## Workflow

### AR Tracking
1. User chĩa camera vào bằng tốt nghiệp
2. `ARImageTracker` detect image → Spawn prefab
3. Prefab hiện 3D object trên bằng

### Camera Controls
1. React UI hiện controls ở dưới cùng (như app Camera)
2. User bấm nút → Gọi Unity qua `window.Unity.call()`
3. `WebViewManager` nhận message → Forward đến `CameraController`
4. `CameraController` xử lý:
   - Chụp ảnh: Lưu vào DCIM, refresh Android gallery
   - Quay video: TODO (cần plugin)
   - Flash: Bật/tắt đèn flash Android native

## Unity ↔ React Communication

### React → Unity
```typescript
window.Unity.call('onCapturePhoto', '')
window.Unity.call('onRecordToggle', 'start' | 'stop')
window.Unity.call('onFlashToggle', 'on' | 'off')
```

### Unity → React
```csharp
webViewObject.EvaluateJS("window.functionName('data')")
```

## Build Process

### React
```bash
cd webview-ui
npm run build  # Build vào ../TVU_AR_Graduation/Assets/StreamingAssets/webview/
```

### Unity
- Editor: WebView load từ Vite dev server (http://localhost:5173)
- Android: WebView load từ StreamingAssets

## Notes
- WebView mặc định hiện (transparent background)
- Camera controls luôn hiện ở dưới cùng
- Không có OCR/ML Kit (đã xóa vì không ổn định)
- Flash chỉ hoạt động trên Android device
- Video recording chưa implement (cần plugin)
