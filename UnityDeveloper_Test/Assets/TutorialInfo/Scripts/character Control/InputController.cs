using UnityEngine;

/// <summary>
/// Handles WASD movement and Space jump relative to the current gravity direction.
/// Works with CharacterController; gravity is applied manually to support custom gravity axes.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class InputController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravityMultiplier = 2f;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    private CharacterController _controller;
    private GravityManipulator _gravityManipulator;
    private playerAnimation _animController;

    private Vector3 _velocity;       // Current velocity (includes gravity component)
    private bool _isGrounded;
    private float _fallTimer;        // Tracks how long player has been airborne

    [Header("Fall-Death Settings")]
    [SerializeField] private float maxFallTime = 3f; // seconds of free-fall before game over

    public bool IsGrounded => _isGrounded;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
       // _gravityManipulator = GetComponent<GravityManipulator>();
        _animController = GetComponent<playerAnimation>();
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;

        CheckGrounded();
        HandleMovement();
        //HandleJump();
        ApplyGravity();
        TrackFreeFall();
    }

    /// <summary>Uses a sphere cast along the negative gravity axis to detect the ground.</summary>
    private void CheckGrounded()
    {
        Vector3 gravDir = _gravityManipulator != null
            ? _gravityManipulator.GravityDirection
            : Vector3.down;

        Vector3 origin = transform.position - gravDir * (_controller.height * 0.5f - _controller.radius);
        _isGrounded = Physics.CheckSphere(origin, _controller.radius + groundCheckDistance, groundLayer);

        // Reset vertical velocity component when grounded
        if (_isGrounded)
        {
            Vector3 gravAxis = gravDir;
            float downwardSpeed = Vector3.Dot(_velocity, gravAxis);
            if (downwardSpeed > 0f)
                _velocity -= gravAxis * downwardSpeed; // cancel only the gravity-axis velocity
        }
    }

    /// <summary>Moves the character based on WASD input, relative to camera and gravity plane.</summary>
    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        // Build movement axes perpendicular to gravity
        Vector3 gravDir = _gravityManipulator != null
            ? _gravityManipulator.GravityDirection
            : Vector3.down;

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        // Project camera directions onto the plane perpendicular to gravity
        Vector3 forward = Vector3.ProjectOnPlane(camForward, -gravDir).normalized;
        Vector3 right = Vector3.ProjectOnPlane(camRight, -gravDir).normalized;

        Vector3 moveDir = (forward * v + right * h).normalized;
        Vector3 moveVelocity = moveDir * moveSpeed;

        // Preserve gravity-axis velocity, replace planar velocity
        Vector3 gravComponent = Vector3.Project(_velocity, gravDir);
        _velocity = moveVelocity + gravComponent;

        _controller.Move(_velocity * Time.deltaTime);

        // Rotate player to face movement direction
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, -gravDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);
        }

        // Drive animations
        _animController?.SetMoving(moveDir.sqrMagnitude > 0.01f);
    }

    /// <summary>Applies an impulse along the anti-gravity axis when Space is pressed.</summary>
    /*private void HandleJump()
    {
        if (_isGrounded && Input.GetButtonDown("Jump"))
        {
            Vector3 gravDir = _gravityManipulator != null
                ? _gravityManipulator.GravityDirection
                : Vector3.down;

            // Remove existing velocity on gravity axis, then add jump impulse
            _velocity -= Vector3.Project(_velocity, gravDir);
            _velocity += -gravDir * jumpForce;

            //_animController?.TriggerJump();
        }
    }*/

    /// <summary>Continuously accelerates the player along the gravity direction.</summary>
    private void ApplyGravity()
    {
        if (_isGrounded) return;

        Vector3 gravDir = _gravityManipulator != null
            ? _gravityManipulator.GravityDirection
            : Vector3.down;

        _velocity += gravDir * (Mathf.Abs(Physics.gravity.y) * gravityMultiplier * Time.deltaTime);
    }

    /// <summary>Tracks airborne time and triggers GameOver if the player is in free-fall too long.</summary>
    private void TrackFreeFall()
    {
        if (!_isGrounded)
        {
            _fallTimer += Time.deltaTime;
            if (_fallTimer >= maxFallTime)
                GameManager.Instance?.TriggerGameOver("Free-fall detected!");
        }
        else
        {
            _fallTimer = 0f;
        }
    }
}