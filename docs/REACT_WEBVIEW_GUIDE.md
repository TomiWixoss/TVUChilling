# HƯỚNG DẪN SỬ DỤNG REACT + UNITY-WEBVIEW

## TỔNG QUAN

Thay vì dùng UI Toolkit của Unity, ta sẽ dùng:
- **unity-webview**: Plugin overlay WebView lên Unity
- **React**: Framework JavaScript để build UI
- **Vite**: Build tool nhanh cho React
- **CSS thuần**: Không cần Tailwind/ShadcnUI

---

## SO SÁNH KIẾN TRÚC

### Cũ: UI Toolkit
```
Unity Scene
└── UI Document (UXML + USS)
    └── C# Controller
```

### Mới: React + WebView + Tailwind + ShadcnUI
```
Unity Scene
└── WebView Overlay (Transparent)
    └── React App
        ├── Tailwind CSS (Styling)
        ├── ShadcnUI (Components)
        ├── Communication: Unity ↔ JavaScript
        └── Build: Vite → Static files
```

---

## ƯU ĐIỂM

✅ **Hot Reload cực nhanh**: Vite HMR < 100ms
✅ **Tailwind CSS**: Utility-first, responsive dễ dàng
✅ **ShadcnUI**: Components đẹp, accessible, customizable
✅ **Ecosystem phong phú**: npm packages, React hooks
✅ **Dễ debug**: Chrome DevTools
✅ **Không cần học USS/UXML**: Dùng React + Tailwind quen thuộc

---

## NHƯỢC ĐIỂM

❌ **Performance**: WebView nặng hơn native UI
❌ **Overlay only**: Không render trong 3D space
❌ **Build phức tạp**: Phải build React trước, rồi import vào Unity
❌ **Bundle size**: React + Tailwind + ShadcnUI ~200KB gzipped

---

## BƯỚC 1: CÀI ĐẶT UNITY-WEBVIEW

### Download plugin

1. Truy cập: https://github.com/gree/unity-webview
2. Download: `dist/unity-webview.unitypackage`
3. Unity > Assets > Import Package > Custom Package
4. Chọn file vừa download > Import All

### Verify
- Có thư mục `Assets/Plugins/WebViewObject.cs`
- Có thư mục `Assets/Plugins/Android/`
- Có thư mục `Assets/Plugins/iOS/`

---

## BƯỚC 2: SETUP REACT PROJECT

### Tạo project React với Vite

Mở Terminal trong thư mục gốc project:

```bash
# Tạo React app
npm create vite@latest webview-ui -- --template react

# Di chuyển vào thư mục
cd webview-ui

# Cài dependencies
npm install
```

### Cấu trúc thư mục
```
TVU_AR_Graduation/
├── Assets/              (Unity)
├── webview-ui/          (React - MỚI)
│   ├── src/
│   │   ├── App.jsx
│   │   ├── App.css
│   │   └── main.jsx
│   ├── public/
│   ├── index.html
│   ├── package.json
│   └── vite.config.js
└── ...
```

---

## BƯỚC 3: CẤU HÌNH VITE BUILD

### Sửa `webview-ui/vite.config.js`

```javascript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  base: './', // Quan trọng: Dùng relative paths
  build: {
    outDir: '../Assets/StreamingAssets/webview', // Build vào Unity
    emptyOutDir: true,
    assetsDir: 'assets',
  }
})
```


---

## BƯỚC 4: CÀI ĐẶT TAILWIND CSS

### Install Tailwind

```bash
cd webview-ui

# Cài Tailwind và dependencies
npm install -D tailwindcss postcss autoprefixer

# Tạo config files
npx tailwindcss init -p
```

### Cấu hình `tailwind.config.js`

```javascript
/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        // TVU Brand colors
        'tvu-navy': '#001F3F',
        'tvu-blue': '#00C8FF',
        'tvu-gold': '#FFD700',
      },
    },
  },
  plugins: [],
}
```

### Thêm Tailwind vào `src/index.css`

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

/* Custom styles cho WebView transparent */
body {
  margin: 0;
  padding: 0;
  background: transparent;
  overflow: hidden;
}

