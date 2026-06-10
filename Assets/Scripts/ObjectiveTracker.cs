using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks puzzle objectives and notifies GameManager when enough are complete.
/// Objectives self-register via Awake; nothing else needs to manage the list.
/// </summary>
public class ObjectiveTracker : MonoBehaviour
{
    public static ObjectiveTracker Instance { get; private set; }

    // Fired once when the singleton is ready, so late-awaking objectives can register.
    public static event System.Action<ObjectiveTracker> OnInstanceReady;

    [Tooltip("How many objectives must be complete to win.")]
    [SerializeField] private int requiredToWin = 2;

    [SerializeField] private UIManager uiManager;

    private readonly List<PuzzleObjective> _objectives = new();
    private int _completedCount;

    public event System.Action<int, int> OnProgressChanged; // (completed, required)
    public event System.Action<PuzzleObjective> OnObjectiveComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        OnInstanceReady?.Invoke(this);
    }

    private void Start()
    {
        if (uiManager == null)
            uiManager = FindAnyObjectByType<UIManager>();

        RefreshUI();
    }

    // -- Registration ---------------------------------------------------------

    public void Register(PuzzleObjective obj)
    {
        if (!_objectives.Contains(obj))
            _objectives.Add(obj);
    }

    // -- Completion -----------------------------------------------------------

    public void NotifyComplete(PuzzleObjective obj)
    {
        if (obj.IsComplete) return;

        obj.IsComplete = true;
        _completedCount++;

        OnObjectiveComplete?.Invoke(obj);
        OnProgressChanged?.Invoke(_completedCount, requiredToWin);

        RefreshUI();

        if (_completedCount >= requiredToWin)
            GameManager.Instance?.TriggerWin();
    }

    // -- Queries --------------------------------------------------------------

    public PuzzleObjective NextIncomplete()
    {
        foreach (var o in _objectives)
            if (!o.IsComplete) return o;
        return null;
    }

    public int CompletedCount => _completedCount;
    public int RequiredToWin  => requiredToWin;

    public void SetRequiredToWin(int n) => requiredToWin = n;

    // -- Private --------------------------------------------------------------

    private void RefreshUI()
    {
        if (uiManager == null) return;
        uiManager.SetObjective(NextIncomplete());
        uiManager.SetProgress(_completedCount, requiredToWin);
    }
}