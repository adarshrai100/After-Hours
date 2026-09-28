using UnityEngine;

public class TaxiJobManager : MonoBehaviour
{
    public enum JobState
    {
        WaitingForPassenger,
        PassengerOnBoard,
        JobComplete
    }

    [Header("Job Points")]
    [SerializeField] private PassengerPickup pickupPoint;
    [SerializeField] private Collider destinationPoint;

    private JobState currentState = JobState.WaitingForPassenger;

    public JobState CurrentState => currentState;

    public void PassengerPickedUp()
    {
        if (currentState != JobState.WaitingForPassenger)
            return;

        currentState = JobState.PassengerOnBoard;
        Debug.Log("Job started! Drive to the destination.");
    }

    public void PassengerDelivered()
    {
        if (currentState != JobState.PassengerOnBoard)
            return;

        currentState = JobState.JobComplete;
        Debug.Log("Passenger delivered! Job complete.");
    }
}