using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

/// <summary>
/// Placement Controller - Đặt/di chuyển/xóa/scale objects trong AR
/// Hỗ trợ đa dạng objects với raycast placement
/// </summary>
public class PlacementController : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private Camera arCamera;

    [Header("Placement Prefabs")]
    [SerializeField] private List<GameObject> placementPrefabs = new List<GameObject>();
    
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float scaleSpeed = 0.01f;

    private List<PlacedObject> placedObjects = new List<PlacedObject>();
    private PlacedObject selectedObject;
    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    private int currentPrefabIndex = 0;
    
    private enum InteractionMode
    {
        Place,
        Move,
        Scale
    }
    private InteractionMode currentMode = InteractionMode.Place;

    void Start()
    {
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
    }

    void Update()
    {
        if (!enabled) return;

        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        // Ignore touches on UI (camera controls, placement buttons)
        if (IsTouchOverUI(touch.position))
        {
            return;
        }

        if (touch.phase == TouchPhase.Began)
        {
            HandleTouchBegan(touch.position);
        }
        else if (touch.phase == TouchPhase.Moved && selectedObject != null)
        {
            if (currentMode == InteractionMode.Move)
            {
                HandleMove(touch.position);
            }
        }

        // Pinch to scale
        if (Input.touchCount == 2 && selectedObject != null)
        {
            HandlePinchScale();
        }
    }

    bool IsTouchOverUI(Vector2 touchPosition)
    {
        // Check if touch is in UI zones
        // Bottom area: Camera controls (height < 200px)
        // Top-left: Prefab selector (x < 100px, y > Screen.height - 300px)
        // Top-right: Action buttons (x > Screen.width - 100px, y > Screen.height - 200px)
        
        float screenHeight = Screen.height;
        float screenWidth = Screen.width;
        
        // Bottom zone (camera controls)
        if (touchPosition.y < 200)
        {
            return true;
        }
        
        // Top-left zone (prefab selector)
        if (touchPosition.x < 100 && touchPosition.y > screenHeight - 300)
        {
            return true;
        }
        
        // Top-right zone (action buttons)
        if (touchPosition.x > screenWidth - 100 && touchPosition.y > screenHeight - 200)
        {
            return true;
        }
        
        return false;
    }

    void HandleTouchBegan(Vector2 screenPosition)
    {
        // Check if touched existing object
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            PlacedObject placedObj = hit.collider.GetComponent<PlacedObject>();
            
            if (placedObj != null)
            {
                SelectObject(placedObj);
                return;
            }
        }

        // Place new object
        if (currentMode == InteractionMode.Place)
        {
            PlaceObject(screenPosition);
        }
    }

    void PlaceObject(Vector2 screenPosition)
    {
        if (placementPrefabs.Count == 0 || currentPrefabIndex >= placementPrefabs.Count)
        {
            Debug.LogWarning("[Placement] No prefabs available!");
            return;
        }

        // Raycast to AR plane
        if (raycastManager.Raycast(screenPosition, raycastHits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = raycastHits[0].pose;
            GameObject prefab = placementPrefabs[currentPrefabIndex];
            
            GameObject obj = Instantiate(prefab, hitPose.position, hitPose.rotation);
            
            // Add PlacedObject component
            PlacedObject placedObj = obj.AddComponent<PlacedObject>();
            placedObj.Initialize(prefab.name);
            
            placedObjects.Add(placedObj);
            
            Debug.Log($"[Placement] Placed {prefab.name} at {hitPose.position}");
        }
    }

    void HandleMove(Vector2 screenPosition)
    {
        if (selectedObject == null) return;

        // Raycast to plane
        if (raycastManager.Raycast(screenPosition, raycastHits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = raycastHits[0].pose;
            selectedObject.transform.position = Vector3.Lerp(
                selectedObject.transform.position,
                hitPose.position,
                moveSpeed * Time.deltaTime
            );
        }
    }

    void HandlePinchScale()
    {
        if (selectedObject == null) return;

        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

        float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
        float currentMagnitude = (touch0.position - touch1.position).magnitude;

        float difference = currentMagnitude - prevMagnitude;
        
        Vector3 newScale = selectedObject.transform.localScale + Vector3.one * difference * scaleSpeed;
        newScale = Vector3.Max(newScale, Vector3.one * 0.1f); // Min scale
        newScale = Vector3.Min(newScale, Vector3.one * 5f);   // Max scale
        
        selectedObject.transform.localScale = newScale;
    }

    void SelectObject(PlacedObject obj)
    {
        // Deselect previous
        if (selectedObject != null)
        {
            selectedObject.SetSelected(false);
        }

        // Select new
        selectedObject = obj;
        selectedObject.SetSelected(true);
        currentMode = InteractionMode.Move;

        Debug.Log($"[Placement] Selected: {obj.name}");
    }

    public void DeselectObject()
    {
        if (selectedObject != null)
        {
            selectedObject.SetSelected(false);
            selectedObject = null;
        }
        currentMode = InteractionMode.Place;
    }

    public void DeleteSelected()
    {
        if (selectedObject != null)
        {
            placedObjects.Remove(selectedObject);
            Destroy(selectedObject.gameObject);
            selectedObject = null;
            currentMode = InteractionMode.Place;

            Debug.Log("[Placement] Deleted selected object");
        }
    }

    public void ClearAll()
    {
        foreach (var obj in placedObjects)
        {
            if (obj != null)
            {
                Destroy(obj.gameObject);
            }
        }

        placedObjects.Clear();
        selectedObject = null;
        currentMode = InteractionMode.Place;

        Debug.Log("[Placement] Cleared all objects");
    }

    public void SetCurrentPrefab(int index)
    {
        if (index >= 0 && index < placementPrefabs.Count)
        {
            currentPrefabIndex = index;
            Debug.Log($"[Placement] Selected prefab: {placementPrefabs[index].name}");
        }
    }

    public int GetPlacedObjectCount()
    {
        return placedObjects.Count;
    }

    void OnEnable()
    {
        // Enable plane detection
        if (planeManager != null)
        {
            planeManager.enabled = true;
        }
    }

    void OnDisable()
    {
        // Disable plane detection
        if (planeManager != null)
        {
            planeManager.enabled = false;
        }
    }
}

/// <summary>
/// Component đánh dấu object đã được place
/// </summary>
public class PlacedObject : MonoBehaviour
{
    private string objectType;
    private Renderer[] renderers;
    private Color[] originalColors;

    public void Initialize(string type)
    {
        objectType = type;
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
    }

    public void SetSelected(bool selected)
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (selected)
            {
                renderers[i].material.color = Color.yellow;
            }
            else
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }
}
