using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

using System.Collections.Generic;

/// <summary>
/// AR Placement Manager - Wrapper cho ObjectSpawner theo pattern ARTemplate
/// Kết hợp ObjectSpawner (XR Interaction Toolkit) với WebView UI
/// </summary>
public class ARPlacementManager : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private Camera arCamera;

    [Header("XR Interaction Toolkit")]
    [SerializeField] private ObjectSpawner objectSpawner;
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRInteractionGroup interactionGroup;

    [Header("Settings")]
    [SerializeField] private bool autoEnableOnStart = true;

    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    private GameObject selectedObject;

    void Start()
    {
        // Auto-find components
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }

        if (raycastManager == null)
        {
            raycastManager = FindFirstObjectByType<ARRaycastManager>();
        }

        if (planeManager == null)
        {
            planeManager = FindFirstObjectByType<ARPlaneManager>();
        }

        if (objectSpawner == null)
        {
            objectSpawner = GetComponent<ObjectSpawner>();
            if (objectSpawner == null)
            {
                Debug.LogError("[ARPlacementManager] ObjectSpawner not found! Please add ObjectSpawner component.");
            }
        }

        if (interactionGroup == null)
        {
            interactionGroup = FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRInteractionGroup>();
        }

        // Subscribe to ObjectSpawner events
        if (objectSpawner != null)
        {
            objectSpawner.objectSpawned += OnObjectSpawned;
        }

        if (autoEnableOnStart)
        {
            EnablePlacement();
        }

        Debug.Log("[ARPlacementManager] Initialized with ObjectSpawner pattern");
    }

    void Update()
    {
        if (!enabled) return;

        // Handle touch input for placement
        HandleTouchInput();

        // Track selected object from XRInteractionGroup
        UpdateSelectedObject();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        // Only handle tap (not drag)
        if (touch.phase != TouchPhase.Began) return;

        // Ignore touches on UI
        if (IsTouchOverUI(touch.position)) return;

        // Try to place object
        TryPlaceObject(touch.position);
    }

    bool IsTouchOverUI(Vector2 touchPosition)
    {
        float screenHeight = Screen.height;

        // Top bar (80px)
        if (touchPosition.y > screenHeight - 80)
        {
            return true;
        }

        // Bottom zone - ModelSelector/CameraControls (200px)
        if (touchPosition.y < 200)
        {
            return true;
        }

        return false;
    }

    void TryPlaceObject(Vector2 screenPosition)
    {
        if (objectSpawner == null) return;

        // Raycast to AR plane
        if (raycastManager.Raycast(screenPosition, raycastHits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = raycastHits[0].pose;

            // Use ObjectSpawner to spawn (handles rotation, facing camera, etc.)
            objectSpawner.TrySpawnObject(hitPose.position, hitPose.up);
        }
    }

    void OnObjectSpawned(GameObject spawnedObject)
    {
        Debug.Log($"[ARPlacementManager] Object spawned: {spawnedObject.name}");

        // Ensure object has XRGrabInteractable for selection/manipulation
        if (spawnedObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() == null)
        {
            var grabInteractable = spawnedObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            grabInteractable.throwOnDetach = false;
        }

        // Ensure object has collider
        if (spawnedObject.GetComponent<Collider>() == null)
        {
            spawnedObject.AddComponent<BoxCollider>();
        }
    }

    void UpdateSelectedObject()
    {
        if (interactionGroup == null) return;

        var currentFocused = interactionGroup.focusInteractable;

        if (currentFocused != null && currentFocused.transform.gameObject != selectedObject)
        {
            // New object selected
            selectedObject = currentFocused.transform.gameObject;
            NotifyObjectSelected(selectedObject);
        }
        else if (currentFocused == null && selectedObject != null)
        {
            // Object deselected
            NotifyObjectDeselected();
            selectedObject = null;
        }
    }

    void NotifyObjectSelected(GameObject obj)
    {
        Debug.Log($"[ARPlacementManager] Object selected: {obj.name}");

        var webViewManager = FindFirstObjectByType<WebViewManager>();
        if (webViewManager != null)
        {
            webViewManager.SendMessageToWebView("onObjectSelected", obj.name);
        }
    }

    void NotifyObjectDeselected()
    {
        Debug.Log("[ARPlacementManager] Object deselected");

        var webViewManager = FindFirstObjectByType<WebViewManager>();
        if (webViewManager != null)
        {
            webViewManager.SendMessageToWebView("onObjectDeselected", "");
        }
    }

    // Public API for WebView

    public void SetSpawnObjectIndex(int index)
    {
        if (objectSpawner != null)
        {
            objectSpawner.SetSpawnObjectIndex(index);
            Debug.Log($"[ARPlacementManager] Set spawn object index: {index}");
        }
    }

    public void DeleteSelected()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject);
            selectedObject = null;
            NotifyObjectDeselected();
            Debug.Log("[ARPlacementManager] Deleted selected object");
        }
    }

    public void DuplicateSelected()
    {
        if (selectedObject != null && objectSpawner != null)
        {
            Vector3 offset = selectedObject.transform.forward * 0.5f;
            GameObject duplicate = Instantiate(
                selectedObject,
                selectedObject.transform.position + offset,
                selectedObject.transform.rotation
            );

            Debug.Log("[ARPlacementManager] Duplicated selected object");
        }
    }

    public void ClearAll()
    {
        if (objectSpawner == null) return;

        // Delete all children of ObjectSpawner
        foreach (Transform child in objectSpawner.transform)
        {
            Destroy(child.gameObject);
        }

        selectedObject = null;
        NotifyObjectDeselected();

        Debug.Log("[ARPlacementManager] Cleared all objects");
    }

    public void EnablePlacement()
    {
        enabled = true;

        if (planeManager != null)
        {
            planeManager.enabled = true;
        }

        Debug.Log("[ARPlacementManager] Placement enabled");
    }

    public void DisablePlacement()
    {
        enabled = false;

        if (planeManager != null)
        {
            planeManager.enabled = false;
        }

        Debug.Log("[ARPlacementManager] Placement disabled");
    }

    void OnDestroy()
    {
        if (objectSpawner != null)
        {
            objectSpawner.objectSpawned -= OnObjectSpawned;
        }
    }
}