#root {
  width: 100vw;
  height: 100vh;
  background: transparent;
}
```

### Update `src/main.jsx`

```jsx
import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.jsx'
import './index.css' // Import Tailwind

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)
```

---

## BƯỚC 5: CÀI ĐẶT SHADCN/UI

### Install ShadcnUI CLI

```bash
# Vẫn trong thư mục webview-ui
npx shadcn-ui@latest init
```

### Chọn options:
```
✔ Would you like to use TypeScript? … no
✔ Which style would you like to use? › Default
✔ Which color would you like to use as base color? › Slate
✔ Where is your global CSS file? … src/index.css
✔ Would you like to use CSS variables for colors? … yes
✔ Where is your tailwind.config.js located? … tailwind.config.js
✔ Configure the import alias for components: … @/components
✔ Configure the import alias for utils: … @/lib/utils
✔ Are you using React Server Components? … no
```

### Cài components cần thiết

```bash
# Button component
npx shadcn-ui@latest add button

# Dialog (cho popup xác nhận tên)
npx shadcn-ui@latest add dialog

# Input (cho nhập tên)
npx shadcn-ui@latest add input

# Label
npx shadcn-ui@latest add label

# Toast (thông báo)
npx shadcn-ui@latest add toast

# Card (nếu cần)
npx shadcn-ui@latest add card
```

### Update `vite.config.js` cho alias

```javascript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
  plugins: [react()],
  base: './',
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  build: {
    outDir: '../Assets/StreamingAssets/webview',
    emptyOutDir: true,
    assetsDir: 'assets',
  }
})
```

---

## BƯỚC 6: TẠO CAMERA UI VỚI REACT + TAILWIND

### Tạo `src/components/CameraUI.jsx`

```jsx
import React, { useState } from 'react'
import { Button } from '@/components/ui/button'

export default function CameraUI() {
  const [flashOn, setFlashOn] = useState(false)
  const [activeMode, setActiveMode] = useState('cup')
  const [status, setStatus] = useState('Đang tìm kiếm...')

  // Gửi message cho Unity
  const sendToUnity = (action, data) => {
    if (window.unityInstance) {
      window.unityInstance.SendMessage('WebViewManager', action, JSON.stringify(data))
    }
  }

  const handleFlashToggle = () => {
    setFlashOn(!flashOn)
    sendToUnity('OnFlashToggle', { enabled: !flashOn })
  }

  const handleShutterPress = () => {
    sendToUnity('OnShutterPress', {})
  }

  const handleModeChange = (mode) => {
    setActiveMode(mode)
    sendToUnity('OnModeChange', { mode })
  }

  return (
    <div className="fixed inset-0 pointer-events-none">
      {/* Top Bar */}
      <div className="absolute top-0 left-0 right-0 h-[120px] bg-black/50 flex items-center justify-between px-5 pointer-events-auto">
        {/* Flash Button */}
        <Button
          variant="ghost"
          size="icon"
          className={`w-20 h-20 rounded-full text-4xl ${
            flashOn ? 'bg-white/40' : 'bg-white/20'
          } hover:bg-white/30`}
          onClick={handleFlashToggle}
        >
          💡
        </Button>

        {/* Status Label */}
        <span className="text-white text-3xl font-medium">
          {status}
        </span>
      </div>

      {/* Shutter Button */}
      <div className="absolute bottom-[125px] left-1/2 -translate-x-1/2 pointer-events-auto">
        <button
          className="w-[200px] h-[200px] rounded-full bg-white border-8 border-white/50 
                     active:bg-red-500 transition-colors shadow-2xl"
          onMouseDown={handleShutterPress}
          onTouchStart={handleShutterPress}
        >
        </button>
      </div>

      {/* Mode Carousel */}
      <div className="absolute bottom-[25px] left-0 right-0 h-20 flex items-center justify-center gap-3 pointer-events-auto">
        <Button
          variant={activeMode === 'cup' ? 'default' : 'outline'}
          className={`w-[250px] h-20 text-2xl rounded-full ${
            activeMode === 'cup' 
              ? 'bg-tvu-blue border-tvu-blue' 
              : 'bg-white/20 border-white text-white hover:bg-white/30'
          }`}
          onClick={() => handleModeChange('cup')}
        >
          Cúp Dọc
        </Button>

        <Button
          variant={activeMode === 'card' ? 'default' : 'outline'}
          className={`w-[250px] h-20 text-2xl rounded-full ${
            activeMode === 'card' 
              ? 'bg-tvu-blue border-tvu-blue' 
              : 'bg-white/20 border-white text-white hover:bg-white/30'
          }`}
          onClick={() => handleModeChange('card')}
        >
          Bìa Ngang
        </Button>

        <Button
          variant={activeMode === 'mix' ? 'default' : 'outline'}
          className={`w-[250px] h-20 text-2xl rounded-full ${
            activeMode === 'mix' 
              ? 'bg-tvu-blue border-tvu-blue' 
              : 'bg-white/20 border-white text-white hover:bg-white/30'
          }`}
          onClick={() => handleModeChange('mix')}
        >
          Mix Hiệu Ứng
        </Button>
      </div>
    </div>
  )
}
```

### Update `src/App.jsx`

```jsx
import React from 'react'
import CameraUI from './components/CameraUI'

