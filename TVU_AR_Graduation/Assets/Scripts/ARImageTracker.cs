using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

/// <summary>
/// AR Image Tracker - Phát hiện image target và spawn prefab
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class ARImageTracker : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject trackedPrefab;

    private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        // AR Foundation 6.0: Subscribe to trackablesChanged event
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    void OnDisable()
    {
        // AR Foundation 6.0: Unsubscribe from trackablesChanged event
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Khi phát hiện ảnh mới → Spawn prefab
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            string imageName = trackedImage.referenceImage.name;
            Debug.Log($"[AR] Image detected: {imageName}");

            if (!spawnedObjects.ContainsKey(imageName))
            {
                GameObject obj = Instantiate(trackedPrefab, trackedImage.transform);
                obj.SetActive(true);
                spawnedObjects[imageName] = obj;
                Debug.Log($"[AR] Spawned object for: {imageName}");
            }
        }

        // Khi ảnh được update → Cập nhật visibility
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            string imageName = trackedImage.referenceImage.name;

            if (spawnedObjects.TryGetValue(imageName, out GameObject obj))
            {
                bool isTracking = trackedImage.trackingState == TrackingState.Tracking;
                obj.SetActive(isTracking);
            }
        }

        // Khi mất tracking hoàn toàn → Ẩn object
        foreach (var kvp in eventArgs.removed)
        {
            ARTrackedImage trackedImage = kvp.Value;
            string imageName = trackedImage.referenceImage.name;
            Debug.Log($"[AR] Image lost: {imageName}");

            if (spawnedObjects.TryGetValue(imageName, out GameObject obj))
            {
                obj.SetActive(false);
            }
        }
    }
}
