using UnityEngine;

/// <summary>
/// Triggers the win condition when the player reaches the exit,
/// provided enough objectives are complete.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PuzzleExitTrigger : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null) return;

        var tracker = ObjectiveTracker.Instance;
        if (tracker == null) return;

        if (tracker.CompletedCount >= tracker.RequiredToWin)
        {
            GameManager.Instance?.TriggerWin();
        }
        else
        {
            int remaining = tracker.RequiredToWin - tracker.CompletedCount;
            UIManager.Instance?.ShowHint($"Complete {remaining} more puzzle(s) before leaving.");
        }
    }
}