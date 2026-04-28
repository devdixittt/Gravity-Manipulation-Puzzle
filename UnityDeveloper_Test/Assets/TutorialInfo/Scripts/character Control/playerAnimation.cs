using UnityEngine;

/// <summary>
/// Bridges game logic to the Animator component.
/// Parameter names must match those in the Animator Controller in the base project.
/// </summary>
[RequireComponent(typeof(Animator))]
public class playerAnimation : MonoBehaviour
{
    private Animator _animator;

    // Animator parameter hashes (faster than string lookup)
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int FallHash = Animator.StringToHash("IsFalling");

    private InputController _movement;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _movement = GetComponent<InputController>();
    }

    private void Update()
    {
        // Keep grounded/falling states in sync every frame
        _animator.SetBool(IsGroundedHash, _movement.IsGrounded);
        _animator.SetBool(FallHash, !_movement.IsGrounded);
    }

    public void SetGrounded(bool isGrounded) => _animator.SetBool(IsGroundedHash, isGrounded);
    /// <summary>Call when horizontal movement starts or stops.</summary>
    public void SetMoving(bool isMoving) => _animator.SetBool(IsMovingHash, isMoving);

    /// <summary>Fires the jump trigger.</summary>
}