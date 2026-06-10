using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Owns overall game state: pausing, winning, restarting.
/// All other systems talk to GameManager rather than each other.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, Won }

    public GameState State { get; private set; } = GameState.Playing;

    // Raised when state changes so UI/audio can react without polling.
    public event System.Action<GameState> OnStateChanged;

    [Header("References")]
    [SerializeField] private UIManager uiManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (uiManager == null)
            uiManager = FindAnyObjectByType<UIManager>();

        SetState(GameState.Playing);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void TriggerWin()
    {
        if (State == GameState.Won) return;
        SetState(GameState.Won);
        uiManager?.ShowWinScreen();
    }

    public void TogglePause()
    {
        if (State == GameState.Won) return;
        SetState(State == GameState.Paused ? GameState.Playing : GameState.Paused);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void SetState(GameState next)
    {
        State = next;
        Time.timeScale = (next == GameState.Paused) ? 0f : 1f;
        Cursor.lockState = (next == GameState.Playing) ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = (next != GameState.Playing);
        OnStateChanged?.Invoke(next);
    }
}