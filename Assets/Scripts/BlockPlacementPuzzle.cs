using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BlockPlacementPuzzle : MonoBehaviour
{
    public PuzzleObjective objective;
    public string requiredTag = "PuzzleBlock";
    public float completionTime = 1.5f;

    private float timer;

    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (objective == null || objective.IsComplete)
        {
            return;
        }

        if (!other.CompareTag(requiredTag))
        {
            timer = 0f;
            return;
        }

        timer += Time.deltaTime;
        if (timer >= completionTime)
        {
            objective.Complete();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(requiredTag))
        {
            timer = 0f;
        }
    }
}
