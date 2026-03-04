# TODO - TVU AR GRADUATION APP

## CẤU TRÚC TASKS

Mỗi phase/sprint có file riêng để dễ quản lý. Làm theo thứ tự từ trên xuống dưới.

---

## 📋 DANH SÁCH FILES

### PHASE 0: FOUNDATION (Tuần 1-2)
📄 **[phase0-foundation.md](phase0-foundation.md)**
- Setup môi trường Unity + Android SDK
- Cài AR Foundation + ARCore
- Thu thập Image Target và 3D assets
- Setup Git và version control
- Build APK test đầu tiên

---

### PHASE 1: CORE FEATURES (Tuần 3-5)

📄 **[phase1-sprint1.md](phase1-sprint1.md)** - AR Image Tracking (Tuần 3)
- Setup AR Scene với XR Origin
- Tạo Reference Image Library
- AR Tracked Image Manager
- Test tracking với Cube
- Script ARImageTracker

📄 **[phase1-sprint2.md](phase1-sprint2.md)** - OCR Integration (Tuần 4)
- Cài ML Kit plugin
- Capture frame và OCR
- Parse tên sinh viên
- Popup xác nhận

📄 **[phase1-sprint3.md](phase1-sprint3.md)** - 3D Content Pipeline (Tuần 5)
- Import và optimize model Rồng
- Setup animation
- Thêm VFX và Audio
- Đo performance (30 FPS)

---

### PHASE 2: POLISH & FEATURES (Tuần 6-8)

📄 **[phase2-sprint4.md](phase2-sprint4.md)** - UI/UX (Tuần 6)
- Custom Splash Screen
- Camera UI (Flash, Shutter)
- Carousel Menu
- Onboarding Tutorial

📄 **[phase2-sprint5.md](phase2-sprint5.md)** - Timeline & VFX (Tuần 7)
- Unity Timeline setup
- Animation sequence
- Depth Mask Shader
- TextMeshPro 3D

📄 **[phase2-sprint6.md](phase2-sprint6.md)** - Capture System (Tuần 8)
- Implement chụp ảnh
- Implement quay video
- Lưu vào Gallery

---

### PHASE 3: QA & OPTIMIZATION (Tuần 9-11)

📄 **[phase3-sprint7.md](phase3-sprint7.md)** - Performance Tuning (Tuần 9)
- Profile và optimize
- Giảm APK size < 120MB

📄 **[phase3-sprint8.md](phase3-sprint8.md)** - Beta Testing (Tuần 10)
- Test với 20-30 người
- Thu thập feedback
- Hotfix bugs

📄 **[phase3-sprint9.md](phase3-sprint9.md)** - Mass Testing (Tuần 11)
- Test 100 máy
- Fix critical bugs
- Build Final v1.0.0

---

### PHASE 4: DEPLOYMENT (Tuần 12)

📄 **[phase4-deployment.md](phase4-deployment.md)**
- Viết tài liệu
- Tạo Landing Page
- Presentation cho Giám đốc RDI
- Phân phối APK

---

## 🎯 CÁCH SỬ DỤNG

1. **Bắt đầu từ Phase 0** → Mở file `phase0-foundation.md`
2. **Tick checkbox** ✅ khi hoàn thành mỗi task
3. **Commit code** sau mỗi sprint:
   ```bash
   git add .
   git commit -m "Sprint X complete: [mô tả]"
   git push origin dev
   ```
4. **Chuyển file tiếp theo** khi hoàn thành sprint

---

## 📊 PROGRESS TRACKING

### Phase 0: Foundation
- [ ] Hoàn thành - Ngày: _____

### Phase 1: Core Features
- [ ] Sprint 1 - AR Tracking - Ngày: _____
- [ ] Sprint 2 - OCR Integration - Ngày: _____
- [ ] Sprint 3 - 3D Pipeline - Ngày: _____

### Phase 2: Polish & Features
- [ ] Sprint 4 - UI/UX - Ngày: _____
- [ ] Sprint 5 - Timeline & VFX - Ngày: _____
- [ ] Sprint 6 - Capture System - Ngày: _____

### Phase 3: QA & Optimization
- [ ] Sprint 7 - Performance - Ngày: _____
- [ ] Sprint 8 - Beta Testing - Ngày: _____
- [ ] Sprint 9 - Mass Testing - Ngày: _____

### Phase 4: Deployment
- [ ] Hoàn thành - Ngày: _____

---

## 💡 TIPS

- Mỗi sprint ước tính **3-5 ngày** làm việc
- **Test trên thiết bị thật** sau mỗi sprint
- **Backup code lên Git** hàng ngày
- Nếu kẹt quá **2 ngày** → Hỏi cộng đồng hoặc đổi approach
- Đọc `docs/SETUP_GUIDE.md` nếu gặp lỗi setup

---

## 📁 CẤU TRÚC THƯ MỤC

```
tasks/
├── README.md (file này - index)
├── phase0-foundation.md
├── phase1-sprint1.md
├── phase1-sprint2.md
├── phase1-sprint3.md
├── phase2-sprint4.md
├── phase2-sprint5.md
├── phase2-sprint6.md
├── phase3-sprint7.md
├── phase3-sprint8.md
├── phase3-sprint9.md
└── phase4-deployment.md
```

---

**🚀 BẮT ĐẦU TỪ:** [phase0-foundation.md](phase0-foundation.md)
