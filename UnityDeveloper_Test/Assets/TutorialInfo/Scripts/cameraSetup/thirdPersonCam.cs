using UnityEngine;

/// <summary>
/// Third-person camera that orbits the player and adjusts its up-vector
/// to match the current gravity direction.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;          // Drag the Player here

    [Header("Orbit Settings")]
    [SerializeField] private float distance = 6f;
    [SerializeField] private float heightOffset = 2f;
    [SerializeField] private float rotationSpeed = 3f;

    [Header("Clamp")]
    [SerializeField] private float minVerticalAngle = -20f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionRadius = 0.3f;

    private float _yaw;    // Horizontal orbit angle
    private float _pitch;  // Vertical orbit angle

    private GravityManipulator _gravManipulator;

    private void Start()
    {
        if (target != null)
            _gravManipulator = target.GetComponent<GravityManipulator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Mouse input
        _yaw += Input.GetAxis("Mouse X") * rotationSpeed;
        _pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
        _pitch = Mathf.Clamp(_pitch, minVerticalAngle, maxVerticalAngle);

        // Determine up-vector from current gravity
        Vector3 gravDir = _gravManipulator != null
            ? _gravManipulator.GravityDirection
            : Vector3.down;
        Vector3 up = -gravDir;

        // Build camera rotation
        Quaternion rotation = Quaternion.AngleAxis(_yaw, up) *
                              Quaternion.AngleAxis(_pitch, Vector3.right);

        Vector3 pivotPos = target.position + up * heightOffset;
        Vector3 desiredPos = pivotPos - rotation * Vector3.forward * distance;

        // Simple collision check
        Vector3 finalPos = desiredPos;
        if (Physics.SphereCast(pivotPos, collisionRadius,
                               (desiredPos - pivotPos).normalized,
                               out RaycastHit hit,
                               distance, collisionMask))
        {
            finalPos = pivotPos + (desiredPos - pivotPos).normalized * (hit.distance - collisionRadius);
        }

        transform.position = finalPos;
        transform.LookAt(pivotPos, up);
    }
}