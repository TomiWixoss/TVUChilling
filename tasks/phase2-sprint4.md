# PHASE 2 - SPRINT 4: UI/UX VỚI UI TOOLKIT (Tuần 6)
**Mục tiêu:** Hoàn thiện giao diện người dùng bằng UI Toolkit

**Đọc trước:** `docs/UI_TOOLKIT_GUIDE.md`

---

## BƯỚC 0: SETUP UI TOOLKIT

### Verify UI Toolkit
- [ ] Window > Package Manager
- [ ] Tìm "UI Toolkit" → Status: "Built-in"

### Tạo thư mục cấu trúc
- [ ] `Assets/UI/Styles/`
- [ ] `Assets/UI/Scripts/`
- [ ] `Assets/Resources/Styles/` (quan trọng!)

### Tạo Panel Settings
- [ ] Project > Chuột phải > Create > UI Toolkit > Panel Settings
- [ ] Đặt tên: `DefaultPanelSettings`
- [ ] Lưu vào `Assets/UI/`
- [ ] Inspector:
  - [ ] Scale Mode: **Scale With Screen Size**
  - [ ] Reference Resolution: **1080 x 1920**

---

## BƯỚC 1: CUSTOM SPLASH SCREEN

### Chuẩn bị logo
- [ ] Chuẩn bị logo TVU (PNG, 1024x1024, nền trong suốt)
- [ ] Chuẩn bị logo DRI (PNG, 1024x1024, nền trong suốt)
- [ ] Import vào `Assets/Images/Logos/`

### Cấu hình Splash Screen
- [ ] Edit > Project Settings > Player
- [ ] Tab "Splash Image"
- [ ] Show Unity Logo: **OFF**
- [ ] Logos:
  - [ ] Click "+" để thêm logo
  - [ ] Kéo logo TVU vào
  - [ ] Click "+" thêm logo DRI
