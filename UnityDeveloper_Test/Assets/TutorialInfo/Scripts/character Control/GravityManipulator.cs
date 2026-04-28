using System.Collections;
using UnityEngine;

/// <summary>
/// Handles gravity direction selection via arrow keys and applies gravity on Enter.
/// Shows a hologram arrow indicating the selected direction.
/// </summary>
public class GravityManipulator : MonoBehaviour
{
    [Header("Gravity")]
    [SerializeField] private float gravityStrength = 20f;

    [Header("Surface Snap")]
    [SerializeField] private float raycastLength = 50f;
    [SerializeField] private float skinOffset = 0.1f;
    [SerializeField] private LayerMask surfaceLayer;

    [Header("Transition")]
    [SerializeField] private float transitionDuration = 0.4f;

    [Header("Hologram")]
    [SerializeField] private GameObject hologramPrefab;

    // ── Public State ──────────────────────────────────────────────────────────
    public Vector3 GravityDirection { get; private set; } = Vector3.down;
    public Vector3 Up => -GravityDirection;
    public bool IsTransitioning { get; private set; } = false;
    public float VerticalVelocity { get; set; } = 0f;

    // ── Private ───────────────────────────────────────────────────────────────
    private Vector3 _pendingDirection = Vector3.down;
    private CharacterController _cc;
    private GameObject _hologram;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    private void Start()
    {
        if (hologramPrefab != null)
        {
            _hologram = Instantiate(hologramPrefab);
            _hologram.SetActive(false);
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;
        if (IsTransitioning) return;

        HandleInput();
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private void HandleInput()
    {
        Vector3 newDir = _pendingDirection;

        if (Input.GetKeyDown(KeyCode.UpArrow)) newDir = Vector3.forward;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) newDir = Vector3.back;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) newDir = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) newDir = Vector3.right;
        else if (Input.GetKeyDown(KeyCode.PageUp)) newDir = Vector3.up;
        else if (Input.GetKeyDown(KeyCode.PageDown)) newDir = Vector3.down;

        if (newDir != _pendingDirection)
        {
            _pendingDirection = newDir;
            ShowHologram();
        }

        bool anyKey = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||
                      Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
                      Input.GetKey(KeyCode.PageUp) || Input.GetKey(KeyCode.PageDown);

        if (!anyKey) HideHologram();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (_pendingDirection == GravityDirection) return;
            HideHologram();
            StartCoroutine(DoGravityTransition(_pendingDirection));
        }
    }

    // ── Gravity Transition ────────────────────────────────────────────────────

    private IEnumerator DoGravityTransition(Vector3 newGravDir)
    {
        IsTransitioning = true;
        VerticalVelocity = 0f;

        // ── 1. Find the surface in new gravity direction ───────────────────
        bool hit = FindSurface(newGravDir, out Vector3 surfacePoint,
                                          out Vector3 surfaceNormal);

        if (!hit)
        {
            Debug.LogWarning("[Gravity] No surface found in direction: " + newGravDir);
            IsTransitioning = false;
            yield break;
        }

        // ── 2. Calculate where player should stand ────────────────────────
        // Place player so feet are on surface with a small skin gap
        float halfHeight = _cc.height / 2f + skinOffset;
        Vector3 targetPos = surfacePoint + (-newGravDir) * halfHeight;

        // ── 3. Calculate player's new rotation ───────────────────────────
        // New up = opposite of gravity
        // Preserve forward direction projected onto new up plane
        Vector3 newUp = -newGravDir;
        Vector3 projForward = Vector3.ProjectOnPlane(transform.forward, newUp);

        if (projForward.sqrMagnitude < 0.001f)
            projForward = Vector3.ProjectOnPlane(transform.right, newUp);

        Quaternion targetRot = Quaternion.LookRotation(projForward.normalized, newUp);

        // ── 4. Smoothly move + rotate player ─────────────────────────────
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        _cc.enabled = false;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        // Snap to exact final values
        transform.position = targetPos;
        transform.rotation = targetRot;

        _cc.enabled = true;
        GravityDirection = newGravDir;
        VerticalVelocity = 0f;
        IsTransitioning = false;
    }

    // ── Surface Detection ─────────────────────────────────────────────────────

    /// <summary>
    /// Casts a ray from the player's center in the new gravity direction.
    /// Returns the hit point and normal of the surface found.
    private bool FindSurface(Vector3 gravDir, out Vector3 point, out Vector3 normal)
    {
        point = Vector3.zero;
        normal = Vector3.up;

        Ray ray = new Ray(transform.position, gravDir);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastLength, surfaceLayer))
        {
            point = hit.point;
            normal = hit.normal;
            return true;
        }

        // Wider fallback using SphereCast
        if (Physics.SphereCast(ray, 0.5f, out RaycastHit hit2, raycastLength, surfaceLayer))
        {
            point = hit2.point;
            normal = hit2.normal;
            return true;
        }

        return false;
    }

    // ── Hologram ──────────────────────────────────────────────────────────────

    private void ShowHologram()
    {
        if (_hologram == null) return;

        if (FindSurface(_pendingDirection, out Vector3 point, out Vector3 normal))
        {
            float halfHeight = _cc.height / 2f + skinOffset;
            Vector3 newUp = -_pendingDirection;
            Vector3 pos = point + newUp * halfHeight;
            Vector3 projFwd = Vector3.ProjectOnPlane(transform.forward, newUp);

            if (projFwd.sqrMagnitude < 0.001f)
                projFwd = Vector3.ProjectOnPlane(Vector3.forward, newUp);

            _hologram.transform.position = pos;
            _hologram.transform.rotation = Quaternion.LookRotation(
                                               projFwd.normalized, newUp);
            _hologram.SetActive(true);
        }
    }

    private void HideHologram()
    {
        if (_hologram != null)
            _hologram.SetActive(false);
    }

    public Vector3 GetGravityVector() => GravityDirection * gravityStrength;
}