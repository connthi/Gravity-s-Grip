using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityAffectedObject : MonoBehaviour
{
    public Vector3 gravityDirection = Physics.gravity.normalized;
    public float gravityStrength = 9.81f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        rb.AddForce(gravityDirection.normalized * gravityStrength * rb.mass, ForceMode.Force);
    }

    public void SetGravity(Vector3 newGravity)
    {
        gravityDirection = newGravity.normalized;
    }
}
