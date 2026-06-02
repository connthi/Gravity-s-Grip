using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorSwitch : MonoBehaviour
{
    public PuzzleDoor targetDoor;
    public PuzzleObjective objective;
    public string requiredTag = "Torch";
    public bool requireLitTorch = true;
    public bool allowPlayerCarry = true;

    private void Reset()
    {
        Collider collider = GetComponent<Collider>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (targetDoor == null)
            return;

        if (IsValidActivation(other))
        {
            targetDoor.OpenDoor();
            if (objective != null)
                objective.Complete();
        }
    }

    private bool IsValidActivation(Collider other)
    {
        TorchPickup torch = GetTorchFromCollider(other);
        if (torch != null)
            return !requireLitTorch || torch.IsLit;

        if (allowPlayerCarry)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.HasTorch())
            {
                TorchPickup carried = player.GetCarriedTorch();
                return carried != null && (!requireLitTorch || carried.IsLit);
            }
        }

        return false;
    }

    private TorchPickup GetTorchFromCollider(Collider other)
    {
        return other.GetComponent<TorchPickup>();
    }
}
