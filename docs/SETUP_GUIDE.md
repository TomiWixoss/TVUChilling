# HƯỚNG DẪN SETUP CHI TIẾT - TVU AR GRADUATION APP

## MỤC LỤC
1. [Cài đặt Unity và Android SDK](#1-cài-đặt-unity-và-android-sdk)
2. [Tạo Project Unity mới](#2-tạo-project-unity-mới)
3. [Cài đặt AR Foundation](#3-cài-đặt-ar-foundation)
4. [Setup Git và Version Control](#4-setup-git-và-version-control)
5. [Chuẩn bị Image Target](#5-chuẩn-bị-image-target)
6. [Build APK đầu tiên](#6-build-apk-đầu-tiên)

---

## 1. CÀI ĐẶT UNITY VÀ ANDROID SDK

### Bước 1.1: Download Unity Hub
1. Truy cập: https://unity.com/download
2. Click "Download Unity Hub"
3. Cài đặt Unity Hub (file .exe)
4. Mở Unity Hub, đăng nhập hoặc tạo tài khoản Unity (miễn phí)

### Bước 1.2: Cài Unity 6 LTS
1. Trong Unity Hub, click tab "Installs"
2. Click "Install Editor" > Chọn "Unity 6 LTS" (Long Term Support)
3. Tick các module:
   - ✅ Android Build Support
     - ✅ Android SDK & NDK Tools
     - ✅ OpenJDK
   - ✅ Visual Studio Community 2022 (nếu chưa có)
4. Click "Install" và chờ (khoảng 30-60 phút tùy mạng)

### Bước 1.3: Verify cài đặt
1. Sau khi cài xong, vào Unity Hub > Installs
2. Verify có Unity 6.x.x với icon Android màu xanh lá
3. Click vào icon bánh răng > "Add modules" để kiểm tra

---

## 2. TẠO PROJECT UNITY MỚI

### Bước 2.1: Tạo project
1. Unity Hub > Tab "Projects"
2. Click "New project"
3. Chọn template: **3D (URP)** - Universal Render Pipeline
   - Lý do: URP tối ưu cho mobile, performance tốt hơn Built-in
4. Điền thông tin:
   - Project name: `TVU_AR_Graduation`
   - Location: Chọn thư mục (tránh OneDrive/Google Drive)
5. Click "Create project"

### Bước 2.2: Cấu hình Project Settings
1. Mở Unity Editor
2. Edit > Project Settings
3. **Player Settings:**
   - Company Name: `TVU`
   - Product Name: `AR Graduation`
   - Icon: (để sau, khi có logo)
4. **Quality Settings:**
   - Chọn preset "Medium" làm default cho Android
   - Xóa các preset "Very High", "Ultra" (không cần cho mobile)

### Bước 2.3: Switch sang Android Platform
1. File > Build Settings
2. Chọn "Android" trong danh sách Platform
3. Click "Switch Platform" (chờ 5-10 phút)
4. Verify: Icon Unity bên cạnh Android phải sáng lên

---

## 3. CÀI ĐẶT AR FOUNDATION

### Bước 3.1: Mở Package Manager
1. Window > Package Manager
2. Dropdown "Packages:" chọn "Unity Registry"

### Bước 3.2: Cài AR Foundation
1. Tìm "AR Foundation" trong list
2. Click vào > Click "Install"
3. Chờ cài đặt xong (1-2 phút)

### Bước 3.3: Cài ARCore XR Plugin
1. Vẫn trong Package Manager
2. Tìm "ARCore XR Plugin"
3. Click "Install"

### Bước 3.4: Cài TextMeshPro
1. Tìm "TextMeshPro" (thường đã có sẵn)
2. Nếu chưa có: Click "Install"
3. Sau khi cài, sẽ có popup "Import TMP Essentials"
4. Click "Import TMP Essential Resources"

### Bước 3.5: Enable ARCore trong XR Settings
1. Edit > Project Settings
2. Tab "XR Plug-in Management"
3. Click "Install XR Plugin Management" (nếu chưa có)
4. Chọn tab Android (icon robot)
5. Tick ✅ "ARCore"

### Bước 3.6: Cấu hình Android Settings
1. Vẫn trong Project Settings > Player
2. Tab Android (icon robot)
3. **Other Settings:**
   - Minimum API Level: **Android 7.0 'Nougat' (API level 24)**
   - Target API Level: **Automatic (highest installed)**
   - Scripting Backend: **IL2CPP**
   - Target Architectures: Tick ✅ **ARM64** (bỏ ARMv7)
4. **Publishing Settings:**
   - Create new Keystore (để sau, khi build release)

---

## 4. SETUP GIT VÀ VERSION CONTROL

### Bước 4.1: Cài Git
1. Download Git từ: https://git-scm.com/download/win
2. Cài đặt với cấu hình mặc định
3. Verify: Mở Command Prompt, gõ `git --version`

### Bước 4.2: Tạo GitHub Repository
1. Truy cập: https://github.com
2. Đăng nhập > Click "New repository"
3. Repository name: `tvu-ar-graduation`
4. Chọn "Private" (nếu không muốn public)
5. **KHÔNG** tick "Initialize with README"
6. Click "Create repository"

### Bước 4.3: Tạo .gitignore cho Unity
1. Mở thư mục project (nơi có folder Assets, Packages...)
2. Tạo file mới tên `.gitignore` (không có đuôi .txt)
3. Copy nội dung từ: https://github.com/github/gitignore/blob/main/Unity.gitignore
4. Paste vào file `.gitignore` và lưu

### Bước 4.4: Commit code lần đầu
Mở Command Prompt/Terminal trong thư mục project:
```bash
git init
git add .
git commit -m "Initial Unity project with AR Foundation"
git branch -M main
git remote add origin https://github.com/[username]/tvu-ar-graduation.git
git push -u origin main
```

### Bước 4.5: Tạo branch dev
```bash
git checkout -b dev
git push -u origin dev
```
Từ giờ làm việc trên branch `dev`, chỉ merge vào `main` khi hoàn thành sprint.

---

## 5. CHUẨN BỊ IMAGE TARGET

### Bước 5.1: Chụp ảnh cúp
**Yêu cầu:**
- Độ phân giải: Tối thiểu 1024x1024px (khuyến nghị 2048x2048px)
- Format: PNG hoặc JPG
- Ánh sáng: Đều, không bị bóng che mặt cúp
- Góc chụp: Vuông góc với mặt cúp (không chụp nghiêng)
- Background: Trơn, tương phản với cúp (ví dụ: cúp vàng trên nền đen)

**Cách chụp tốt nhất:**
1. Đặt cúp trên bàn, nền trơn
2. Dùng đèn bàn chiếu từ 2 bên (tránh bóng)
3. Chụp từ trên xuống, camera song song với mặt cúp
4. Chụp 3-5 ảnh khác nhau (góc hơi khác nhau)

### Bước 5.2: Xử lý ảnh
1. Mở ảnh bằng Paint.NET hoặc Photoshop
2. Crop vuông (1:1 ratio)
3. Resize về 2048x2048px
4. Tăng Contrast lên 10-20% (giúp tracking tốt hơn)
5. Lưu dạng PNG (chất lượng cao)

### Bước 5.3: Import vào Unity
1. Tạo thư mục: `Assets/Images/Targets/`
2. Copy file ảnh vào thư mục này
3. Trong Unity, click vào ảnh
4. Inspector > Texture Type: **Default**
5. Max Size: **2048**
6. Compression: **None** (để chất lượng cao cho tracking)
7. Click "Apply"

### Bước 5.4: Tạo Reference Image Library
1. Trong Project window, chuột phải > Create > XR > Reference Image Library
2. Đặt tên: `CupImageLibrary`
3. Click vào library vừa tạo
4. Inspector > Click "Add Image"
5. Kéo ảnh cúp vào field "Texture"
6. **Quan trọng:** Nhập "Physical Size"
   - Đo chiều rộng thật của mặt cúp (ví dụ: 15cm)
   - Nhập: 0.15 (đơn vị mét)
   - Lý do: AR Foundation cần biết kích thước thật để tính scale
7. Đặt tên: "Cup_Front"
8. Click "Save"

### Bước 5.5: Test chất lượng Image Target
1. Trong Inspector của library, xem cột "Quality"
2. Nếu hiện "Low" hoặc "Poor":
   - Ảnh thiếu texture (quá trơn)
   - Tăng contrast hoặc chụp lại
3. Mục tiêu: "Good" hoặc "Excellent"

---

## 6. BUILD APK ĐẦU TIÊN

### Bước 6.1: Tạo Scene test đơn giản
1. File > New Scene > Basic (Built-in)
2. Xóa "Main Camera" trong Hierarchy
3. GameObject > XR > XR Origin (Action-based)
4. GameObject > XR > AR Session
5. File > Save As: `Assets/Scenes/ARTestScene.unity`

### Bước 6.2: Add scene vào Build Settings
1. File > Build Settings
2. Click "Add Open Scenes"
3. Verify: ARTestScene xuất hiện trong list với index 0

### Bước 6.3: Cấu hình Build
1. Vẫn trong Build Settings
2. Verify Platform là "Android" (có icon Unity bên cạnh)
3. **Development Build:** Tick ✅ (để debug)
4. **Compression Method:** LZ4 (nhanh hơn)

### Bước 6.4: Kết nối điện thoại
1. Bật "Developer Options" trên Android:
   - Settings > About Phone
   - Tap "Build Number" 7 lần
2. Bật "USB Debugging":
   - Settings > Developer Options > USB Debugging: ON
3. Cắm USB vào máy tính
4. Trên điện thoại: Cho phép "USB Debugging" (popup)
5. Verify: Trong Build Settings, dropdown "Run Device" phải hiện tên máy

### Bước 6.5: Build and Run
1. Click "Build And Run"
2. Chọn nơi lưu APK (ví dụ: `Builds/test_v1.apk`)
3. Chờ build (5-15 phút lần đầu)
4. App sẽ tự động cài và chạy trên điện thoại

### Bước 6.6: Verify
- App mở được (màn hình đen với camera)
- Không crash
- Logcat không có lỗi đỏ

**Nếu gặp lỗi:**
- "Unable to install APK": Gỡ app cũ trước
- "ARCore not supported": Máy không hỗ trợ ARCore (cần máy khác)
- "Permission denied": Bật lại USB Debugging

---

## TROUBLESHOOTING THƯỜNG GẶP

### Lỗi: "Gradle build failed"
**Nguyên nhân:** Thiếu Android SDK hoặc NDK
**Giải pháp:**
1. Unity Hub > Installs > Click icon bánh răng
2. "Add modules" > Tick lại "Android SDK & NDK Tools"
3. Cài lại

### Lỗi: "Unable to list target platforms"
**Nguyên nhân:** Unity không tìm thấy Android SDK
**Giải pháp:**
1. Edit > Preferences > External Tools
2. Verify đường dẫn "Android SDK" và "Android NDK"
3. Nếu trống: Click "Download" để Unity tự tải

### Lỗi: "ARCore not supported on this device"
**Nguyên nhân:** Máy không hỗ trợ ARCore
**Giải pháp:**
- Kiểm tra danh sách máy hỗ trợ: https://developers.google.com/ar/devices
- Cần máy từ 2018 trở lên (Android 7.0+)

### Lỗi: "Shader compilation failed"
**Nguyên nhân:** URP shader không tương thích
**Giải pháp:**
1. Edit > Render Pipeline > Universal Render Pipeline > Upgrade Project Materials
2. Chờ Unity convert shader

---

## CHECKLIST HOÀN THÀNH PHASE 0

- [ ] Unity 6 LTS đã cài với Android Build Support
- [ ] Project mới đã tạo với template 3D (URP)
- [ ] AR Foundation + ARCore XR Plugin đã cài
- [ ] XR Plug-in Management đã enable ARCore
- [ ] Git repository đã tạo và commit code lần đầu
- [ ] Ảnh cúp đã chụp và import vào Unity
- [ ] Reference Image Library đã tạo với Physical Size chính xác
- [ ] Build APK test thành công và chạy được trên điện thoại
- [ ] Không có lỗi đỏ trong Console

**Nếu tất cả đã xong → Chuyển sang Phase 1: Sprint 1**

---

## TÀI LIỆU THAM KHẢO

- Unity AR Foundation Manual: https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.0/manual/index.html
- ARCore Developer Guide: https://developers.google.com/ar/develop/unity-arf
- Unity Learn - AR Tutorial: https://learn.unity.com/course/augmented-reality
- Git Basics: https://git-scm.com/book/en/v2/Getting-Started-Git-Basics

---

**LƯU Ý:**
- Backup project thường xuyên (Git commit mỗi ngày)
- Test trên thiết bị thật, không chỉ Unity Editor
- Nếu kẹt quá 2 giờ → Hỏi trên Unity Forum hoặc Stack Overflow
- Đọc Console log kỹ, thường có gợi ý fix lỗi
