using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float lookSensitivity = 2f;
    public Transform cameraHolder;
    public Transform torchHolder;

    private Vector3 gravityDirection = Vector3.down;
    private Vector3 velocity;
    private GameObject carriedTorch;
    private float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraHolder == null)
        {
            cameraHolder = Camera.main?.transform;
        }
    }

    void Update()
    {
        HandleLook();
        HandleMove();
        HandleTorchInput();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        if (cameraHolder != null)
        {
            cameraHolder.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }
    }

    private void HandleMove()
    {
        float forward = Input.GetAxis("Vertical");
        float strafe = Input.GetAxis("Horizontal");

        Vector3 input = transform.forward * forward + transform.right * strafe;
        Vector3 movement = input.normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;

        velocity += gravityDirection * 9.81f * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        if (IsGrounded())
        {
            velocity = Vector3.zero;
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, gravityDirection, 1.1f);
    }

    private void HandleTorchInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && carriedTorch != null)
        {
            DropTorch();
        }
    }

    public void SetGravity(Vector3 newGravity)
    {
        gravityDirection = newGravity.normalized;
    }

    public bool HasTorch()
    {
        return carriedTorch != null;
    }

    public void PickupTorch(GameObject torch)
    {
        if (torchHolder == null || carriedTorch != null)
            return;

        carriedTorch = torch;
        carriedTorch.transform.SetParent(torchHolder);
        carriedTorch.transform.localPosition = Vector3.zero;
        carriedTorch.transform.localRotation = Quaternion.identity;
        Collider torchCollider = carriedTorch.GetComponent<Collider>();
        if (torchCollider != null)
        {
            torchCollider.enabled = false;
        }
    }

    public void DropTorch()
    {
        if (carriedTorch == null)
            return;

        carriedTorch.transform.SetParent(null);
        carriedTorch = null;
    }
}
