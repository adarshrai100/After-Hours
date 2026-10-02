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

    [Header("Run")]
    [SerializeField] private RunManager runManager;

    [Header("Job Settings")]
    [SerializeField] private float jobTimeLimit = 30f;
    [SerializeField] private int baseReward = 100;

    [Header("References")]
    [SerializeField] private TaxiWallet wallet;

    [Header("Passenger")]
    [SerializeField] private PassengerController passengerPrefab;
    private PassengerController currentPassenger;

    private readonly List<TaxiJobPoint> pickupPoints = new();
    private readonly List<TaxiJobPoint> destinationPoints = new();

    private JobState currentState = JobState.WaitingForPassenger;
    private float remainingTime;

    private TaxiJobPoint currentPickup;
    private TaxiJobPoint currentDestination;

    public JobState CurrentState => currentState;
    public float RemainingTime => remainingTime;

    private TaxiJobPoint previousPickup;
    private TaxiJobPoint previousDestination;
    private float currentJobDistance;
    private float currentJobTimeLimit;
    private int currentJobReward;

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

            if (runManager != null)
            {
                runManager.EndRun();
            }
        }
    }

    private void StartNextJob()
    {
        if (runManager != null && !runManager.IsPlaying)
            return;


        TaxiJobPoint newPickup;
        TaxiJobPoint newDestination;

        do
        {
            newPickup = pickupPoints[Random.Range(0, pickupPoints.Count)];
            newDestination =
                destinationPoints[Random.Range(0, destinationPoints.Count)];
        }
        while (
            pickupPoints.Count > 1 &&
            destinationPoints.Count > 1 &&
            newPickup == previousPickup &&
            newDestination == previousDestination
        );

        previousPickup = newPickup;
        previousDestination = newDestination;

        currentPickup = newPickup;
        currentDestination = newDestination;

        currentJobDistance = Vector3.Distance(
    currentPickup.transform.position,
    currentDestination.transform.position
);

        currentJobTimeLimit = Mathf.Clamp(
            currentJobDistance * 0.8f,
            15f,
            60f
        );

        currentJobReward = Mathf.RoundToInt(
            Mathf.Clamp(currentJobDistance * 5f, 75f, 300f)
        );

        currentState = JobState.WaitingForPassenger;

        SpawnPassenger();
        UpdateMarkers();

        Debug.Log(
            $"New job: {currentPickup.name} → {currentDestination.name}"
        );
    }

    public void PassengerPickedUp(TaxiJobPoint pickupPoint)
    {
        if (currentState != JobState.WaitingForPassenger)
            return;

        if (pickupPoint != currentPickup)
            return;

        currentState = JobState.PassengerOnBoard;
        remainingTime = jobTimeLimit;

        Debug.Log($"Job started! Time limit: {jobTimeLimit:F0} seconds.");

        UpdateMarkers();
    }

    public bool TryDeliverPassenger(TaxiJobPoint destinationPoint)
    {
        if (runManager != null && !runManager.IsPlaying)
            return false;

        if (currentState != JobState.PassengerOnBoard)
            return false;

        if (destinationPoint != currentDestination)
            return false;

        currentState = JobState.JobComplete;

        if (wallet != null)
        {
            wallet.AddCredits(currentJobReward);
        }

            Debug.Log(
        $"Passenger delivered! Reward: {currentJobReward} credits."
    );

        UpdateMarkers();

        Invoke(nameof(StartNextJob), 1f);

        return true;
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

    private void SpawnPassenger()
    {
        if (currentPassenger != null)
        {
            Destroy(currentPassenger.gameObject);
        }

        if (passengerPrefab == null)
            return;

        Vector3 spawnPosition = currentPickup.transform.position;
        spawnPosition.y = 1f;

        currentPassenger = Instantiate(
            passengerPrefab,
            spawnPosition,
            currentPickup.transform.rotation
        );
    }

    public bool TryPickupPassenger(TaxiJobPoint pickupPoint)
    {
        if (runManager != null && !runManager.IsPlaying)
            return false;

        if (currentState != JobState.WaitingForPassenger)
            return false;

        if (pickupPoint != currentPickup)
            return false;

        currentState = JobState.PassengerOnBoard;
        remainingTime = currentJobTimeLimit;

        Debug.Log(
        $"Job started! Distance: {currentJobDistance:F1} | " +
        $"Time: {currentJobTimeLimit:F0}s | " +
        $"Reward: {currentJobReward}"
    );

        UpdateMarkers();

        return true;
    }

    public void RestartJobs()
    {
        CancelInvoke(nameof(StartNextJob));

        if (currentPassenger != null)
        {
            Destroy(currentPassenger);
            currentPassenger = null;
        }

        previousPickup = null;
        previousDestination = null;

        currentState = JobState.WaitingForPassenger;
        remainingTime = 0f;

        StartNextJob();
    }
}