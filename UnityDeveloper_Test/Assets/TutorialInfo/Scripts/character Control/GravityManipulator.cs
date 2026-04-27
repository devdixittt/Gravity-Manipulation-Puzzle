using UnityEngine;

/// <summary>
/// Handles gravity direction selection via arrow keys and applies gravity on Enter.
/// Shows a hologram arrow indicating the selected direction.
/// </summary>
public class GravityManipulator : MonoBehaviour
{
    [Header("Gravity Settings")]
    [SerializeField] private float gravityStrength = 20f;
    [SerializeField] private float gravityTransitionSpeed = 5f;

    [Header("Hologram")]
    [SerializeField] private GameObject hologramPrefab;   // Arrow/ghost prefab from base project
    [SerializeField] private float hologramDistance = 2f;

    // Public so PlayerMovement can read it
    public Vector3 GravityDirection { get; private set; } = Vector3.down;

    private Vector3 _pendingGravityDir = Vector3.down;   // Direction chosen but not yet confirmed
    private Vector3 _currentGravityDir = Vector3.down;
    private GameObject _hologramInstance;
    private bool _hologramVisible = false;

    // All 6 possible gravity directions
    private readonly Vector3[] _gravityOptions =
    {
        Vector3.down,
        Vector3.up,
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back
    };

    private void Start()
    {
        if (hologramPrefab != null)
        {
            _hologramInstance = Instantiate(hologramPrefab, transform.position, Quaternion.identity);
            _hologramInstance.SetActive(false);
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;

        HandleDirectionInput();
        HandleConfirmInput();
        SmoothGravity();
    }

    /// <summary>Arrow keys cycle through gravity directions and show the hologram.</summary>
    private void HandleDirectionInput()
    {
        Vector3 newDir = _pendingGravityDir;

        if (Input.GetKeyDown(KeyCode.UpArrow)) newDir = Vector3.forward;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) newDir = Vector3.back;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) newDir = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) newDir = Vector3.right;

        // Additional up/down gravity with Page keys (optional bonus)
        if (Input.GetKeyDown(KeyCode.PageUp)) newDir = Vector3.up;
        else if (Input.GetKeyDown(KeyCode.PageDown)) newDir = Vector3.down;

        bool arrowPressed = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||
                            Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
                            Input.GetKey(KeyCode.PageUp) || Input.GetKey(KeyCode.PageDown);

        if (newDir != _pendingGravityDir)
        {
            _pendingGravityDir = newDir;
            UpdateHologram();
        }

        // Hide hologram when no arrow key is held
        if (!arrowPressed && _hologramVisible)
            SetHologramVisible(false);
        else if (arrowPressed && !_hologramVisible)
            SetHologramVisible(true);
    }

    /// <summary>Enter confirms and applies the selected gravity direction.</summary>
    private void HandleConfirmInput()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            _currentGravityDir = _pendingGravityDir;
            Debug.Log($"[GravityManipulator] Gravity set to: {_currentGravityDir}");
            SetHologramVisible(false);
        }
    }

    /// <summary>Smoothly interpolates GravityDirection toward the confirmed direction.</summary>
    private void SmoothGravity()
    {
        GravityDirection = Vector3.Slerp(GravityDirection, _currentGravityDir,
                                         gravityTransitionSpeed * Time.deltaTime).normalized;
    }

    /// <summary>Positions and orients the hologram to preview the chosen gravity direction.</summary>
    private void UpdateHologram()
    {
        if (_hologramInstance == null) return;

        Vector3 offset = _pendingGravityDir * hologramDistance;
        _hologramInstance.transform.position = transform.position + offset;

        // Point the hologram arrow along the pending gravity direction
        if (_pendingGravityDir != Vector3.zero)
            _hologramInstance.transform.rotation =
                Quaternion.LookRotation(_pendingGravityDir, Vector3.up);
    }

    private void SetHologramVisible(bool visible)
    {
        _hologramVisible = visible;
        if (_hologramInstance != null)
            _hologramInstance.SetActive(visible);
    }

    /// <summary>Returns the scaled gravity vector for external use.</summary>
    public Vector3 GetGravityVector() => GravityDirection * gravityStrength;
}