using UnityEngine;

/// </summary>
[RequireComponent(typeof(CharacterController))]
public class InputController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravityMultiplier = 2.5f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private CharacterController _cc;
    private GravityManipulator _grav;
    private playerAnimation _anim;
    private ThirdPersonCamera _cam;

    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _grav = GetComponent<GravityManipulator>();
        _anim = GetComponent<playerAnimation>();
        _cam = Camera.main?.GetComponent<ThirdPersonCamera>();
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;
        if (_grav.IsTransitioning) return;

        CheckGrounded();
        Move();
        ApplyGravity();
    }

    // ── Ground Check ──────────────────────────────────────────────────────────
    private void CheckGrounded()
    {
        // Feet position = center shifted along gravity direction by half height
        Vector3 feet = transform.position
                       + _grav.GravityDirection * (_cc.height * 0.5f - _cc.radius);

        IsGrounded = Physics.CheckSphere(feet,
                                         _cc.radius + groundCheckDistance,
                                         groundLayer);

        if (IsGrounded && _grav.VerticalVelocity > 0f)
            _grav.VerticalVelocity = 0f;

        _anim?.SetGrounded(IsGrounded);
    }

    // ── Movement ──────────────────────────────────────────────────────────────
    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Project camera axes onto the gravity plane so movement
        // always feels correct regardless of which surface player is on
        Vector3 camForward = _cam != null ? _cam.transform.forward : transform.forward;
        Vector3 camRight = _cam != null ? _cam.transform.right : transform.right;

        Vector3 flatForward = Vector3.ProjectOnPlane(camForward, _grav.Up).normalized;
        Vector3 flatRight = Vector3.ProjectOnPlane(camRight, _grav.Up).normalized;

        Vector3 inputDir = (flatForward * v + flatRight * h).normalized;
        bool hasInput = inputDir.sqrMagnitude > 0.01f;

        // Combine movement + gravity into one Move call
        Vector3 moveVelocity = hasInput ? inputDir * moveSpeed : Vector3.zero;
        Vector3 gravVelocity = _grav.GravityDirection * _grav.VerticalVelocity;

        _cc.Move((moveVelocity) * Time.deltaTime);

        // Rotate player to face movement direction
        if (hasInput)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir, _grav.Up);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                     targetRot,
                                                     rotationSpeed * Time.deltaTime);
        }

        _anim?.SetMoving(hasInput);
    }

    // ── Gravity ───────────────────────────────────────────────────────────────

    private void ApplyGravity()
    {
        if (IsGrounded) return;

        _grav.VerticalVelocity += Mathf.Abs(Physics.gravity.y)
                                  * gravityMultiplier
                                  * Time.deltaTime;
    }
}