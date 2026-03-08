using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

/// <summary>
/// AR Image Tracker - Phát hiện 2 image targets và spawn prefabs tương ứng
/// Cup_TVU → Phoenix prefab
/// Diploma_TVU → Dancer prefab
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class ARImageTracker : MonoBehaviour
{
    [Header("Prefab Mappings")]
    [SerializeField] private GameObject phoenixPrefab;
    [SerializeField] private GameObject dancerPrefab;

    private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Khi phát hiện ảnh mới → Spawn prefab tương ứng
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            string imageName = trackedImage.referenceImage.name;
            Debug.Log($"[AR] Image detected: {imageName}");

            if (!spawnedObjects.ContainsKey(imageName))
            {
                GameObject prefabToSpawn = GetPrefabForImage(imageName);
                
                if (prefabToSpawn != null)
                {
                    GameObject obj = Instantiate(prefabToSpawn, trackedImage.transform);
                    obj.SetActive(true);
                    spawnedObjects[imageName] = obj;
                    Debug.Log($"[AR] Spawned {prefabToSpawn.name} for: {imageName}");
                }
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

    GameObject GetPrefabForImage(string imageName)
    {
        return imageName switch
        {
            "Cup_TVU" => phoenixPrefab,
            "Diploma_TVU" => dancerPrefab,
            _ => null
        };
    }
}
