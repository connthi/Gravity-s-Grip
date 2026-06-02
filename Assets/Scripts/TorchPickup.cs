using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TorchPickup : MonoBehaviour
{
    public string playerTag = "Player";
    public Transform carryPosition;
    public float pickupRange = 2f;

    private bool isCarried;

    private void Reset()
    {
        Collider collider = GetComponent<Collider>();
        collider.isTrigger = true;
    }

    private void Update()
    {
        if (isCarried)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag(playerTag))
                {
                    PlayerController player = hit.GetComponent<PlayerController>();
                    if (player != null && carryPosition != null)
                    {
                        player.PickupTorch(gameObject);
                        isCarried = true;
                        gameObject.tag = "Torch";
                        return;
                    }
                }
            }
        }
    }
}
