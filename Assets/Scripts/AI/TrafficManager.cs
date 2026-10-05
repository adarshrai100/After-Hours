using UnityEngine;

public class TrafficManager : MonoBehaviour
{
    [Header("Traffic")]
    [SerializeField] private GameObject trafficCarPrefab;
    [SerializeField] private TrafficRoute[] routes;

    [Header("Settings")]
    [SerializeField] private int trafficCount = 4;
    [SerializeField] private float spawnSpacing = 12f;

    [Header("Intersection")]
    [SerializeField] private TrafficIntersection centerIntersection;

    private void Start()
    {
        SpawnInitialTraffic();
    }

    private void SpawnInitialTraffic()
    {
        if (trafficCarPrefab == null ||
            routes == null ||
            routes.Length == 0)
        {
            return;
        }

        for (int i = 0; i < trafficCount; i++)
        {
            TrafficRoute route = routes[i % routes.Length];

            TrafficLane lane =
                FindLaneForRoute(route);

            SpawnCar(route, lane, i / routes.Length);
        }
    }

    public void SpawnReplacement(TrafficLane lane)
    {
        if (lane == null || lane.Route == null)
            return;

        SpawnCar(
            lane.Route,
            lane,
            0
        );
    }

    private void SpawnCar(
        TrafficRoute route,
        TrafficLane lane,
        int spacingIndex)
    {
        if (route == null || route.PointCount < 2)
            return;

        Transform spawnPoint = route.GetPoint(0);

        if (spawnPoint == null)
            return;

        Vector3 spawnPosition =
            spawnPoint.position;

        Vector3 direction =
            route.GetPoint(1).position -
            route.GetPoint(0).position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            spawnPosition +=
                direction.normalized *
                spawnSpacing *
                spacingIndex;
        }

        GameObject car = Instantiate(
            trafficCarPrefab,
            spawnPosition,
            Quaternion.identity,
            transform
        );

        TrafficController controller =
            car.GetComponent<TrafficController>();

        if (controller != null)
            controller.SetRoute(
                route,
                this,
                lane,
                centerIntersection
            );
    }

    private TrafficLane FindLaneForRoute(TrafficRoute route)
    {
        TrafficLane[] lanes =
            FindObjectsByType<TrafficLane>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (TrafficLane lane in lanes)
        {
            if (lane.Route == route)
                return lane;
        }

        return null;
    }
}