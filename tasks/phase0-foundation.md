# PHASE 0: FOUNDATION (Tuần 1-2)
**Mục tiêu:** Chuẩn bị môi trường phát triển và asset

---

## SETUP MÔI TRƯỜNG

### Cài đặt Unity và Android SDK
- [ ] Download và cài Unity Hub từ https://unity.com/download
- [ ] Cài Unity 6 LTS với modules:
  - [ ] Android Build Support
  - [ ] Android SDK & NDK Tools
  - [ ] OpenJDK
  - [ ] Visual Studio Community 2022
- [ ] Verify: Mở Unity Hub > Installs, thấy Unity 6.x.x với icon Android

### Tạo Project Unity
- [ ] Unity Hub > New Project
- [ ] Chọn template: **3D (URP)**
- [ ] Project name: `TVU_AR_Graduation`
- [ ] Location: Chọn thư mục (tránh OneDrive/Google Drive)
- [ ] Click "Create project"

### Cấu hình Project Settings
- [ ] Edit > Project Settings > Player:
  - [ ] Company Name: `TVU`
  - [ ] Product Name: `AR Graduation`
- [ ] Quality Settings:
  - [ ] Chọn preset "Medium" làm default
  - [ ] Xóa preset "Very High", "Ultra"
- [ ] File > Build Settings:
  - [ ] Chọn "Android"
  - [ ] Click "Switch Platform" (chờ 5-10 phút)

---

## CÀI ĐẶT AR FOUNDATION

### Install Packages
- [ ] Window > Package Manager
- [ ] Packages: "Unity Registry"
- [ ] Tìm và cài "AR Foundation" (version 6.0+)
- [ ] Tìm và cài "ARCore XR Plugin"
- [ ] Tìm và cài "TextMeshPro" (Import TMP Essential Resources)

### Enable ARCore
- [ ] Edit > Project Settings > XR Plug-in Management
- [ ] Click "Install XR Plugin Management"
- [ ] Tab Android (icon robot)
- [ ] Tick ✅ "ARCore"

### Cấu hình Android Settings
- [ ] Project Settings > Player > Tab Android
- [ ] Other Settings:
  - [ ] Minimum API Level: **Android 7.0 (API 24)**
  - [ ] Target API Level: **Automatic**
  - [ ] Scripting Backend: **IL2CPP**
  - [ ] Target Architectures: Tick ✅ **ARM64** (bỏ ARMv7)

---

## THU THẬP IMAGE TARGET

### Chụp ảnh cúp
- [ ] Chuẩn bị:
  - [ ] Đặt cúp trên bàn, nền trơn
  - [ ] Đèn chiếu từ 2 bên (tránh bóng)
- [ ] Chụp 3-5 ảnh:
  - [ ] Góc vuông góc với mặt cúp
  - [ ] Độ phân giải >= 1024x1024px
  - [ ] Format: PNG hoặc JPG
- [ ] Lưu ảnh vào máy tính

### Xử lý ảnh
- [ ] Mở ảnh bằng Paint.NET/Photoshop
- [ ] Crop vuông (1:1 ratio)
- [ ] Resize về 2048x2048px
- [ ] Tăng Contrast lên 10-20%
- [ ] Lưu dạng PNG chất lượng cao

### Import vào Unity
- [ ] Tạo thư mục: `Assets/Images/Targets/`
- [ ] Copy file ảnh vào thư mục
- [ ] Click vào ảnh trong Unity:
  - [ ] Texture Type: **Default**
  - [ ] Max Size: **2048**
  - [ ] Compression: **None**
  - [ ] Click "Apply"

### Tạo Reference Image Library
- [ ] Project window > Chuột phải > Create > XR > Reference Image Library
- [ ] Đặt tên: `CupImageLibrary`
- [ ] Inspector > Click "Add Image"
- [ ] Kéo ảnh cúp vào field "Texture"
- [ ] **Đo chiều rộng thật của cúp** (ví dụ: 15cm)
- [ ] Nhập "Physical Size": `0.15` (đơn vị mét)
- [ ] Đặt tên: "Cup_Front"
- [ ] Verify: Cột "Quality" hiện "Good" hoặc "Excellent"

---

## TÌM 3D ASSETS MIỄN PHÍ

### Model 3D - Rồng
- [ ] Truy cập Sketchfab.com
- [ ] Filter: "Downloadable" + "Free"
- [ ] Tìm "Dragon" hoặc "Chinese Dragon"
- [ ] Yêu cầu:
  - [ ] Poly count < 10,000 triangles
  - [ ] Format: FBX hoặc OBJ
  - [ ] License: CC0 hoặc CC-BY
- [ ] Download và lưu vào `Assets/Models/Dragon/`

### VFX - Particle Effects
- [ ] Truy cập Mixkit.co hoặc Unity Asset Store
- [ ] Tìm và download:
  - [ ] Confetti (pháo giấy)
  - [ ] Fireworks (pháo hoa)
  - [ ] Magic Circle (vòng phép)
