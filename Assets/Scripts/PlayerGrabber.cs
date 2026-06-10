using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lets the player grab and throw objects tagged "Throwable".
/// Attach alongside PlayerController on the Player GameObject.
/// </summary>
public class PlayerGrabber : MonoBehaviour
{
    [SerializeField] private float grabRange    = 3f;
    [SerializeField] private float throwStrength = 8f;

    private Camera           _cam;
    private Transform        _holdPoint;
    private ThrowableObject  _held;
    private PlayerInputActions _input;

    private void Awake()
    {
        _input = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _input.Player.Enable();
        _input.Player.Interact.performed += _ =>
        {
            if (!IsPlaying()) return;
            if (_held != null) Release();
            else               TryGrab();
        };
        _input.Player.Throw.performed += _ =>
        {
            if (!IsPlaying()) return;
            if (_held != null) Throw();
        };
    }

    private void OnDisable()
    {
        _input.Player.Disable();
    }

    private bool IsPlaying()
        => GameManager.Instance == null || GameManager.Instance.State == GameManager.GameState.Playing;

    private void Start()
    {
        _cam = Camera.main ?? FindAnyObjectByType<Camera>();

        var go = new GameObject("GrabHoldPoint");
        go.transform.SetParent(_cam.transform, false);
        go.transform.localPosition = new Vector3(0f, 0f, grabRange * 0.6f);
        _holdPoint = go.transform;
    }

    private void Update() { }

    private void TryGrab()
    {
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, grabRange)) return;

        ThrowableObject obj = hit.collider.GetComponentInParent<ThrowableObject>();
        if (obj == null) return;

        _held = obj;
        _held.PickUp(_holdPoint);
    }

    private void Release()
    {
        _held.Drop(Vector3.zero);
        _held = null;
    }

    private void Throw()
    {
        _held.Drop(_cam.transform.forward * throwStrength);
        _held = null;
    }
}