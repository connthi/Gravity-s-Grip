using UnityEngine;

public class PuzzleExitTrigger : MonoBehaviour
{
    public PuzzleManager manager;

    private void Start()
    {
        if (manager == null)
            manager = PuzzleManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (manager == null)
            return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
            return;

        if (manager.CountCompletedObjectives() >= manager.requiredPuzzlesToWin)
        {
            if (manager.uiManager != null)
                manager.uiManager.ShowWinScreen();
        }
        else
        {
            if (manager.uiManager != null)
                manager.uiManager.SetHint("You need to complete more puzzles before exiting.");
        }
    }
}
