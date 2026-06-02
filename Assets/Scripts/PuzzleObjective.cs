using UnityEngine;
using UnityEngine.Events;

public class PuzzleObjective : MonoBehaviour
{
    public string title = "Puzzle Objective";
    [TextArea(2, 5)]
    public string description = "Complete this objective.";
    public bool IsComplete;
    public UnityEvent OnComplete;

    private void Start()
    {
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.RegisterObjective(this);
        }
    }

    public void Complete()
    {
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.CompleteObjective(this);
        }
        else
        {
            IsComplete = true;
            OnComplete?.Invoke();
        }
    }
}
