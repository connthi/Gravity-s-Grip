using UnityEngine;

/// <summary>
/// Applies a custom gravity direction to a Rigidbody.
/// Disable Unity's built-in gravity (useGravity = false) on the same object.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class GravityAffectedObject : MonoBehaviour
{
    [SerializeField] private Vector3 gravityDirection = Vector3.down;
    [SerializeField] private float   gravityStrength  = 9.81f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb            = GetComponent<Rigidbody>();
        _rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        _rb.AddForce(gravityDirection.normalized * gravityStrength * _rb.mass, ForceMode.Force);
    }

    public void SetGravity(Vector3 direction)
    {
        gravityDirection = direction.normalized;
    }
}