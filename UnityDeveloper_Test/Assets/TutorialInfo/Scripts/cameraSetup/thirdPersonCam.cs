using UnityEngine;

/// <summary>
/// Third-person camera that orbits the player and adjusts its up-vector
/// to match the current gravity direction.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Settings")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float heightOffset = 2f;
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minPitch = -20f;
    [SerializeField] private float maxPitch = 60f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float upVectorSpeed = 6f;

    private float _yaw;
    private float _pitch = 20f;
    private Vector3 _smoothedUp = Vector3.up;

    private GravityManipulator _grav;

    // Exposed so PlayerMovement can read camera facing direction
    public Vector3 SmoothedUp => _smoothedUp;

    private void Start()
    {
        _yaw = target != null ? target.eulerAngles.y : 0f;
        _grav = target != null ? target.GetComponent<GravityManipulator>() : null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Smoothly track gravity up vector
        Vector3 desiredUp = _grav != null ? -_grav.GravityDirection : Vector3.up;
        _smoothedUp = Vector3.Slerp(_smoothedUp, desiredUp,
                                    upVectorSpeed * Time.deltaTime).normalized;

        // Mouse input
        _yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        // Position camera behind and above player
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);
        Vector3 pivot = target.position + _smoothedUp * heightOffset;

        transform.position = Vector3.Lerp(transform.position,
                                          pivot + offset,
                                          smoothSpeed * Time.deltaTime);
        // Look at player using correct up vector
        transform.LookAt(pivot, _smoothedUp);
    }
}