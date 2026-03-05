using UnityEngine;
using UnityEngine.UIElements;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    
    private VisualElement overlay;
    private TextField nameInput;
    private Button confirmButton;
    private Button cancelButton;
    
    private string recognizedName;
    
    void Start()
    {
        // Auto-find UIDocument if not assigned
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
        
        // Validate UIDocument
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument component not found on PopupManager GameObject!");
            return;
        }
        
        // Get root visual element
        var root = uiDocument.rootVisualElement;
        
        // Query UI elements with new BEM names
        overlay = root.Q<VisualElement>("confirmation-overlay");
        nameInput = root.Q<TextField>("name-input");
        confirmButton = root.Q<Button>("confirm-button");
        cancelButton = root.Q<Button>("cancel-button");
        
        // Setup button callbacks
        confirmButton.clicked += OnConfirmClicked;
        cancelButton.clicked += OnCancelClicked;
        
        // Hide popup initially
        overlay.style.display = DisplayStyle.None;
        
        // Subscribe to OCR event
        if (OCRManager.Instance != null)
        {
            OCRManager.Instance.OnTextRecognized += ShowPopup;
        }
    }
    
    void ShowPopup(string ocrText)
    {
        // Parse tên từ OCR text
        recognizedName = NameParser.ExtractStudentName(ocrText);
        
        // Hiển thị trong TextField
        nameInput.value = recognizedName;
        
        // Show popup
        overlay.style.display = DisplayStyle.Flex;
    }
    
    void OnConfirmClicked()
    {
        string finalName = nameInput.value;
        Debug.Log($"User confirmed name: {finalName}");
        
        // TODO: Trigger AR animation với tên này
        
        // Hide popup
        overlay.style.display = DisplayStyle.None;
    }
    
    void OnCancelClicked()
    {
        Debug.Log("User cancelled");
        overlay.style.display = DisplayStyle.None;
    }
    
    void OnDestroy()
    {
        // Unsubscribe
        if (OCRManager.Instance != null)
        {
            OCRManager.Instance.OnTextRecognized -= ShowPopup;
        }
        
        // Remove button callbacks
        if (confirmButton != null)
        {
            confirmButton.clicked -= OnConfirmClicked;
        }
        if (cancelButton != null)
        {
            cancelButton.clicked -= OnCancelClicked;
        }
    }
}
