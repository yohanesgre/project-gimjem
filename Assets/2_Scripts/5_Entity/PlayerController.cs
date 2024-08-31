using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 movement;
    private Rigidbody rb;
    private CameraManager cameraManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraManager = FindObjectOfType<CameraManager>();
        if (cameraManager == null)
        {
            Debug.LogError("CameraManager not found in the scene!");
        }
    }

    private void Update()
    {
        // Get input
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        // Get the current active camera
        Camera currentCamera = cameraManager.mainCamera;

        // Calculate isometric movement vectors
        Vector3 forward = Vector3.ProjectOnPlane(currentCamera.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(currentCamera.transform.right, Vector3.up).normalized;

        // Calculate movement direction
        movement = (right * moveHorizontal + forward * moveVertical).normalized;
    }

    private void FixedUpdate()
    {
        // Move the player
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);

        // Optionally, face the movement direction
        if (movement != Vector3.zero)
        {
            transform.forward = movement;
        }
    }
}