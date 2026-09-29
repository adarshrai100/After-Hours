using System.Collections.Generic;
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
    [SerializeField] private int baseReward = 100;

    [Header("References")]
    [SerializeField] private TaxiWallet wallet;

    private readonly List<TaxiJobPoint> pickupPoints = new();
    private readonly List<TaxiJobPoint> destinationPoints = new();

    private JobState currentState = JobState.WaitingForPassenger;
    private float remainingTime;

    private TaxiJobPoint currentPickup;
    private TaxiJobPoint currentDestination;

    public JobState CurrentState => currentState;
    public float RemainingTime => remainingTime;

    private void Awake()
    {
        TaxiJobPoint[] points = FindObjectsByType<TaxiJobPoint>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (TaxiJobPoint point in points)
        {
            if (point.Type == TaxiJobPoint.PointType.Pickup)
                pickupPoints.Add(point);
            else
                destinationPoints.Add(point);
        }
    }

    private void Start()
    {
        if (pickupPoints.Count == 0 || destinationPoints.Count == 0)
        {
            Debug.LogError("TaxiJobManager requires at least one pickup and one destination point.");
            return;
        }

        StartNextJob();
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

    private void StartNextJob()
    {
        currentPickup = pickupPoints[Random.Range(0, pickupPoints.Count)];

        do
        {
            currentDestination =
                destinationPoints[Random.Range(0, destinationPoints.Count)];
        }
        while (currentDestination == currentPickup &&
               destinationPoints.Count > 1);

        currentState = JobState.WaitingForPassenger;

        UpdateMarkers();

        Debug.Log("New passenger is waiting.");
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

        if (wallet != null)
        {
            wallet.AddCredits(baseReward);
        }

        Debug.Log($"Passenger delivered! Reward: {baseReward} credits.");

        UpdateMarkers();

        Invoke(nameof(StartNextJob), 1f);
    }

    private void UpdateMarkers()
    {
        foreach (TaxiJobPoint point in pickupPoints)
        {
            bool active = point == currentPickup &&
                          currentState == JobState.WaitingForPassenger;

            point.SetMarkerVisible(active);
        }

        foreach (TaxiJobPoint point in destinationPoints)
        {
            bool active = point == currentDestination &&
                          currentState == JobState.PassengerOnBoard;

            point.SetMarkerVisible(active);
        }
    }
}