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
        if (other.CompareTag(requiredTag))
        {
            TorchPickup torch = other.GetComponent<TorchPickup>();
            if (torch == null)
                return false;

            return !requireLitTorch || torch.IsLit;
        }

        if (allowPlayerCarry && other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null || !player.HasTorch())
                return false;

            TorchPickup carried = player.GetCarriedTorch();
            return carried != null && (!requireLitTorch || carried.IsLit);
        }

        return false;
    }
}
