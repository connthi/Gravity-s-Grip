using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attach to any puzzle object. Registers itself with ObjectiveTracker on Awake.
/// Call Complete() from switches, triggers, or other game logic.
/// SetContent() is called by LevelBuilder at construction time.
/// </summary>
public class PuzzleObjective : MonoBehaviour
{
    [field: SerializeField] public string Title       { get; private set; } = "Objective";
    [field: SerializeField, TextArea(2, 4)]
    public string Description { get; private set; } = "Complete this objective.";

    public UnityEvent onComplete;

    public bool IsComplete { get; internal set; }

    private void Awake()
    {
        if (ObjectiveTracker.Instance != null)
            ObjectiveTracker.Instance.Register(this);
        else
            ObjectiveTracker.OnInstanceReady += tracker => tracker.Register(this);
    }

    private void OnDestroy()
    {
        ObjectiveTracker.OnInstanceReady -= tracker => tracker.Register(this);
    }

    public void Complete()
    {
        if (IsComplete) return;
        ObjectiveTracker.Instance?.NotifyComplete(this);
        onComplete?.Invoke();
    }

    /// Called by LevelBuilder after AddComponent.
    public void SetContent(string title, string description)
    {
        Title       = title;
        Description = description;
    }
}