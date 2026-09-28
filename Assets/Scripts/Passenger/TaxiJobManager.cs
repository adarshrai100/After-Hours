using UnityEngine;

public class TaxiJobManager : MonoBehaviour
{
    public enum JobState
    {
        WaitingForPassenger,
        PassengerOnBoard,
        JobComplete,
        JobFailed
    }

    [Header("Job Settings")]
    [SerializeField] private float jobTimeLimit = 30f;

    private JobState currentState = JobState.WaitingForPassenger;
    private float remainingTime;

    public JobState CurrentState => currentState;
    public float RemainingTime => remainingTime;

    private void Update()
    {
        if (currentState != JobState.PassengerOnBoard)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            currentState = JobState.JobFailed;

            Debug.Log("Job failed! Time ran out.");
        }
    }

    public void PassengerPickedUp()
    {
        if (currentState != JobState.WaitingForPassenger)
            return;

        currentState = JobState.PassengerOnBoard;
        remainingTime = jobTimeLimit;

        Debug.Log($"Job started! Time limit: {jobTimeLimit:F0} seconds.");
    }

    public void PassengerDelivered()
    {
        if (currentState != JobState.PassengerOnBoard)
            return;

        currentState = JobState.JobComplete;

        Debug.Log("Passenger delivered! Job complete.");
    }
}