function App() {
  return (
    <div className="w-screen h-screen">
      <CameraUI />
    </div>
  )
}

export default App
```

---

## BƯỚC 7: TẠO POPUP XÁC NHẬN VỚI SHADCN DIALOG

### Tạo `src/components/ConfirmationDialog.jsx`

```jsx
import React, { useState, useEffect } from 'react'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

export default function ConfirmationDialog() {
  const [open, setOpen] = useState(false)
  const [studentName, setStudentName] = useState('')

  // Lắng nghe message từ Unity
  useEffect(() => {
    window.showConfirmationDialog = (ocrName) => {
      setStudentName(ocrName)
      setOpen(true)
    }

    return () => {
      delete window.showConfirmationDialog
    }
  }, [])

  const handleConfirm = () => {
    // Gửi tên đã confirm về Unity
    if (window.unityInstance) {
      window.unityInstance.SendMessage(
        'WebViewManager', 
        'OnNameConfirmed', 
        studentName
      )
    }
    setOpen(false)
  }

  const handleCancel = () => {
    setOpen(false)
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogContent className="sm:max-w-[800px] bg-white rounded-3xl p-10">
        <DialogHeader>
          <DialogTitle className="text-5xl text-center mb-8">
            Hệ thống nhận diện bạn là:
          </DialogTitle>
        </DialogHeader>

        <div className="grid gap-6 py-4">
          <div className="grid gap-3">
            <Label htmlFor="name" className="text-3xl">
              Tên của bạn
            </Label>
            <Input
              id="name"
              value={studentName}
              onChange={(e) => setStudentName(e.target.value)}
              className="h-20 text-4xl"
              placeholder="Nhập tên của bạn"
            />
          </div>
        </div>

        <DialogFooter className="flex gap-4">
          <Button
            variant="destructive"
            onClick={handleCancel}
            className="flex-1 h-24 text-3xl rounded-2xl"
          >
            Hủy
          </Button>
          <Button
            onClick={handleConfirm}
            className="flex-1 h-24 text-3xl rounded-2xl bg-green-600 hover:bg-green-700"
          >
            Kích hoạt
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
```

### Update `src/App.jsx`

```jsx
import React from 'react'
import CameraUI from './components/CameraUI'
import ConfirmationDialog from './components/ConfirmationDialog'
import { Toaster } from '@/components/ui/toaster'

function App() {
  return (
    <div className="w-screen h-screen">
      <CameraUI />
      <ConfirmationDialog />
      <Toaster />
    </div>
  )
}

export default App
```

---

## BƯỚC 8: BUILD REACT APP

### Development mode (Hot reload)

```bash
cd webview-ui
npm run dev
```

Mở browser: http://localhost:5173 để test UI

### Production build

```bash
cd webview-ui
npm run build
```

Files sẽ được build vào: `Assets/StreamingAssets/webview/`

---

## BƯỚC 9: TÍCH HỢP VÀO UNITY

### Tạo script `WebViewManager.cs`

```csharp
using UnityEngine;
using System.Collections;

public class WebViewManager : MonoBehaviour
{
    private WebViewObject webViewObject;
    
    void Start()
    {
        StartCoroutine(InitWebView());
    }
    
    IEnumerator InitWebView()
    {
        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        
        webViewObject.Init(
            cb: (msg) =>
            {
                Debug.Log($"WebView callback: {msg}");
            },
            err: (msg) =>
            {
                Debug.LogError($"WebView error: {msg}");
            },
            started: (msg) =>
            {
                Debug.Log("WebView started");
            },
            hooked: (msg) =>
            {
                Debug.Log("WebView hooked");
            },
            ld: (msg) =>
            {
                Debug.Log($"WebView loaded: {msg}");
            },
            enableWKWebView: true,
            transparent: true // Quan trọng: Làm WebView trong suốt
        );
        
        // Set margins (fullscreen)
        webViewObject.SetMargins(0, 0, 0, 0);
        
        // Load HTML từ StreamingAssets
        string url = GetWebViewURL();
        webViewObject.LoadURL(url);
        
        webViewObject.SetVisibility(true);
        
        yield return null;
    }
    
    string GetWebViewURL()
    {
        #if UNITY_EDITOR
        // Development: Load từ Vite dev server
        return "http://localhost:5173";
        #elif UNITY_ANDROID
        // Production: Load từ StreamingAssets
        return "file:///android_asset/webview/index.html";
        #elif UNITY_IOS
        return Application.streamingAssetsPath + "/webview/index.html";
        #else
        return Application.streamingAssetsPath + "/webview/index.html";
        #endif
    }
    
    // Callbacks từ React
    public void OnFlashToggle(string json)
    {
        Debug.Log($"Flash toggled: {json}");
        // TODO: Implement flash logic
    }
    
    public void OnShutterPress(string json)
    {
        Debug.Log("Shutter pressed");
        // TODO: Implement capture logic
    }
    
    public void OnModeChange(string json)
    {
        Debug.Log($"Mode changed: {json}");
        // TODO: Enable/disable AR targets
    }
    
    public void OnNameConfirmed(string studentName)
    {
        Debug.Log($"Name confirmed: {studentName}");
        // TODO: Trigger AR animation với tên này
    }
    
    // Gọi từ Unity để hiện popup
    public void ShowConfirmationDialog(string ocrName)
    {
        string js = $"window.showConfirmationDialog('{ocrName}')";
        webViewObject.EvaluateJS(js);
    }
}
```

### Thêm vào Scene

1. Hierarchy > Create Empty GameObject
2. Đặt tên: `WebViewManager`
3. Add Component: `WebViewManager` script

---

## BƯỚC 10: TEST

### Test trong Unity Editor

1. Chạy Vite dev server:
   ```bash
   cd webview-ui
   npm run dev
   ```

2. Play Unity scene
3. WebView sẽ load từ `http://localhost:5173`
4. Thay đổi code React → Hot reload ngay lập tức

### Test trên Android

1. Build React:
   ```bash
   cd webview-ui
   npm run build
   ```

2. Verify files trong `Assets/StreamingAssets/webview/`
3. Build APK từ Unity
4. Test trên thiết bị

---

## COMMUNICATION: UNITY ↔ REACT

### Unity → React (EvaluateJS)

```csharp
// Trong Unity C#
webViewObject.EvaluateJS("window.showConfirmationDialog('NGUYỄN VĂN A')");
```

### React → Unity (SendMessage)

```javascript
// Trong React
window.unityInstance.SendMessage('WebViewManager', 'OnNameConfirmed', 'NGUYỄN VĂN A')
```

---

## TROUBLESHOOTING

### WebView không hiện

- Verify `transparent: true` trong Init
- Check margins: `SetMargins(0, 0, 0, 0)`
- Check URL đúng chưa

### Hot reload không hoạt động

- Verify Vite dev server đang chạy
- Check URL trong Editor: `http://localhost:5173`

### Build lỗi

- Verify `base: './'` trong `vite.config.js`
- Check outDir đúng: `../Assets/StreamingAssets/webview`

---

**✅ Hoàn thành! Bây giờ bạn có React + Tailwind + ShadcnUI chạy trong Unity WebView**
