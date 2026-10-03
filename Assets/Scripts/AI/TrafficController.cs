using UnityEngine;

public class TrafficController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 6f;

    [Header("Traffic Spacing")]
    [SerializeField] private float detectionDistance = 8f;
    [SerializeField] private float minimumGap = 4f;
    [SerializeField] private float brakingSpeed = 12f;
    [SerializeField] private float accelerationSpeed = 5f;

    private TrafficRoute route;
    private TrafficManager trafficManager;

    private int currentPointIndex;
    private Transform currentTarget;

    private float currentSpeed;

    public void SetRoute(
        TrafficRoute newRoute,
        TrafficManager newTrafficManager)
    {
        route = newRoute;
        trafficManager = newTrafficManager;

        currentPointIndex = 1;
        currentSpeed = moveSpeed;

        if (route != null)
            currentTarget = route.GetPoint(currentPointIndex);
    }

    private void Update()
    {
        if (route == null || currentTarget == null)
            return;

        UpdateSpeed();
        MoveAlongRoute();
    }

    private void UpdateSpeed()
    {
        bool carAhead = false;

        Ray ray = new Ray(
            transform.position + Vector3.up * 0.5f,
            transform.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            detectionDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Traffic"))
            {
                float distance = hit.distance;

                if (distance < minimumGap)
                    carAhead = true;
            }
        }

        if (carAhead)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                brakingSpeed * Time.deltaTime
            );
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                moveSpeed,
                accelerationSpeed * Time.deltaTime
            );
        }
    }

    private void MoveAlongRoute()
    {
        Vector3 direction =
            currentTarget.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.25f)
        {
            if (currentPointIndex >= route.PointCount - 1)
            {
                if (trafficManager != null)
                    trafficManager.SpawnReplacement(route);

                Destroy(gameObject);
                return;
            }

            currentPointIndex++;

            currentTarget =
                route.GetPoint(currentPointIndex);

            return;
        }

        Vector3 movementDirection =
            direction.normalized;

        transform.position +=
            movementDirection *
            currentSpeed *
            Time.deltaTime;

        Quaternion targetRotation =
            Quaternion.LookRotation(movementDirection);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}