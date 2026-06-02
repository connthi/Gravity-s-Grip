using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PuzzleCompletionTrigger : MonoBehaviour
{
    public PuzzleObjective objective;
    public string requiredTag = "Player";
    public bool requireTorch = false;
    public bool requireLitTorch = false;

    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (objective == null || objective.IsComplete)
            return;

        if (!other.CompareTag(requiredTag))
            return;

        if (requireTorch)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null || !player.HasTorch())
                return;

            if (requireLitTorch)
            {
                TorchPickup carried = player.GetCarriedTorch();
                if (carried == null || !carried.IsLit)
                    return;
            }
        }

        objective.Complete();
    }
}
