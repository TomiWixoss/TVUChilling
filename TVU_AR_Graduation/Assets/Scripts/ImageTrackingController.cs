using UnityEngine;

/// <summary>
/// Image Tracking Controller - Điều khiển phoenix bay circular path
/// Bay từ cúp lên trời, lượn vòng, rồi bay về
/// </summary>
public class ImageTrackingController : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] private float flightRadius = 2f;
    [SerializeField] private float flightHeight = 3f;
    [SerializeField] private float flightSpeed = 1f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Transform imageTarget; // Reference point (cúp/bằng)
    private float currentAngle = 0f;
    private Vector3 centerPoint;
    private bool isFlying = false;

    void Start()
    {
        // Get parent transform (AR tracked image)
        imageTarget = transform.parent;
        
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void OnEnable()
    {
        isFlying = true;
        
        // Play flying animation
        if (animator != null)
        {
            animator.SetBool("IsFlying", true);
        }
    }

    void OnDisable()
    {
        isFlying = false;
        
        // Stop animation
        if (animator != null)
        {
            animator.SetBool("IsFlying", false);
        }
    }

    void Update()
    {
        if (!isFlying || imageTarget == null) return;

        // Update center point (image position + height offset)
        centerPoint = imageTarget.position + Vector3.up * flightHeight;

        // Circular flight path
        currentAngle += flightSpeed * Time.deltaTime;
        
        float x = Mathf.Cos(currentAngle) * flightRadius;
        float z = Mathf.Sin(currentAngle) * flightRadius;
        
        Vector3 targetPosition = centerPoint + new Vector3(x, 0, z);
        
        // Move phoenix
        transform.position = targetPosition;

        // Rotate to face movement direction
        Vector3 direction = new Vector3(-Mathf.Sin(currentAngle), 0, Mathf.Cos(currentAngle));
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
