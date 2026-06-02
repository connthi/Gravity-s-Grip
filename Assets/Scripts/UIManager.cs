using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public Text objectiveTitleText;
    public Text objectiveDescriptionText;
    public Text progressText;
    public Text torchStatusText;
    public Text hintText;
    public GameObject winPanel;
    public GameObject hudPanel;

    public void SetObjective(PuzzleObjective objective)
    {
        if (objectiveTitleText == null || objectiveDescriptionText == null)
            return;

        if (objective == null)
        {
            objectiveTitleText.text = "All puzzles complete";
            objectiveDescriptionText.text = "Return to the exit or enjoy the win screen.";
            SetHint("All puzzles complete. Find the exit or celebrate your victory.");
            return;
        }

        objectiveTitleText.text = objective.title;
        objectiveDescriptionText.text = objective.description;
        SetHint(objective.description);
    }

    public void SetProgress(int completed, int required)
    {
        if (progressText != null)
        {
            progressText.text = string.Format("Puzzles: {0}/{1}", completed, required);
        }
    }

    public void SetTorchStatus(bool isLit, float fuelPercent)
    {
        if (torchStatusText == null)
            return;

        if (!isLit)
        {
            torchStatusText.text = "Torch: Out / Unlit";
        }
        else
        {
            torchStatusText.text = string.Format("Torch: Lit ({0}%)", Mathf.RoundToInt(fuelPercent * 100f));
        }
    }

    public void SetHint(string hint)
    {
        if (hintText == null)
            return;

        hintText.text = hint;
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
