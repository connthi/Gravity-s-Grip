using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BlockPlacementPuzzle : MonoBehaviour
{
    public PuzzleObjective objective;
    public string requiredTag = "PuzzleBlock";
    public float completionTime = 1.5f;
    public bool requireStableRotation = false;
    public Vector3 targetRotation = Vector3.zero;
    public float rotationToleranceDegrees = 15f;
    public bool resetTimerOnExit = true;

    private float timer;

    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (objective == null || objective.IsComplete)
            return;

        if (!other.CompareTag(requiredTag))
        {
            if (resetTimerOnExit)
                timer = 0f;
            return;
        }

        if (requireStableRotation)
        {
            Quaternion expected = Quaternion.Euler(targetRotation);
            Quaternion actual = Quaternion.Euler(other.transform.localEulerAngles);
            float angle = Quaternion.Angle(expected, actual);
            if (angle > rotationToleranceDegrees)
            {
                timer = 0f;
                return;
            }
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
