using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public Text objectiveTitleText;
    public Text objectiveDescriptionText;
    public Text progressText;
    public GameObject winPanel;
    public GameObject hudPanel;

    public void SetObjective(PuzzleObjective objective)
    {
        if (objective == null)
        {
            objectiveTitleText.text = "All puzzles complete";
            objectiveDescriptionText.text = "Return to the exit or enjoy the win screen.";
            return;
        }

        objectiveTitleText.text = objective.title;
        objectiveDescriptionText.text = objective.description;
    }

    public void SetProgress(int completed, int required)
    {
        if (progressText != null)
        {
            progressText.text = $"Puzzles: {completed}/{required}";
        }
    }

    public void ShowWinScreen()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
