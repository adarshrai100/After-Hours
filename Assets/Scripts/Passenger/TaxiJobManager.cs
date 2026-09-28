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

    [Header("Job Points")]
    [SerializeField] private PassengerPickup pickupPoint;
    [SerializeField] private PassengerDestination destinationPoint;

    private GameObject pickupMarker;
    private GameObject destinationMarker;

    private JobState currentState = JobState.WaitingForPassenger;
    private float remainingTime;

    public JobState CurrentState => currentState;
    public float RemainingTime => remainingTime;

    private void Awake()
    {
        pickupMarker = pickupPoint.transform.Find("PickupMarker")?.gameObject;
        destinationMarker = destinationPoint.transform.Find("DestinationMarker")?.gameObject;
    }

    private void Start()
    {
        UpdateMarkers();
    }

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
            UpdateMarkers();
        }
    }

    public void PassengerPickedUp()
    {
        if (currentState != JobState.WaitingForPassenger)
            return;

        currentState = JobState.PassengerOnBoard;
        remainingTime = jobTimeLimit;

        Debug.Log($"Job started! Time limit: {jobTimeLimit:F0} seconds.");

        UpdateMarkers();
    }

    public void PassengerDelivered()
    {
        if (currentState != JobState.PassengerOnBoard)
            return;

        currentState = JobState.JobComplete;

        Debug.Log("Passenger delivered! Job complete.");

        UpdateMarkers();
    }

    private void UpdateMarkers()
    {
        if (pickupMarker != null)
            pickupMarker.SetActive(currentState == JobState.WaitingForPassenger);

        if (destinationMarker != null)
            destinationMarker.SetActive(currentState == JobState.PassengerOnBoard);
    }
}