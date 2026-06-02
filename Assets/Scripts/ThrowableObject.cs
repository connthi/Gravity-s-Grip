using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrowableObject : MonoBehaviour
{
    public float holdDistance = 1.5f;
    public float throwForce = 8f;

    private Rigidbody rb;
    private bool isHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isHeld)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void PickUp(Transform holdPoint)
    {
        isHeld = true;
        rb.isKinematic = true;
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Drop(Vector3 force)
    {
        isHeld = false;
        transform.SetParent(null);
        rb.isKinematic = false;

        if (force != Vector3.zero)
        {
            rb.AddForce(force, ForceMode.Impulse);
        }
    }
}
