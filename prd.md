Dưới đây là tài liệu **PRD (Product Requirements Document - Tài liệu Đặc tả Yêu cầu Sản phẩm)** được viết theo chuẩn doanh nghiệp công nghệ. Tài liệu này dùng để trình bày với Giám đốc RDI và làm kim chỉ nam cho quá trình phát triển.

---

# TÀI LIỆU ĐẶC TẢ SẢN PHẨM (PRD)
**Tên dự án:** Ứng dụng AR Lễ Tốt Nghiệp (TVU & DRI AR Graduation App)
**Nền tảng:** Android (Production), iOS (Future)
**Phiên bản tài liệu:** 2.0
**Loại dự án:** Thực tập - Sản phẩm thực tế
**Người phát triển:** Solo Developer
**Người phê duyệt:** Giám đốc RDI

---

## 1. TỔNG QUAN DỰ ÁN (EXECUTIVE SUMMARY)
### 1.1. Vấn đề (Problem)
Những vật phẩm lưu niệm tốt nghiệp truyền thống (cúp gỗ, bìa đồ án) mang tính tĩnh, thiếu sự tương tác và khó tạo ra hiệu ứng lan truyền (viral) trên mạng xã hội đối với thế hệ sinh viên Gen Z.
### 1.2. Giải pháp (Solution)
Một ứng dụng di động sử dụng công nghệ Thực tế Tăng cường (AR). Khi chĩa camera vào cúp hoặc bìa đồ án, ứng dụng sẽ quét và hiển thị các hiệu ứng 3D hoành tráng (rồng bay, pháo hoa), đồng thời sử dụng trí tuệ nhân tạo (AI/OCR) để đọc tên sinh viên trên cúp thật và hiển thị tên đó dưới dạng 3D lơ lửng.
### 1.3. Mục tiêu kinh doanh / Giá trị mang lại (Value Proposition)
*   **Với sinh viên:** Tạo trải nghiệm "Wow", cá nhân hóa cảm xúc trong ngày tốt nghiệp. Cung cấp công cụ quay video/chụp ảnh AR độc đáo để đăng TikTok/Facebook.
*   **Với nhà trường (TVU & DRI):** Nâng tầm hình ảnh trường đại học đổi mới sáng tạo, ứng dụng công nghệ cao. Tăng độ nhận diện thương hiệu tự nhiên qua các video sinh viên chia sẻ.

---

## 2. CHÂN DUNG NGƯỜI DÙNG (USER PERSONAS)
*   **Người dùng chính (Primary):** Sinh viên tốt nghiệp TVU (22-25 tuổi), yêu thích công nghệ, có nhu cầu cao về việc lưu giữ kỷ niệm và chia sẻ hình ảnh lên mạng xã hội.
*   **Người dùng phụ (Secondary):** Phụ huynh, bạn bè muốn dùng app để quay phim, chụp ảnh kỷ niệm cho sinh viên.

---

## 3. CÂU CHUYỆN NGƯỜI DÙNG (USER STORIES)
1.  *Là một sinh viên*, tôi muốn ứng dụng khởi động với logo trường thay vì logo Unity, *để* cảm thấy sự chuyên nghiệp và tự hào.
2.  *Là một sinh viên*, tôi muốn ứng dụng tự đọc tên tôi trên cúp, *để* tôi không phải nhập tay mất thời gian.
3.  *Là một người dùng*, tôi muốn có thể tự chọn hiệu ứng (Rồng hoặc Phượng Hoàng), *để* video của tôi không bị đụng hàng với người khác.
4.  *Là một người dùng*, tôi muốn có nút quay video màn hình ngay trong app, *để* dễ dàng lưu lại khoảnh khắc AR chuyển động và share lên mạng.

---

## 4. YÊU CẦU CHỨC NĂNG CHI TIẾT (FUNCTIONAL REQUIREMENTS)

### 4.1. Màn hình Splash Screen (Intro)
*   **Mô tả:** Màn hình khởi động của ứng dụng, loại bỏ hoàn toàn logo mặc định của nền tảng.
*   **Luồng hoạt động:**
    *   Mở app -> Nền màu Đen/Xanh Navy.
    *   Logo TVU và DRI (căn giữa) từ từ hiện rõ (Fade in) trong 1.5s.
    *   Giữ nguyên trong 2s -> Mờ dần (Fade out) và tự động chuyển sang Giao diện Camera.

