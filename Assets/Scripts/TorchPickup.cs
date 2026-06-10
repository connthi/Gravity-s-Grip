using UnityEngine;

/// <summary>
/// Manages a single torch: fuel, lit state, and attach/detach to a holder.
/// Does NOT read input — PlayerController owns all key handling.
/// Does NOT touch UI — fire events that listeners can subscribe to.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TorchPickup : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("State")]
    [SerializeField] private bool startLit       = true;
    [SerializeField] private float maxFuel       = 120f;
    [SerializeField] private float burnRate      = 1f;

    [Header("Auto pickup")]
    [Tooltip("Player walks into this radius and automatically picks it up.")]
    [SerializeField] private float autoPickupRadius = 0.8f;

    // ── Events ────────────────────────────────────────────────────────────────

    public event System.Action<bool>  OnLitChanged;    // (isLit)
    public event System.Action<float> OnFuelChanged;   // (fuelPercent 0–1)

    // ── Public State ──────────────────────────────────────────────────────────

    public bool  IsLit       => _lit;
    public float FuelPercent => maxFuel > 0f ? _fuel / maxFuel : 0f;
    public bool  IsCarried   => _carried;

    // ── Private ───────────────────────────────────────────────────────────────

    private TorchLight      _torchLight;
    private FireSimulation  _fire;
    private Collider        _col;
    private Rigidbody       _rb;

    private bool  _lit;
    private float _fuel;
    private bool  _carried;

    private Vector3 _originalScale;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        _torchLight    = GetComponent<TorchLight>();
        _fire          = GetComponent<FireSimulation>();
        _col           = GetComponent<Collider>();
        _rb            = GetComponent<Rigidbody>();
        _originalScale = transform.localScale;
        _fuel          = maxFuel;

        _col.isTrigger = true;
        SetLit(startLit);
    }

    private void Update()
    {
        if (_lit) BurnFuel();

        // Auto-pickup: any PlayerController within radius grabs this torch.
        if (!_carried && autoPickupRadius > 0f)
            CheckAutoPickup();
    }

    // ── Attach / Detach ───────────────────────────────────────────────────────

    public void AttachTo(Transform holder)
    {
        _carried = true;

        if (_rb != null) _rb.isKinematic = true;
        _col.enabled = false;

        transform.SetParent(holder, false);
        transform.localPosition = new Vector3(0.15f, -0.18f, 0.4f);
        transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
        transform.localScale    = _originalScale * 0.45f;

        // Auto-ignite when picked up if there is fuel.
        if (!_lit && _fuel > 0f) SetLit(true);
    }

    public void Detach()
    {
        _carried = false;

        Transform prev = transform.parent;
        transform.SetParent(null);

        // Drop slightly in front of where the holder was.
        if (prev != null)
            transform.position = prev.position + prev.forward * 0.6f;

        transform.localScale = _originalScale;

        _col.enabled = true;
        if (_rb != null) _rb.isKinematic = false;
    }

    // ── Lit State ─────────────────────────────────────────────────────────────

    public void ToggleLit()
    {
        if (!_lit && _fuel <= 0f) return; // Can't relight with no fuel.
        SetLit(!_lit);
    }

    public void Ignite()
    {
        if (_fuel > 0f) SetLit(true);
    }

    public void Extinguish()   => SetLit(false);

    public void RefillFuel(float amount)
    {
        _fuel = Mathf.Clamp(_fuel + amount, 0f, maxFuel);
        OnFuelChanged?.Invoke(FuelPercent);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void SetLit(bool lit)
    {
        _lit = lit;
        _torchLight?.SetLit(lit);

        if (_fire != null)
        {
            if (lit) _fire.ResumeFire();
            else     _fire.PauseFire();
        }

        OnLitChanged?.Invoke(lit);
    }

    private void BurnFuel()
    {
        _fuel -= burnRate * Time.deltaTime;
        _fuel  = Mathf.Max(0f, _fuel);
        OnFuelChanged?.Invoke(FuelPercent);

        if (_fuel <= 0f) SetLit(false);
    }

    private void CheckAutoPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, autoPickupRadius);
        foreach (var hit in hits)
        {
            PlayerController pc = hit.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.PickupTorch(this);
                return;
            }
        }
    }
}