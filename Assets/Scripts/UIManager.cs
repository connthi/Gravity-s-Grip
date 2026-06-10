using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Owns all HUD and screen-overlay display.
/// Subscribes to events from PlayerController and ObjectiveTracker
/// instead of being polled every frame.
/// Inject() is called by LevelBuilder to wire up runtime-built UI elements.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // -- Inspector (set in Editor or via Inject()) ----------------------------

    [SerializeField] private GameObject hudPanel;
    [SerializeField] private Text objectiveTitleText;
    [SerializeField] private Text objectiveDescText;
    [SerializeField] private Text progressText;
    [SerializeField] private Text torchStatusText;
    [SerializeField] private Text hintText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject pausePanel;

    // -- Lifecycle ------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnGameStateChanged;

        var tracker = ObjectiveTracker.Instance;
        if (tracker != null)
        {
            tracker.OnProgressChanged   += (c, r) => SetProgress(c, r);
            tracker.OnObjectiveComplete += o      => SetObjective(tracker.NextIncomplete());
            SetProgress(tracker.CompletedCount, tracker.RequiredToWin);
            SetObjective(tracker.NextIncomplete());
        }

        var player = FindAnyObjectByType<PlayerController>();
        if (player != null)
            player.OnTorchChanged += OnTorchChanged;

        winPanel?.SetActive(false);
        pausePanel?.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
    }

    // -- Public API -----------------------------------------------------------

    public void SetObjective(PuzzleObjective obj)
    {
        if (obj == null)
        {
            SetText(objectiveTitleText, "All objectives complete!");
            SetText(objectiveDescText,  "Find the exit.");
            return;
        }
        SetText(objectiveTitleText, obj.Title);
        SetText(objectiveDescText,  obj.Description);
    }

    public void SetProgress(int completed, int required)
        => SetText(progressText, $"Puzzles: {completed}/{required}");

    public void ShowHint(string hint)
        => SetText(hintText, hint);

    public void ShowWinScreen()
    {
        hudPanel?.SetActive(false);
        winPanel?.SetActive(true);
    }

    public void ShowPauseScreen(bool show)
        => pausePanel?.SetActive(show);

    /// Called by LevelBuilder to wire up runtime-created UI elements.
    public void Inject(
        GameObject hud,
        Text objTitle, Text objDesc,
        Text progress, Text torchStatus, Text hint,
        GameObject win, GameObject pause)
    {
        hudPanel           = hud;
        objectiveTitleText = objTitle;
        objectiveDescText  = objDesc;
        progressText       = progress;
        torchStatusText    = torchStatus;
        hintText           = hint;
        winPanel           = win;
        pausePanel         = pause;
    }

    // -- Event Handlers -------------------------------------------------------

    private void OnGameStateChanged(GameManager.GameState state)
    {
        ShowPauseScreen(state == GameManager.GameState.Paused);
        if (state == GameManager.GameState.Won) ShowWinScreen();
    }

    private void OnTorchChanged(TorchPickup torch)
    {
        if (torch == null) { SetText(torchStatusText, "Torch: none"); return; }

        torch.OnLitChanged  += lit => UpdateTorchUI(lit, torch.FuelPercent);
        torch.OnFuelChanged += pct => UpdateTorchUI(torch.IsLit, pct);
        UpdateTorchUI(torch.IsLit, torch.FuelPercent);
    }

    // -- Helpers --------------------------------------------------------------

    private void UpdateTorchUI(bool lit, float pct)
        => SetText(torchStatusText, lit
            ? $"Torch: Lit ({Mathf.RoundToInt(pct * 100f)}%)"
            : "Torch: Unlit");

    private static void SetText(Text t, string s)
    {
        if (t != null) t.text = s;
    }
}