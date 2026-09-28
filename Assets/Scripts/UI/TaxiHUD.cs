using TMPro;
using UnityEngine;

public class TaxiHUD : MonoBehaviour
{
    [SerializeField] private TaxiJobManager jobManager;
    [SerializeField] private TMP_Text timerText;

    private void Update()
    {
        if (jobManager.CurrentState == TaxiJobManager.JobState.PassengerOnBoard)
        {
            timerText.text = $"TIME: {jobManager.RemainingTime:F1}";
        }
        else
        {
            timerText.text = "TIME: --";
        }
    }
}