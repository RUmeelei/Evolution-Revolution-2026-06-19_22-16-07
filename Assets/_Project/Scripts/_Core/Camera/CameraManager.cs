using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera cam;

    [Header("Main")]
    [SerializeField] private float smoothing = 1f;
    [SerializeField] private float keyboardScrollSpeed = 5f;
    [SerializeField] private float zoomSpeed = 500f;
    
    [SerializeField] private float minHeight = 1f;
    [SerializeField] private float maxHeight = 50f;

    private Vector3 targetPosition;
    private Vector3 lastMousePosition;

    private float targetZoom;

    private bool isDragging = false;

    void Start()
    {
        targetPosition = transform.position;
        targetZoom = 5;
    }
    
    void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleSmoothing();
    }

    private void HandleMovement()
    {
        float speedMultiplier = keyboardScrollSpeed * (cam.orthographicSize / minHeight);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speedMultiplier = speedMultiplier * 2;
        }

        if (Input.GetKey(KeyCode.W) && !isDragging) targetPosition += Vector3.up * speedMultiplier * Time.deltaTime;
        if (Input.GetKey(KeyCode.S) && !isDragging) targetPosition += Vector3.down * speedMultiplier * Time.deltaTime;
        if (Input.GetKey(KeyCode.A) && !isDragging) targetPosition += Vector3.left * speedMultiplier * Time.deltaTime;
        if (Input.GetKey(KeyCode.D) && !isDragging) targetPosition += Vector3.right * speedMultiplier * Time.deltaTime;

        if (Input.GetMouseButtonDown(2)) {
            lastMousePosition = Input.mousePosition;
            isDragging = true;
        }
        if (Input.GetMouseButtonUp(2)) {
            isDragging = false;
        }

        if (isDragging) {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;
            
            float worldUnitsPerPixel = cam.orthographicSize * 2f / cam.pixelHeight;
            Vector3 worldDelta = new Vector3(mouseDelta.x, mouseDelta.y, 0f) * worldUnitsPerPixel;
            
            targetPosition -= worldDelta;
        }
    }

    private void HandleZoom()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            float newSize = targetZoom - Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;

            targetZoom = Mathf.Clamp(newSize, minHeight, maxHeight);
        }
    }

    private void HandleSmoothing()
    {
        targetPosition.z = -1f;

        if (isDragging) 
        {
            cam.transform.position = targetPosition;
        } 
        else
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPosition, 10f * Time.deltaTime / smoothing);
        }

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, 10f * Time.deltaTime / smoothing);
    }
}