# HƯỚNG DẪN UI TOOLKIT CHO TVU AR GRADUATION

## TỔNG QUAN

UI Toolkit là hệ thống UI mới của Unity, thay thế Unity UI (UGUI) cũ. Nó cho phép:
- Code UI bằng C# declarative
- Style bằng USS (giống CSS)
- Layout bằng UXML (giống HTML)
- Hot reload: Thay đổi USS/UXML không cần recompile

---

## SO SÁNH VỚI UNITY UI CŨ

### Unity UI (UGUI) - Cũ
```csharp
// Phải kéo thả trong Editor
[SerializeField] private Button myButton;
[SerializeField] private Text myText;

void Start() {
    myButton.onClick.AddListener(OnClick);
    myText.text = "Hello";
}
```

### UI Toolkit - Mới
```csharp
// Code hoàn toàn trong C#
void OnEnable() {
    var root = GetComponent<UIDocument>().rootVisualElement;
    
    var button = new Button { text = "Hello" };
    button.clicked += OnClick;
    root.Add(button);
}
```

---

## CÀI ĐẶT

### Bước 1: Verify UI Toolkit có sẵn
- Unity 6 đã có built-in
- Window > Package Manager
- Tìm "UI Toolkit" → Phải có status "Built-in"

### Bước 2: Tạo UI Document
1. Hierarchy > UI Toolkit > UI Document
2. Đặt tên: `CameraUI`
3. Inspector:
   - Panel Settings: Tạo mới (chuột phải > Create > UI Toolkit > Panel Settings)
   - Source Asset: Để trống (sẽ code bằng C#)

---

## CẤU TRÚC FILE

```
Assets/
├── UI/
│   ├── Styles/
│   │   ├── CameraUI.uss       (CSS-like styling)
│   │   └── Common.uss          (Shared styles)
│   ├── Layouts/
│   │   └── CameraUI.uxml       (HTML-like layout - optional)
│   └── Scripts/
│       └── CameraUIController.cs
```

---

## VÍ DỤ: CAMERA UI

### 1. Tạo UI bằng C# (CameraUIController.cs)

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
    
    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        
        // Load stylesheet
        var styleSheet = Resources.Load<StyleSheet>("Styles/CameraUI");
        root.styleSheets.Add(styleSheet);
        
        BuildUI();
        RegisterCallbacks();
    }
    
    void BuildUI()
    {
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
        
        var cupButton = new Button { text = "Cúp Dọc" };
        cupButton.AddToClassList("mode-button");
        
        var cardButton = new Button { text = "Bìa Ngang" };
        cardButton.AddToClassList("mode-button");
        
        var mixButton = new Button { text = "Mix Hiệu Ứng" };
        mixButton.AddToClassList("mode-button");
        
        modeCarousel.Add(cupButton);
        modeCarousel.Add(cardButton);
        modeCarousel.Add(mixButton);
        
        // Add to root
        root.Add(topBar);
        root.Add(shutterButton);
        root.Add(modeCarousel);
    }
    
    void RegisterCallbacks()
    {
        flashButton.clicked += OnFlashClicked;
        shutterButton.RegisterCallback<PointerDownEvent>(OnShutterDown);
        shutterButton.RegisterCallback<PointerUpEvent>(OnShutterUp);
    }
    
    void OnFlashClicked()
    {
        Debug.Log("Flash toggled");
    }
    
    void OnShutterDown(PointerDownEvent evt)
    {
        Debug.Log("Shutter pressed");
    }
    
    void OnShutterUp(PointerUpEvent evt)
    {
        Debug.Log("Shutter released");
    }
}
```

---

### 2. Style bằng USS (CameraUI.uss)

```css
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

---

### 3. Lưu USS vào Resources

```
Assets/
└── Resources/
    └── Styles/
        └── CameraUI.uss
```

**Quan trọng:** USS phải nằm trong thư mục `Resources/` để load được bằng `Resources.Load<StyleSheet>()`

---

## HOT RELOAD

### Cách bật Hot Reload
1. Edit > Preferences > UI Toolkit
2. Tick "Enable Live Reload"

### Cách dùng
1. Play mode trong Unity
2. Mở file USS trong VS Code
3. Sửa CSS (ví dụ: đổi màu button)
4. Save file
5. Unity tự động reload → Thấy thay đổi ngay lập tức

**Lưu ý:** Chỉ USS/UXML hot reload, C# vẫn phải recompile.

---

## RESPONSIVE DESIGN

### Dùng % thay vì px

```css
.my-button {
    width: 80%;  /* 80% chiều rộng màn hình */
    height: 10%; /* 10% chiều cao màn hình */
}
```

### Dùng Flexbox

```css
.container {
    flex-direction: row;      /* Ngang */
    justify-content: center;  /* Căn giữa ngang */
    align-items: center;      /* Căn giữa dọc */
}
```

### Media Queries (Unity 2023+)

```css
@media (max-width: 800px) {
    .my-button {
        width: 100%;
    }
}
```

---

## DEBUG UI TOOLKIT

### UI Debugger
1. Window > UI Toolkit > Debugger
2. Chọn UI Document trong scene
3. Xem hierarchy và styles realtime

### Inspector
- Hover vào element → Highlight trong scene
- Click element → Xem computed styles

---

## BEST PRACTICES

### 1. Tách Style ra file USS
❌ **Tệ:**
```csharp
button.style.width = 200;
button.style.height = 80;
button.style.backgroundColor = Color.blue;
```

✅ **Tốt:**
```csharp
button.AddToClassList("my-button");
```
```css
.my-button {
    width: 200px;
    height: 80px;
    background-color: blue;
}
```

### 2. Dùng USS Classes thay vì inline styles
- Dễ maintain
- Hot reload
- Reusable

### 3. Tổ chức file
- 1 file USS cho mỗi screen
- 1 file Common.uss cho shared styles
- Load stylesheet trong OnEnable()

### 4. Performance
- Tránh tạo/xóa elements liên tục
- Dùng `SetEnabled()` thay vì `RemoveFromHierarchy()`
- Cache references

---

## MIGRATION TỪ UNITY UI

### Canvas → UIDocument
```csharp
// Old
Canvas canvas = GetComponent<Canvas>();

// New
UIDocument uiDocument = GetComponent<UIDocument>();
VisualElement root = uiDocument.rootVisualElement;
```

### Button
```csharp
// Old
Button button = GetComponent<Button>();
button.onClick.AddListener(OnClick);

// New
Button button = new Button();
button.clicked += OnClick;
```

### Text → Label
```csharp
// Old
Text text = GetComponent<Text>();
text.text = "Hello";

// New
Label label = new Label("Hello");
```

### Image → VisualElement with background
```csharp
// Old
Image image = GetComponent<Image>();
image.sprite = mySprite;

// New
VisualElement image = new VisualElement();
image.style.backgroundImage = new StyleBackground(mySprite);
```

---

## TÀI LIỆU THAM KHẢO

- Unity Manual: https://docs.unity3d.com/Manual/UIElements.html
- USS Properties: https://docs.unity3d.com/Manual/UIE-USS-Properties-Reference.html
- C# API: https://docs.unity3d.com/ScriptReference/UIElements.VisualElement.html

---

**Bắt đầu với UI Toolkit ngay trong Sprint 4!**
