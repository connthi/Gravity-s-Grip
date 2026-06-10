using UnityEngine;

/// <summary>
/// Completes a PuzzleObjective when the correct tagged object rests
/// inside this trigger for a sustained duration.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BlockPlacementPuzzle : MonoBehaviour
{
    [SerializeField] private PuzzleObjective objective;
    [SerializeField] private string          requiredTag         = "PuzzleBlock";
    [SerializeField] private float           requiredHoldSeconds = 1.5f;

    [Tooltip("If enabled, the block's rotation must roughly match targetRotation.")]
    [SerializeField] private bool  requireRotation        = false;
    [SerializeField] private Vector3 targetRotation       = Vector3.zero;
    [SerializeField] private float   rotationTolerance    = 15f;

    private float _timer;
    private bool  _blockPresent;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(requiredTag)) _blockPresent = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(requiredTag))
        {
            _blockPresent = false;
            _timer = 0f;
        }
    }

    private void Update()
    {
        if (objective == null || objective.IsComplete) return;
        if (!_blockPresent) return;

        if (requireRotation && !CheckRotation()) { _timer = 0f; return; }

        _timer += Time.deltaTime;
        if (_timer >= requiredHoldSeconds)
            objective.Complete();
    }

    private bool CheckRotation()
    {
        // Find the block in the trigger to check its rotation.
        Collider[] hits = Physics.OverlapBox(
            transform.position,
            transform.lossyScale * 0.5f,
            transform.rotation);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag(requiredTag)) continue;
            float angle = Quaternion.Angle(
                Quaternion.Euler(targetRotation),
                Quaternion.Euler(hit.transform.eulerAngles));
            return angle <= rotationTolerance;
        }
        return false;
    }
}