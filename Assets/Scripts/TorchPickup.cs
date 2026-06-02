using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TorchPickup : MonoBehaviour
{
    public float pickupRange = 2f;
    public bool startLit = true;
    public float maxFuel = 120f;
    public float burnRate = 1f;
    public bool autoIgniteOnPickup = true;

    private TorchLight torchLight;
    private FireSimulation fireSimulation;
    private Collider torchCollider;
    private bool isCarried;
    private float currentFuel;

    public bool IsLit => torchLight != null && torchLight.IsLit;
    public float FuelPercent => maxFuel <= 0f ? 0f : currentFuel / maxFuel;

    private void Awake()
    {
        torchLight = GetComponent<TorchLight>();
        fireSimulation = GetComponent<FireSimulation>();
        if (fireSimulation == null)
        {
            fireSimulation = gameObject.AddComponent<FireSimulation>();
        }

        torchCollider = GetComponent<Collider>();
        torchCollider.isTrigger = true;

        currentFuel = maxFuel;
        if (torchLight != null)
        {
            torchLight.SetLit(startLit);
        }

        if (fireSimulation != null)
        {
            if (startLit)
                fireSimulation.ResumeFire();
            else
                fireSimulation.PauseFire();
        }
    }

    private void Update()
    {
        if (IsLit)
        {
            BurnFuel();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickup();
        }

        if (isCarried && Input.GetKeyDown(KeyCode.F))
        {
            if (IsLit)
                Extinguish();
            else
                Ignite();
        }
    }

    private void BurnFuel()
    {
        if (currentFuel <= 0f)
        {
            Extinguish();
            return;
        }

        currentFuel -= burnRate * Time.deltaTime;
        currentFuel = Mathf.Max(0f, currentFuel);

        if (currentFuel <= 0f)
            Extinguish();
    }

    private void TryPickup()
    {
        if (isCarried)
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);
        foreach (Collider hit in hits)
        {
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null)
            {
                player.PickupTorch(gameObject);
                return;
            }
        }
    }

    public void PickUp(Transform parent)
    {
        if (isCarried)
            return;

        isCarried = true;
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        torchCollider.enabled = false;

        if (autoIgniteOnPickup && !IsLit)
            Ignite();
    }

    public void Drop()
    {
        if (!isCarried)
            return;

        isCarried = false;
        transform.SetParent(null);
        torchCollider.enabled = true;
    }

    public void Ignite()
    {
        if (currentFuel <= 0f)
            return;

        if (torchLight != null)
            torchLight.SetLit(true);

        if (fireSimulation != null)
            fireSimulation.ResumeFire();
    }

    public void Extinguish()
    {
        if (torchLight != null)
            torchLight.SetLit(false);

        if (fireSimulation != null)
            fireSimulation.PauseFire();
    }

    public void RefillFuel(float amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + amount, 0f, maxFuel);
        if (currentFuel > 0f && startLit)
            Ignite();
    }

}
