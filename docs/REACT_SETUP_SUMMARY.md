# TÓM TẮT SETUP REACT + TAILWIND + SHADCN/UI

## ✅ ĐÃ HOÀN THÀNH

### 1. Khởi tạo Project
```bash
npx shadcn@latest init -t vite
```
- ✅ React 19 + Vite 7 + TypeScript
- ✅ Tailwind CSS 4 (tự động cài)
- ✅ ShadcnUI với preset Nova
- ✅ Button component có sẵn

### 2. Cài thêm Components
```bash
npx shadcn@latest add dialog input label sonner
npm install zustand
```
- ✅ Dialog - Popup modal
- ✅ Input - Text input field
- ✅ Label - Form labels
- ✅ Sonner - Toast notifications
- ✅ Zustand - State management

### 3. Cấu hình Vite
File: `webview-ui/vite.config.ts`
```typescript
build: {
  outDir: '../Assets/StreamingAssets/webview',
  emptyOutDir: true,
  assetsDir: 'assets',
}
```
→ Build vào Unity StreamingAssets

### 4. Tạo UI Components

#### OCR Store (State Management)
File: `webview-ui/src/store/useOCRStore.ts`
- Quản lý state dialog (open/close)
- Quản lý tên sinh viên

#### OCR Dialog Component
File: `webview-ui/src/components/OCRDialog.tsx`
- Popup xác nhận tên OCR
- Input field để sửa tên
- Buttons: Hủy / Kích hoạt
- Gửi tên về Unity khi confirm

#### Unity Bridge Hook
File: `webview-ui/src/hooks/useUnityBridge.ts`
- Expose `window.showOCRDialog()` cho Unity
- TypeScript types cho Unity communication

#### App Component
File: `webview-ui/src/App.tsx`
- Test button để mở dialog
- Hiển thị Unity bridge status
- Tích hợp OCRDialog + Toaster

---

## 🎯 CÁCH SỬ DỤNG

### Development Mode
```bash
cd webview-ui
npm run dev
```
→ Mở http://localhost:5173

### Production Build
```bash
cd webview-ui
npm run build
```
→ Files build vào `Assets/StreamingAssets/webview/`

### Test UI
1. Mở browser: http://localhost:5173
2. Click button "🧪 Test OCR Dialog"
3. Popup hiện với tên "NGUYỄN VĂN A"
4. Sửa tên → Click "Kích hoạt"
5. Check console log

---

## 🔗 UNITY INTEGRATION

### Từ Unity gọi React
```csharp
// Trong Unity C#
webViewObject.EvaluateJS("window.showOCRDialog('NGUYỄN VĂN A')");
```

### Từ React gọi Unity
```typescript
// Trong React
window.unityInstance.SendMessage(
  'WebViewManager',
  'OnNameConfirmed',
  studentName
)
```

---

## 📁 CẤU TRÚC PROJECT

```
webview-ui/
├── src/
│   ├── components/
│   │   ├── ui/              (ShadcnUI components)
│   │   │   ├── button.tsx
│   │   │   ├── dialog.tsx
│   │   │   ├── input.tsx
│   │   │   ├── label.tsx
│   │   │   └── sonner.tsx
│   │   ├── OCRDialog.tsx    (Custom component)
│   │   └── theme-provider.tsx
│   ├── hooks/
│   │   └── useUnityBridge.ts
│   ├── store/
│   │   └── useOCRStore.ts
│   ├── lib/
│   │   └── utils.ts
│   ├── App.tsx
│   ├── main.tsx
│   └── index.css
├── vite.config.ts
├── components.json
├── package.json
└── tsconfig.json
```

---

## 🚀 TIẾP THEO

### Cần làm:
1. ✅ OCR Dialog - DONE
2. ⏳ Camera UI (Top bar, Shutter, Mode carousel)
3. ⏳ Unity WebViewManager.cs script
4. ⏳ Tích hợp với AR tracking
5. ⏳ Test trên Android device

---

## 🐛 TROUBLESHOOTING

### Build không vào Unity folder
→ Check `vite.config.ts` có `outDir` đúng không

### Dialog không hiện
→ Check console log, verify `useUnityBridge()` đã gọi

### TypeScript lỗi `window.unityInstance`
→ Đã declare trong `useUnityBridge.ts`

---

**✅ Setup hoàn tất! UI OCR Dialog đã sẵn sàng test.**