- [ ] Lưu vào `Assets/VFX/`

### SFX - Sound Effects
- [ ] Truy cập Freesound.org
- [ ] Tìm và download:
  - [ ] Whoosh sound (âm thanh bay)
  - [ ] Dragon roar (tiếng rống)
  - [ ] Explosion sound (nổ)
- [ ] Format: WAV hoặc MP3
- [ ] Lưu vào `Assets/Audio/SFX/`

---

## SETUP GIT VÀ VERSION CONTROL

### Cài Git
- [ ] Download Git từ: https://git-scm.com/download/win
- [ ] Cài đặt với cấu hình mặc định
- [ ] Verify: Mở CMD, gõ `git --version`

### Tạo GitHub Repository
- [ ] Truy cập https://github.com
- [ ] Click "New repository"
- [ ] Repository name: `tvu-ar-graduation`
- [ ] Chọn "Private"
- [ ] **KHÔNG** tick "Initialize with README"
- [ ] Click "Create repository"

### Tạo .gitignore
- [ ] Mở thư mục project
- [ ] Tạo file `.gitignore` (không có đuôi .txt)
- [ ] Copy nội dung từ: https://github.com/github/gitignore/blob/main/Unity.gitignore
- [ ] Paste và lưu

### Commit code lần đầu
Mở CMD/Terminal trong thư mục project:
```bash
git init
git add .
git commit -m "Initial Unity project with AR Foundation"
git branch -M main
git remote add origin https://github.com/[username]/tvu-ar-graduation.git
git push -u origin main
```
- [ ] Chạy các lệnh trên
- [ ] Verify: Code đã lên GitHub

### Tạo branch dev
```bash
git checkout -b dev
git push -u origin dev
```
- [ ] Chạy lệnh
- [ ] Từ giờ làm việc trên branch `dev`

---

## BUILD APK ĐẦU TIÊN (TEST)

### Tạo Scene test
- [ ] File > New Scene > Basic (Built-in)
- [ ] Xóa "Main Camera" trong Hierarchy
- [ ] GameObject > XR > XR Origin (Action-based)
- [ ] GameObject > XR > AR Session
- [ ] File > Save As: `Assets/Scenes/ARTestScene.unity`

### Add scene vào Build
- [ ] File > Build Settings
- [ ] Click "Add Open Scenes"
- [ ] Verify: ARTestScene ở index 0

### Kết nối điện thoại Android
- [ ] Bật Developer Options:
  - [ ] Settings > About Phone
  - [ ] Tap "Build Number" 7 lần
- [ ] Bật USB Debugging:
  - [ ] Settings > Developer Options
  - [ ] USB Debugging: ON
- [ ] Cắm USB vào máy tính
- [ ] Cho phép "USB Debugging" trên điện thoại
- [ ] Verify: Build Settings > "Run Device" hiện tên máy

### Build and Run
- [ ] Build Settings > Tick "Development Build"
- [ ] Compression Method: **LZ4**
- [ ] Click "Build And Run"
- [ ] Chọn nơi lưu: `Builds/test_v0.1.apk`
- [ ] Chờ build (5-15 phút lần đầu)
- [ ] App tự động cài và chạy trên điện thoại

### Verify
- [ ] App mở được (màn hình đen với camera)
- [ ] Không crash
- [ ] Không có lỗi đỏ trong Console

---

## CHECKLIST HOÀN THÀNH PHASE 0

- [ ] Unity 6 LTS đã cài với Android Build Support
- [ ] Project mới đã tạo với template 3D (URP)
- [ ] AR Foundation + ARCore XR Plugin đã cài
- [ ] XR Plug-in Management đã enable ARCore
- [ ] Git repository đã tạo và commit code lần đầu
- [ ] Ảnh cúp đã chụp và import vào Unity
- [ ] Reference Image Library đã tạo với Physical Size chính xác
- [ ] 3D assets (Rồng, VFX, SFX) đã download
- [ ] Build APK test thành công và chạy được trên điện thoại
- [ ] Không có lỗi đỏ trong Console

**✅ Nếu tất cả đã xong → Chuyển sang `phase1-sprint1.md`**

---

## TROUBLESHOOTING

### Lỗi: "Gradle build failed"
- [ ] Unity Hub > Installs > Icon bánh răng > "Add modules"
- [ ] Tick lại "Android SDK & NDK Tools" và cài lại

### Lỗi: "Unable to list target platforms"
- [ ] Edit > Preferences > External Tools
- [ ] Verify đường dẫn "Android SDK" và "Android NDK"
- [ ] Nếu trống: Click "Download"

### Lỗi: "ARCore not supported"
- [ ] Kiểm tra máy có trong danh sách: https://developers.google.com/ar/devices
- [ ] Cần máy từ 2018+ (Android 7.0+)

---

**Commit code sau khi hoàn thành Phase 0:**
```bash
git add .
git commit -m "Phase 0 complete: Setup environment and assets"
git push origin dev
```
