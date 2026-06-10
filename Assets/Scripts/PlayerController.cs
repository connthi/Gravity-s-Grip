using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Owns all player input: look, move, jump, torch pickup/drop/toggle.
/// Gravity is a direction vector set externally by GravityPanel.
/// No direct UI calls — fires events that UIManager listens to.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Movement")]
    [SerializeField] private float moveSpeed       = 4f;
    [SerializeField] private float jumpForce       = 5f;
    [SerializeField] private float groundCheckDist = 1.1f;
    [SerializeField] private LayerMask groundMask  = ~0;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float maxPitchAngle   = 80f;

    [Header("References — auto-found if blank")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Transform torchHolder;

    // ── Events ────────────────────────────────────────────────────────────────

    // Raised when the carried torch changes so UIManager can update without being polled.
    public event System.Action<TorchPickup> OnTorchChanged;

    // ── State ─────────────────────────────────────────────────────────────────

    private CharacterController _cc;
    private Vector3 _gravityDir    = Vector3.down;
    private Vector3 _vertVelocity  = Vector3.zero;
    private TorchPickup _torch;
    private float _pitch;

    private PlayerInputActions _input;
    private Vector2 _lookDelta;
    private Vector2 _moveDelta;

    public TorchPickup CarriedTorch => _torch;
    public bool HasTorch            => _torch != null;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _input = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _input.Player.Enable();
        _input.Player.Interact.performed  += _ => { if (!HasTorch) TryPickupNearby(); };
        _input.Player.Drop.performed      += _ => { if (HasTorch) DropTorch(); };
        _input.Player.ToggleTorch.performed += _ => { if (HasTorch) _torch.ToggleLit(); };
        _input.Player.Jump.performed      += _ => { if (IsGrounded()) _vertVelocity = -_gravityDir * jumpForce; };
    }

    private void OnDisable()
    {
        _input.Player.Disable();
    }

    private void Start()
    {
        EnsureCameraHolder();
        EnsureTorchHolder();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing)
            return;

        HandleLook();
        HandleMove();
        HandleJump();
    }

    // ── Input Handlers ────────────────────────────────────────────────────────

    private void HandleLook()
    {
        _lookDelta = _input.Player.Look.ReadValue<Vector2>();
        float yaw   = _lookDelta.x * lookSensitivity;
        float pitch = _lookDelta.y * lookSensitivity;

        transform.Rotate(Vector3.up * yaw);

        _pitch = Mathf.Clamp(_pitch - pitch, -maxPitchAngle, maxPitchAngle);
        cameraHolder.localEulerAngles = new Vector3(_pitch, 0f, 0f);
    }

    private void HandleMove()
    {
        _moveDelta = _input.Player.Move.ReadValue<Vector2>();
        Vector3 move = (transform.forward * _moveDelta.y + transform.right * _moveDelta.x).normalized * moveSpeed;

        if (IsGrounded())
            _vertVelocity = Vector3.zero;
        else
            _vertVelocity += _gravityDir * 9.81f * Time.deltaTime;

        _cc.Move((move + _vertVelocity) * Time.deltaTime);
    }

    private void HandleJump() { /* handled via OnEnable callback */ }

    // ── Torch API (called by TorchPickup proximity detection too) ────────────

    public void PickupTorch(TorchPickup torch)
    {
        if (HasTorch || torch == null) return;

        _torch = torch;
        _torch.AttachTo(torchHolder);
        OnTorchChanged?.Invoke(_torch);
    }

    public void DropTorch()
    {
        if (!HasTorch) return;

        _torch.Detach();
        _torch = null;
        OnTorchChanged?.Invoke(null);
    }

    // ── Gravity API (called by GravityPanel) ─────────────────────────────────

    public void SetGravityDirection(Vector3 dir)
    {
        _gravityDir = dir.normalized;
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private bool IsGrounded()
        => Physics.Raycast(transform.position, _gravityDir, groundCheckDist, groundMask);

    private void TryPickupNearby()
    {
        // Sphere overlap so the player doesn't need to aim precisely at the torch.
        Collider[] hits = Physics.OverlapSphere(transform.position, 2.5f);
        foreach (var hit in hits)
        {
            TorchPickup tp = hit.GetComponentInParent<TorchPickup>();
            if (tp != null) { PickupTorch(tp); return; }
        }
    }

    private void EnsureCameraHolder()
    {
        if (cameraHolder != null) return;

        Camera main = Camera.main ?? FindAnyObjectByType<Camera>();
        if (main != null)
        {
            cameraHolder = main.transform;
            cameraHolder.SetParent(transform, false);
            cameraHolder.localPosition = new Vector3(0f, 0.6f, 0f);
            cameraHolder.localRotation = Quaternion.identity;
        }
        else
        {
            GameObject go = new GameObject("MainCamera") { tag = "MainCamera" };
            go.AddComponent<Camera>();
            cameraHolder = go.transform;
            cameraHolder.SetParent(transform, false);
            cameraHolder.localPosition = new Vector3(0f, 0.6f, 0f);
        }
    }

    private void EnsureTorchHolder()
    {
        if (torchHolder != null) return;

        GameObject go = new GameObject("TorchHolder");
        // Parent to camera so the held torch follows pitch.
        go.transform.SetParent(cameraHolder, false);
        go.transform.localPosition = new Vector3(0.3f, -0.2f, 0.6f);
        torchHolder = go.transform;
    }
}