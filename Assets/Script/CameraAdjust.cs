using UnityEngine;

[RequireComponent(typeof(Camera))]
public class UniversalCameraWidth : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float referenceWidth = 10f; // Lebar dunia yang ingin ditampilkan
    [SerializeField] private float minHeight = 5f; // Tinggi minimum (untuk portrait)
    [SerializeField] private float maxHeight = 20f; // Tinggi maksimum (untuk ultra-wide)

    private Camera mainCamera;
    private float targetAspect;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        UpdateCamera();
    }

    void Update()
    {
#if UNITY_EDITOR
        UpdateCamera(); // Di editor, update terus untuk testing
#endif
    }

    void UpdateCamera()
    {
        if (mainCamera.orthographic)
        {
            AdjustOrthographic();
        }
        else
        {
            AdjustPerspective();
        }
    }

    void AdjustOrthographic()
    {
        // Hitung aspect ratio saat ini
        float currentAspect = (float)Screen.width / Screen.height;

        // Hitung ukuran orthographic berdasarkan lebar konstan
        float orthoSize = referenceWidth / currentAspect / 2f;

        // Clamp untuk mencegah zoom terlalu jauh
        orthoSize = Mathf.Clamp(orthoSize, minHeight, maxHeight);

        mainCamera.orthographicSize = orthoSize;
    }

    void AdjustPerspective()
    {
        // Untuk 3D, sesuaikan FOV berdasarkan lebar
        float horizontalFOV = CalculateHorizontalFOV(referenceWidth);
        mainCamera.fieldOfView = horizontalFOV;
    }

    float CalculateHorizontalFOV(float width)
    {
        // Hitung FOV horizontal yang dibutuhkan
        float distanceToTarget = Mathf.Abs(transform.position.z);
        float horizontalFOV = 2f * Mathf.Atan(width / (2f * distanceToTarget)) * Mathf.Rad2Deg;
        return Mathf.Clamp(horizontalFOV, 30f, 120f); // Batasi range FOV
    }

    // Untuk debugging di editor
    void OnDrawGizmos()
    {
        if (!mainCamera) mainCamera = GetComponent<Camera>();

        float visibleWidth = mainCamera.orthographicSize * 2f * mainCamera.aspect;
        float visibleHeight = mainCamera.orthographicSize * 2f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(visibleWidth, visibleHeight, 0));
    }
}