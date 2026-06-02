using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float lookSensitivity = 2f;
    public float jumpForce = 5f;
    public float groundCheckDistance = 1.1f;
    public LayerMask groundMask = ~0;
    public Transform cameraHolder;
    public Transform torchHolder;
    public UIManager uiManager;

    private Vector3 gravityDirection = Vector3.down;
    private Vector3 velocity;
    private TorchPickup carriedTorch;
    private float pitch;
    private bool isGrounded;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraHolder == null)
        {
            if (Camera.main != null)
                cameraHolder = Camera.main.transform;
        }

        if (uiManager == null)
        {
            uiManager = FindAnyObjectByType<UIManager>();
        }

        RefreshUI();
    }

    void Update()
    {
        HandleLook();
        HandleMove();
        HandleJump();
        HandleTorchInput();
        RefreshUI();
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
        transform.position += input.normalized * moveSpeed * Time.deltaTime;

        if (!IsGrounded())
        {
            velocity += gravityDirection * 9.81f * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
        }
        else
        {
            velocity = Vector3.zero;
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            velocity -= gravityDirection * jumpForce;
        }
    }

    private void HandleTorchInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && carriedTorch != null)
        {
            DropTorch();
        }
    }

    private void RefreshUI()
    {
        if (uiManager == null)
            return;

        if (carriedTorch != null)
        {
            uiManager.SetTorchStatus(carriedTorch.IsLit, carriedTorch.FuelPercent);
        }
        else
        {
            uiManager.SetTorchStatus(false, 0f);
        }
    }

    private bool IsGrounded()
    {
        bool grounded = Physics.Raycast(transform.position, gravityDirection, groundCheckDistance, groundMask);
        isGrounded = grounded;
        return grounded;
    }

    public bool HasTorch()
    {
        return carriedTorch != null;
    }

    public TorchPickup GetCarriedTorch()
    {
        return carriedTorch;
    }

    public void PickupTorch(GameObject torch)
    {
        if (torchHolder == null || carriedTorch != null)
            return;

        TorchPickup torchPickup = torch.GetComponent<TorchPickup>();
        if (torchPickup == null)
            return;

        carriedTorch = torchPickup;
        carriedTorch.PickUp(torchHolder);
    }

    public void DropTorch()
    {
        if (carriedTorch == null)
            return;

        carriedTorch.Drop();
        carriedTorch = null;
    }

    public void SetGravity(Vector3 newGravity)
    {
        gravityDirection = newGravity.normalized;
    }

    public bool IsTorchLit()
    {
        return carriedTorch != null && carriedTorch.IsLit;
    }
}
