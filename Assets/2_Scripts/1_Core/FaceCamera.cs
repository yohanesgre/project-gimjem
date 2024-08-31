using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public float distanceFromParent = 0.1f;
    public float yOffset = 0.5f; // New Y offset
    public float childrenScaleFactor = 1f; // Scale factor for children
    private Camera mainCamera;
    private Transform parentTransform;
    private CameraManager cameraManager;

    void Start()
    {
        cameraManager = FindObjectOfType<CameraManager>();
        if (cameraManager != null)
        {
            mainCamera = cameraManager.mainCamera;
            cameraManager.OnCameraSwitch += UpdateMainCamera;
        }
        else
        {
            mainCamera = Camera.main;
        }

        parentTransform = transform.parent;

        // Ensure the Canvas is set to World Space
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }

        AdjustCanvasChildren();
    }

    void OnDestroy()
    {
        if (cameraManager != null)
        {
            cameraManager.OnCameraSwitch -= UpdateMainCamera;
        }
    }

    void UpdateMainCamera(Camera newCamera)
    {
        mainCamera = newCamera;
    }

    void LateUpdate()
    {
        if (mainCamera == null || parentTransform == null) return;

        // Position the Canvas slightly in front of the parent object with Y offset
        transform.position = parentTransform.position +
                             mainCamera.transform.forward * distanceFromParent +
                             Vector3.up * yOffset;

        // Make the Canvas face the camera
        transform.rotation = mainCamera.transform.rotation;

        // Adjust the scale based on the camera's orthographic size
        float scaleFactor = mainCamera.orthographicSize / 5f;
        transform.localScale = Vector3.one * scaleFactor;
    }

    void AdjustCanvasChildren()
    {
        foreach (RectTransform child in transform)
        {
            child.localScale = Vector3.one * childrenScaleFactor;
        }
    }
}