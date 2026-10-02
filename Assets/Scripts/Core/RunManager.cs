using UnityEngine;

public class RunManager : MonoBehaviour
{
    public enum RunState
    {
        Playing,
        GameOver
    }

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Gameplay")]
    [SerializeField] private TaxiJobManager jobManager;

    private RunState currentState = RunState.Playing;

    public RunState CurrentState => currentState;
    public bool IsPlaying => currentState == RunState.Playing;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void EndRun()
    {
        if (currentState == RunState.GameOver)
            return;

        currentState = RunState.GameOver;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log("RUN OVER");
    }

    public void RestartRun()
    {
        currentState = RunState.Playing;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (jobManager != null)
        {
            jobManager.RestartJobs();
        }

        Debug.Log("RUN RESTARTED");
    }
}