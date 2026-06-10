using UnityEngine;

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

    private void Start()
    {
        _cam = Camera.main ?? FindAnyObjectByType<Camera>();

        var go = new GameObject("GrabHoldPoint");
        go.transform.SetParent(_cam.transform, false);
        go.transform.localPosition = new Vector3(0f, 0f, grabRange * 0.6f);
        _holdPoint = go.transform;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_held != null) Release();
            else               TryGrab();
        }

        if (_held != null && Input.GetMouseButtonDown(0))
            Throw();
    }

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