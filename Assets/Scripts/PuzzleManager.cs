using UnityEngine;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }
    public List<PuzzleObjective> puzzles = new List<PuzzleObjective>();
    public UIManager uiManager;
    public GameObject winPanel;
    public int requiredPuzzlesToWin = 3;
    public string allCompleteHint = "All puzzles complete. Find the exit.";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        UpdateUI();
    }

    public void RegisterObjective(PuzzleObjective objective)
    {
        if (objective != null && !puzzles.Contains(objective))
        {
            puzzles.Add(objective);
            UpdateUI();
        }
    }

    public void CompleteObjective(PuzzleObjective objective)
    {
        if (objective == null || objective.IsComplete)
        {
            return;
        }

        objective.IsComplete = true;
        objective.OnComplete?.Invoke();
        UpdateUI();
        CheckWinCondition();
    }

    public void UpdateUI()
    {
        if (uiManager == null)
        {
            return;
        }

        PuzzleObjective nextObjective = GetNextIncompleteObjective();
        uiManager.SetObjective(nextObjective);
        uiManager.SetProgress(CountCompletedObjectives(), requiredPuzzlesToWin);

        if (nextObjective == null)
        {
            uiManager.SetHint(allCompleteHint);
        }
    }

    public PuzzleObjective GetNextIncompleteObjective()
    {
        foreach (PuzzleObjective objective in puzzles)
        {
            if (!objective.IsComplete)
            {
                return objective;
            }
        }

        return null;
    }

    public int CountCompletedObjectives()
    {
        int count = 0;

        foreach (PuzzleObjective objective in puzzles)
        {
            if (objective.IsComplete)
            {
                count++;
            }
        }

        return count;
    }

    private void CheckWinCondition()
    {
        if (CountCompletedObjectives() >= requiredPuzzlesToWin)
        {
            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }

            uiManager?.ShowWinScreen();
        }
    }
}
