using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject  hudPanel;
    [SerializeField] private TMP_Text    objectiveTitleText;
    [SerializeField] private TMP_Text    objectiveDescText;
    [SerializeField] private TMP_Text    progressText;
    [SerializeField] private TMP_Text    torchStatusText;
    [SerializeField] private TMP_Text    hintText;
    [SerializeField] private GameObject  winPanel;
    [SerializeField] private GameObject  pausePanel;

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
        => SetText(progressText, $"Puzzles: {completed} / {required}");

    public void ShowHint(string hint) => SetText(hintText, hint);

    public void ShowWinScreen()
    {
        hudPanel?.SetActive(false);
        winPanel?.SetActive(true);
    }

    public void ShowPauseScreen(bool show) => pausePanel?.SetActive(show);

    public void Inject(
        GameObject hud,
        TMP_Text objTitle, TMP_Text objDesc,
        TMP_Text progress, TMP_Text torchStatus, TMP_Text hint,
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

    private void UpdateTorchUI(bool lit, float pct)
        => SetText(torchStatusText, lit
            ? $"Torch: Lit  {Mathf.RoundToInt(pct * 100f)}%"
            : "Torch: Unlit");

    private static void SetText(TMP_Text t, string s)
    {
        if (t != null) t.text = s;
    }
}
