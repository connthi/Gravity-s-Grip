using UnityEngine;

/// <summary>
/// Lets the player grab and throw this object via PlayerGrabber.
/// If a GravityAffectedObject component is present it is respected,
/// otherwise falls back to Rigidbody.useGravity.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ThrowableObject : MonoBehaviour
{
    private Rigidbody            _rb;
    private GravityAffectedObject _gao;
    private bool                 _held;

    private void Awake()
    {
        _rb  = GetComponent<Rigidbody>();
        _gao = GetComponent<GravityAffectedObject>();
    }

    private void Update()
    {
        if (!_held) return;
        _rb.linearVelocity  = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    public void PickUp(Transform holdPoint)
    {
        _held            = true;
        _rb.isKinematic  = true;
        if (_gao != null) _gao.enabled = false;

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Drop(Vector3 impulse)
    {
        _held            = false;
        _rb.isKinematic  = false;
        if (_gao != null) _gao.enabled = true;

        transform.SetParent(null);
        if (impulse != Vector3.zero)
            _rb.AddForce(impulse, ForceMode.Impulse);
    }
}