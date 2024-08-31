using System;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public event Action<Camera> OnCameraSwitch;
    public Camera mainCamera;
    public Camera[] isometricCameras;
    public Transform target; // The character to follow

    [Header("Main Camera Settings")]
    public float rotationSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 15f;
    public float zoomSpeed = 2f;
    public float minDistanceForZoom = 10f; // Minimum distance to start zooming
    public float maxDistanceForZoom = 30f; // Distance at which max zoom is applied

    private int currentIsometricCamera = 0;
    private Vector3 mainCameraInitialPosition;

    public float distanceToTarget;
    public float cameraSize;

    private void Start()
    {
        if (isometricCameras != null && isometricCameras.Length > 0)
        {
            // Set the main camera to the first camera in the array
            mainCamera = isometricCameras[0];

            // Disable all cameras except the main camera
            for (int i = 0; i < isometricCameras.Length; i++)
            {
                isometricCameras[i].gameObject.SetActive(i == 0);
            }
        }
        else if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        OnCameraSwitch?.Invoke(mainCamera);

        mainCameraInitialPosition = mainCamera.transform.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchToNextIsometricCamera();
        }
    }

    private void LateUpdate()
    {
        // Main camera behavior
        if (mainCamera.gameObject.activeSelf)
        {
            // Keep the camera at its initial position
            mainCamera.transform.position = mainCameraInitialPosition;

            // Calculate the direction to the target
            Vector3 directionToTarget = (target.position - mainCamera.transform.position).normalized;

            // Calculate the rotation to look at the target
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // Smoothly rotate the camera
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Calculate distance to target
            distanceToTarget = Vector3.Distance(mainCamera.transform.position, target.position);

            // Calculate zoom based on distance
            float targetZoom = Mathf.Lerp(minZoom, maxZoom,
                Mathf.InverseLerp(minDistanceForZoom, maxDistanceForZoom, distanceToTarget));

            // Smoothly apply zoom
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime);

            cameraSize = mainCamera.orthographicSize;
        }
    }

    public void SwitchToNextIsometricCamera()
    {
        // Disable all cameras
        foreach (Camera cam in isometricCameras)
        {
            cam.gameObject.SetActive(false);
        }

        // Enable the next isometric camera
        currentIsometricCamera = (currentIsometricCamera + 1) % isometricCameras.Length;
        mainCamera = isometricCameras[currentIsometricCamera];
        mainCamera.gameObject.SetActive(true);

        // Update the main camera's initial position
        mainCameraInitialPosition = mainCamera.transform.position;

        OnCameraSwitch?.Invoke(mainCamera);
    }
}