### 4.2. Giao diện người dùng cốt lõi (Main Camera UI)
*   **Công nghệ:** UI Toolkit (UIElements) với UXML layout và USS styling
*   **Thiết kế UX:** Tối giản, dạng kính ngắm trong suốt (như app Instagram Story).
*   **Thành phần UI:**
    *   **Top Bar:** Nút Bật/Tắt Đèn Flash điện thoại (Hỗ trợ quét trong hội trường tối). Dòng chữ trạng thái *"Đang tìm kiếm..."*.
    *   **Nút Shutter (Chính giữa dưới):** Chạm (Tap) để chụp ảnh -> Chớp sáng màn hình -> Lưu vào thư viện máy. Nhấn giữ (Hold & Press) để quay video -> Viền nút chuyển đỏ -> Thả tay ra để kết thúc lưu video.
    *   **Carousel Menu (Thanh cuộn ngang):** Chứa các chế độ AR: `[Cúp Dọc]` | `[Bìa Đồ Án Ngang]` | `[Mix Hiệu Ứng]`.
*   **Hot Reload:** Thay đổi USS/UXML không cần recompile, tăng tốc độ phát triển UI.

### 4.3. Chức năng Nhận diện (Image Tracking & AR Engine)
*   **Mode Cúp Dọc:** Camera tìm kiếm mục tiêu hình ảnh mặt trước cúp gỗ. Hệ tọa độ 3D được thiết lập nghiêng 90 độ so với mặt đất ảo (phù hợp với vật thể đứng).
*   **Mode Bìa Ngang:** Camera tìm kiếm mục tiêu hình ảnh bìa đồ án. Hệ tọa độ 3D nằm phẳng ngang mặt bàn.
*   **Logic:** Chuyển đổi Mode trên UI sẽ `Enable/Disable` các Target tương ứng trong lõi AR để tránh xung đột nhận diện.

### 4.4. Hệ thống Nhận diện Ký tự (OCR Engine) & Fail-Safe
*   **Công nghệ:** Tích hợp Google ML Kit (Text Recognition V2) chạy Offline trên thiết bị.
*   **Luồng xử lý:**
    1.  AR Camera nhận diện trúng mặt cúp.
    2.  Hệ thống chụp ngầm 1 frame ảnh, gửi qua lõi ML Kit.
    3.  Thuật toán C# bóc tách chuỗi String, lấy dòng chữ dưới chữ "Cử nhân".
    4.  **Cơ chế an toàn (Fail-Safe UI):** Bật Popup thông báo: *"Hệ thống nhận diện bạn là: [Tên OCR]. Đúng chứ?"*. (Cho phép người dùng sửa lại bằng bàn phím nếu AI đọc sai dấu do vân gỗ).
    5.  Bấm "Kích hoạt" -> Popup biến mất -> Bắt đầu trình diễn AR.

### 4.5. Tính năng Mix Hiệu ứng (Customization Drawer)
*   **Mô tả:** Panel trượt từ dưới lên khi bấm nút `[Mix Hiệu Ứng]`.
*   **Các tùy chọn (Phân loại theo Tab):**
    *   *Linh vật:* Rồng Vàng (Đậu lên đỉnh cúp) / Phượng Hoàng Lửa (Lơ lửng vỗ cánh).
    *   *Bệ đỡ/Khởi đầu:* Bục đá trồi lên / Vòng tròn ma thuật mở ra.
    *   *Hiệu ứng VFX:* Tick chọn Pháo giấy (Confetti) / Pháo hoa (Fireworks).
*   **Logic:** Hệ thống lưu trạng thái User lựa chọn và bật/tắt (SetActive) các Prefab 3D tương ứng trước khi chạy Animation Sequence.

### 4.6. Trình diễn Nghệ thuật 3D (Animation Sequence)
Được quản lý bằng Unity Timeline, đồng bộ âm thanh và hình ảnh:
*   **Bí thuật Occlusion:** Khởi tạo một "Cúp tàng hình" (Depth Mask) có kích thước chuẩn xác với cúp thật để che khuất các vật thể 3D bay ra phía sau, tạo cảm giác không gian thực.
*   **Timeline:** Vòng phép mở -> Linh vật bay lượn (theo quỹ đạo Pivot) -> Linh vật đáp/Hover -> Hiệu ứng âm thanh cao trào (Nổ) -> Pháo hoa bung -> Tên sinh viên (TextMeshPro chất liệu vàng kim) nảy ra dạng 3D từ mặt cúp thật.

---

## 5. YÊU CẦU PHI CHỨC NĂNG (NON-FUNCTIONAL REQUIREMENTS)
*   **Hiệu năng (Performance):** Ứng dụng duy trì khung hình tối thiểu 30 FPS trên các thiết bị tầm trung (Ví dụ: Samsung Galaxy A series, iPhone X trở lên). Thời gian xử lý OCR < 1 giây.
*   **Môi trường mạng:** Các chức năng cốt lõi (AR, OCR, Hiệu ứng 3D) phải hoạt động 100% Offline không cần Internet (vì sóng 3G trong ngày hội trường thường rất yếu).
*   **Dung lượng App:** Tối ưu hóa Model 3D (Low-poly) và nén Texture để tổng dung lượng file cài đặt (APK/AAB) dưới 150MB.

