using UnityEngine;

public class PlayerGrabber : MonoBehaviour
{
    public Camera playerCamera;
    public Transform holdPoint;
    public float grabRange = 3f;
    public float throwStrength = 8f;
    public LayerMask grabbableLayer;
    public string grabbableTag = "Throwable";

    private ThrowableObject heldObject;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject != null)
            {
                Drop();
            }
            else
            {
                TryGrab();
            }
        }

        if (heldObject != null && Input.GetMouseButtonDown(0))
        {
            Throw();
        }
    }

    private void TryGrab()
    {
        if (playerCamera == null || holdPoint == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, grabRange, grabbableLayer))
        {
            if (hit.collider.CompareTag(grabbableTag))
            {
                ThrowableObject throwable = hit.collider.GetComponent<ThrowableObject>();
                if (throwable != null)
                {
                    heldObject = throwable;
                    heldObject.PickUp(holdPoint);
                }
            }
        }
    }

    private void Drop()
    {
        if (heldObject == null)
            return;

        heldObject.Drop(Vector3.zero);
        heldObject = null;
    }

    private void Throw()
    {
        if (heldObject == null)
            return;

        Vector3 force = playerCamera.transform.forward * throwStrength;
        heldObject.Drop(force);
        heldObject = null;
    }
}