- [ ] Background: Màu xanh navy (#001F3F)
- [ ] Animation: **Static** (hoặc Dolly nếu muốn zoom)

### Test
- [ ] Build and Run
- [ ] Verify: Không có logo Unity, chỉ có logo TVU + DRI

---

## BƯỚC 2: TẠO CAMERA UI VỚI UI TOOLKIT

### Tạo UI Document
- [ ] Hierarchy > UI Toolkit > UI Document
- [ ] Đặt tên: `CameraUI`
- [ ] Inspector:
  - [ ] Panel Settings: Kéo `DefaultPanelSettings` vào
  - [ ] Source Asset: Để trống (code bằng C#)

### Tạo USS Stylesheet
- [ ] `Assets/Resources/Styles/` > Chuột phải > Create > UI Toolkit > Style Sheet
- [ ] Đặt tên: `CameraUI.uss`
- [ ] Copy code sau vào file:

```css
/* Root */
.root {
    width: 100%;
    height: 100%;
}

/* Top Bar */
.top-bar {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    height: 120px;
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    padding: 20px;
    background-color: rgba(0, 0, 0, 0.5);
}

.flash-button {
    width: 80px;
    height: 80px;
    font-size: 40px;
    background-color: rgba(255, 255, 255, 0.2);
    border-radius: 40px;
    border-width: 0;
    color: white;
}

.flash-button:hover {
    background-color: rgba(255, 255, 255, 0.3);
}

.flash-button:active {
    background-color: rgba(255, 255, 255, 0.4);
}

.status-label {
    font-size: 32px;
    color: white;
    -unity-text-align: middle-center;
}

/* Shutter Button */
.shutter-button {
    position: absolute;
    bottom: 125px;
    left: 50%;
    translate: -50% 0;
    width: 200px;
    height: 200px;
    background-color: white;
    border-radius: 100px;
    border-width: 8px;
    border-color: rgba(255, 255, 255, 0.5);
}

.shutter-button:active {
    background-color: rgb(255, 50, 50);
}

/* Mode Carousel */
.mode-carousel {
    position: absolute;
    bottom: 25px;
    left: 0;
    right: 0;
    height: 80px;
    flex-direction: row;
    justify-content: center;
    align-items: center;
}

.mode-button {
    width: 250px;
    height: 80px;
    margin: 0 10px;
    font-size: 28px;
    background-color: rgba(255, 255, 255, 0.2);
    border-radius: 40px;
    border-width: 2px;
    border-color: white;
    color: white;
}

.mode-button:hover {
    background-color: rgba(255, 255, 255, 0.3);
}

.mode-button.active {
    background-color: rgba(0, 200, 255, 0.8);
    border-color: rgb(0, 200, 255);
}
```
- [ ] Save file

---

## BƯỚC 3: VIẾT SCRIPT CAMERA UI CONTROLLER

### Tạo script CameraUIController.cs
- [ ] `Assets/UI/Scripts/` > Create > C# Script
- [ ] Đặt tên: `CameraUIController`
- [ ] Copy code sau:

```csharp
using UnityEngine;
using UnityEngine.UIElements;

public class CameraUIController : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    
    // UI Elements
    private Button flashButton;
    private Label statusLabel;
    private Button shutterButton;
    private VisualElement modeCarousel;
    
    // Mode buttons
    private Button cupButton;
    private Button cardButton;
    private Button mixButton;
    private Button currentActiveButton;
    
    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        
        // Load stylesheet
        var styleSheet = Resources.Load<StyleSheet>("Styles/CameraUI");
        if (styleSheet != null)
        {
            root.styleSheets.Add(styleSheet);
        }
        else
        {
            Debug.LogError("Cannot load CameraUI.uss from Resources/Styles/");
        }
        
        BuildUI();
        RegisterCallbacks();
    }
    
    void BuildUI()
    {
        // Clear root
        root.Clear();
        root.AddToClassList("root");
        
        // Top Bar
        var topBar = new VisualElement();
        topBar.AddToClassList("top-bar");
        
        flashButton = new Button { text = "💡" };
        flashButton.AddToClassList("flash-button");
        
        statusLabel = new Label("Đang tìm kiếm...");
        statusLabel.AddToClassList("status-label");
        
        topBar.Add(flashButton);
        topBar.Add(statusLabel);
        
        // Shutter Button
        shutterButton = new Button();
        shutterButton.AddToClassList("shutter-button");
        
        // Mode Carousel
        modeCarousel = new VisualElement();
        modeCarousel.AddToClassList("mode-carousel");
        
        cupButton = new Button { text = "Cúp Dọc" };
        cupButton.AddToClassList("mode-button");
        
        cardButton = new Button { text = "Bìa Ngang" };
        cardButton.AddToClassList("mode-button");
        
        mixButton = new Button { text = "Mix Hiệu Ứng" };
        mixButton.AddToClassList("mode-button");
        
        modeCarousel.Add(cupButton);
        modeCarousel.Add(cardButton);
        modeCarousel.Add(mixButton);
        
        // Add to root
        root.Add(topBar);
        root.Add(shutterButton);
        root.Add(modeCarousel);
        
        // Set default active mode
        SelectMode(cupButton);
    }
    
    void RegisterCallbacks()
    {
        flashButton.clicked += OnFlashClicked;
        
        shutterButton.RegisterCallback<PointerDownEvent>(OnShutterDown);
        shutterButton.RegisterCallback<PointerUpEvent>(OnShutterUp);
        
        cupButton.clicked += () => SelectMode(cupButton);
        cardButton.clicked += () => SelectMode(cardButton);
        mixButton.clicked += () => SelectMode(mixButton);
    }
    
    void OnFlashClicked()
    {
        Debug.Log("Flash toggled");
        // TODO: Implement flash toggle
    }
    
    void OnShutterDown(PointerDownEvent evt)
    {
        Debug.Log("Shutter pressed");
        // TODO: Start recording or prepare photo
    }
    
    void OnShutterUp(PointerUpEvent evt)
    {
        Debug.Log("Shutter released");
        // TODO: Take photo or stop recording
    }
    
    void SelectMode(Button button)
    {
        // Remove active class from previous button
        if (currentActiveButton != null)
        {
            currentActiveButton.RemoveFromClassList("active");
        }
        
        // Add active class to new button
        button.AddToClassList("active");
        currentActiveButton = button;
        
        Debug.Log($"Mode selected: {button.text}");
        // TODO: Enable/disable AR image targets
    }
    
    public void UpdateStatus(string status)
    {
        if (statusLabel != null)
        {
            statusLabel.text = status;
        }
    }
}
```
- [ ] Save file

### Gắn script vào UI Document
- [ ] Chọn `CameraUI` GameObject trong Hierarchy
- [ ] Add Component: `CameraUIController`

---

## BƯỚC 4: TEST UI TOOLKIT

### Test trong Editor
- [ ] Play mode
- [ ] Verify:
  - [ ] Top bar hiển thị
  - [ ] Flash button có icon 💡
  - [ ] Status label hiển thị "Đang tìm kiếm..."
  - [ ] Shutter button ở giữa dưới
  - [ ] 3 mode buttons hiển thị
  - [ ] Click mode button → Highlight màu xanh

### Test Hot Reload
- [ ] Giữ Play mode
- [ ] Mở `CameraUI.uss` trong VS Code
- [ ] Đổi màu flash button: `background-color: rgba(255, 0, 0, 0.5);`
- [ ] Save file
- [ ] Verify: Button đổi màu ngay lập tức (không cần stop Play)

### Debug UI
- [ ] Window > UI Toolkit > Debugger
- [ ] Chọn CameraUI trong scene
- [ ] Xem hierarchy và styles

---

## BƯỚC 5: TÍCH HỢP VỚI AR TRACKER

### Update ARImageTracker.cs
```csharp
// Thêm reference
[SerializeField] private CameraUIController cameraUI;

void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
{
    foreach (var trackedImage in eventArgs.added)
    {
        cameraUI.UpdateStatus("Đã tìm thấy!");
        // ... existing code
    }
    
    foreach (var trackedImage in eventArgs.removed)
    {
        cameraUI.UpdateStatus("Đang tìm kiếm...");
    }
}
```
- [ ] Update code
- [ ] Gán CameraUI reference trong Inspector

---

## BƯỚC 6: ONBOARDING TUTORIAL VỚI UI TOOLKIT

### Tạo Scene mới
- [ ] File > New Scene
- [ ] Đặt tên: `OnboardingScene`
- [ ] Save vào `Assets/Scenes/`

### Tạo UI Document cho Onboarding
- [ ] Hierarchy > UI Toolkit > UI Document
- [ ] Đặt tên: `OnboardingUI`
- [ ] Panel Settings: Kéo `DefaultPanelSettings` vào

### Tạo USS cho Onboarding
- [ ] `Assets/Resources/Styles/Onboarding.uss`
- [ ] Copy code:

```css
.slide {
    width: 100%;
    height: 100%;
    justify-content: center;
    align-items: center;
    background-color: rgb(0, 31, 63);
}

.slide-title {
    font-size: 48px;
    color: white;
    margin-bottom: 40px;
    -unity-text-align: middle-center;
}

.slide-description {
    font-size: 32px;
    color: rgba(255, 255, 255, 0.8);
    margin: 20px 60px;
    -unity-text-align: middle-center;
}

.button-container {
    position: absolute;
    bottom: 80px;
    left: 0;
    right: 0;
    flex-direction: row;
    justify-content: space-around;
    padding: 0 60px;
}

.nav-button {
    width: 300px;
    height: 100px;
    font-size: 36px;
    background-color: rgba(0, 200, 255, 0.8);
    border-radius: 50px;
    border-width: 0;
    color: white;
}

.nav-button:hover {
    background-color: rgba(0, 200, 255, 1);
}

.skip-button {
    background-color: rgba(255, 255, 255, 0.2);
}
```

### Script OnboardingManager.cs
```csharp
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class OnboardingManager : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    
    private VisualElement[] slides;
    private int currentSlideIndex = 0;
    
    private Button nextButton;
    private Button skipButton;
    
    void OnEnable()
    {
        // Check if first time
        if (PlayerPrefs.GetInt("OnboardingComplete", 0) == 1)
        {
            LoadMainScene();
            return;
        }
        
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        
        // Load stylesheet
        var styleSheet = Resources.Load<StyleSheet>("Styles/Onboarding");
        root.styleSheets.Add(styleSheet);
        
        BuildUI();
        ShowSlide(0);
    }
    
    void BuildUI()
    {
        root.Clear();
        
        // Create slides
        slides = new VisualElement[3];
        
        // Slide 1
        slides[0] = CreateSlide(
            "Chào mừng đến AR Graduation",
            "Trải nghiệm lễ tốt nghiệp với công nghệ AR"
        );
        
        // Slide 2
        slides[1] = CreateSlide(
            "Hướng camera vào cúp",
            "Giữ khoảng cách 30-50cm để nhận diện tốt nhất"
        );
        
        // Slide 3
        slides[2] = CreateSlide(
            "Chụp ảnh và quay video",
            "Chạm để chụp ảnh\nGiữ để quay video"
        );
        
        // Add all slides to root
        foreach (var slide in slides)
        {
            root.Add(slide);
        }
        
        // Button container
        var buttonContainer = new VisualElement();
        buttonContainer.AddToClassList("button-container");
        
        skipButton = new Button { text = "Bỏ qua" };
        skipButton.AddToClassList("nav-button");
        skipButton.AddToClassList("skip-button");
        skipButton.clicked += Skip;
        
        nextButton = new Button { text = "Tiếp tục" };
        nextButton.AddToClassList("nav-button");
        nextButton.clicked += NextSlide;
        
        buttonContainer.Add(skipButton);
        buttonContainer.Add(nextButton);
        
        root.Add(buttonContainer);
    }
    
    VisualElement CreateSlide(string title, string description)
    {
        var slide = new VisualElement();
        slide.AddToClassList("slide");
        slide.style.display = DisplayStyle.None; // Hide by default
        
        var titleLabel = new Label(title);
        titleLabel.AddToClassList("slide-title");
        
        var descLabel = new Label(description);
        descLabel.AddToClassList("slide-description");
        
        slide.Add(titleLabel);
        slide.Add(descLabel);
        
        return slide;
    }
    
    void ShowSlide(int index)
    {
        // Hide all slides
        for (int i = 0; i < slides.Length; i++)
        {
            slides[i].style.display = DisplayStyle.None;
        }
        
        // Show current slide
        slides[index].style.display = DisplayStyle.Flex;
        currentSlideIndex = index;
        
        // Update button text
        if (index == slides.Length - 1)
        {
            nextButton.text = "Bắt đầu";
        }
        else
        {
            nextButton.text = "Tiếp tục";
        }
    }
    
    void NextSlide()
    {
        currentSlideIndex++;
        if (currentSlideIndex >= slides.Length)
        {
            Complete();
        }
        else
        {
            ShowSlide(currentSlideIndex);
        }
    }
    
    void Skip()
    {
        Complete();
    }
    
    void Complete()
    {
        PlayerPrefs.SetInt("OnboardingComplete", 1);
        PlayerPrefs.Save();
        LoadMainScene();
    }
    
    void LoadMainScene()
    {
        SceneManager.LoadScene("ARScene");
    }
}
```
- [ ] Tạo script
- [ ] Gắn vào OnboardingUI GameObject
- [ ] Gán references

### Thêm vào Build Settings
- [ ] File > Build Settings
- [ ] Add OnboardingScene ở index 0
- [ ] ARScene ở index 1

---

## BƯỚC 7: TEST TOÀN BỘ UI

### Test Onboarding
- [ ] Play OnboardingScene
- [ ] Verify: 3 slides hiển thị đúng
- [ ] Click "Tiếp tục" → Chuyển slide
- [ ] Slide cuối: Button đổi thành "Bắt đầu"
- [ ] Click "Bắt đầu" → Chuyển ARScene

### Test Camera UI
- [ ] Play ARScene
- [ ] Verify: UI hiển thị đúng
- [ ] Click Flash button → Log "Flash toggled"
- [ ] Click mode buttons → Highlight active
- [ ] Click Shutter → Log "Shutter pressed/released"

### Test Hot Reload
- [ ] Play mode
- [ ] Sửa USS file
- [ ] Save → UI update ngay lập tức

---

## CHECKLIST SPRINT 4

- [ ] UI Toolkit đã setup
- [ ] Panel Settings đã tạo
- [ ] CameraUI.uss với styles hoàn chỉnh
- [ ] CameraUIController hoạt động
- [ ] Top Bar (Flash button, Status label)
- [ ] Shutter Button (ở giữa dưới)
- [ ] Mode Carousel (3 buttons, highlight active)
- [ ] Onboarding Scene với 3 slides
- [ ] Onboarding chỉ hiện lần đầu
- [ ] Hot Reload hoạt động
- [ ] UI responsive trên nhiều màn hình
- [ ] Splash Screen custom (logo TVU + DRI)
- [ ] Test thành công trên thiết bị thật

### Commit code
```bash
git add .
git commit -m "Sprint 4 complete: UI Toolkit with camera controls and onboarding"
git push origin dev
```

**✅ Sprint 4 hoàn thành → Chuyển sang `phase2-sprint5.md`**