---

## 6. KIẾN TRÚC CÔNG NGHỆ (TECH STACK)
*   **Game Engine:** Unity 6 LTS (Hỗ trợ bỏ Splash Screen miễn phí)
*   **AR Framework:** AR Foundation 6.0 + ARCore XR Plugin (Android)
*   **Image Tracking:** ARTrackedImageManager với Reference Image Library
*   **AI/OCR:** Google ML Kit Text Recognition V2 (Native Android Plugin)
*   **Lập trình:** C# (Logic app), ShaderLab (Depth Mask Shader)
*   **UI/UX:** UI Toolkit (UIElements) với USS/UXML, TextMeshPro, DOTween Pro (Animation)
*   **Media Capture:** NatCorder (Screen Recording) hoặc Unity Recorder Package
*   **Asset Pipeline:** Blender (Optimize 3D), Audacity (Audio processing)
*   **Version Control:** Git + GitHub (Backup code)
*   **Testing:** Manual testing trên 100 thiết bị Android thực tế

---

## 7. LỘ TRÌNH PHÁT TRIỂN (RELEASE ROADMAP)

### **PHASE 0: FOUNDATION (Tuần 1-2)**
*Mục tiêu: Chuẩn bị môi trường và asset*
*   Setup Unity 6 + AR Foundation + ARCore
*   Thu thập và chuẩn bị Image Target (ảnh cúp chất lượng cao)
*   Tìm và optimize 3D models miễn phí (Rồng, VFX)
*   Setup Git repository và backup workflow

### **PHASE 1: CORE FEATURES (Tuần 3-5)**
*Mục tiêu: Xây dựng chức năng cốt lõi*
*   **Sprint 1:** AR Image Tracking cơ bản (nhận diện cúp, spawn object)
*   **Sprint 2:** OCR Integration (đọc tên + popup xác nhận)
*   **Sprint 3:** 3D Content Pipeline (import model, animation, optimize)

### **PHASE 2: POLISH & FEATURES (Tuần 6-8)**
*Mục tiêu: Hoàn thiện trải nghiệm người dùng*
*   **Sprint 4:** UI/UX (Splash screen, camera controls, onboarding)
*   **Sprint 5:** Timeline & VFX (animation sequence, particle effects, audio)
*   **Sprint 6:** Capture System (chụp ảnh, quay video, lưu vào Gallery)

### **PHASE 3: QA & OPTIMIZATION (Tuần 9-11)**
*Mục tiêu: Đảm bảo chất lượng sản phẩm*
*   **Sprint 7:** Performance Tuning (optimize FPS, giảm APK size)
*   **Sprint 8:** Beta Testing (20-30 người, thu thập feedback)
*   **Sprint 9:** Mass Testing (100 máy, fix critical bugs)

### **PHASE 4: DEPLOYMENT (Tuần 12)**
*Mục tiêu: Triển khai sản phẩm*
*   Viết tài liệu hướng dẫn sử dụng
*   Build APK final và phân phối
*   Chuẩn bị presentation demo cho Giám đốc RDI
*   Backup plan cho ngày tốt nghiệp

## 8. GIỚI HẠN VÀ RỦI RO (CONSTRAINTS & RISKS)

### **Giới hạn**
*   Ngân sách: $0 (Chỉ sử dụng công cụ và asset miễn phí)
*   Nhân lực: 1 developer
*   Thiết bị test: 100 máy Android (đa dạng cấu hình)
*   Không sử dụng dịch vụ cloud (Firebase, Analytics)

### **Rủi ro kỹ thuật**
*   OCR có thể đọc sai do vân gỗ hoặc ánh sáng kém → Giải pháp: Popup xác nhận + cho phép sửa tay
*   Performance không đạt 30 FPS trên máy yếu → Giải pháp: Optimize model, texture, giảm particle count
*   Image tracking mất dấu khi góc nghiêng → Giải pháp: Hướng dẫn user giữ camera vuông góc

### **Rủi ro vận hành**
*   App crash trong ngày tốt nghiệp → Giải pháp: Test kỹ trên 100 máy, có backup APK version ổn định
*   User không biết cách cài APK → Giải pháp: Video hướng dẫn chi tiết + QR code download

---
*Tài liệu này là cơ sở để tiến hành phát triển, testing và nghiệm thu dự án.*