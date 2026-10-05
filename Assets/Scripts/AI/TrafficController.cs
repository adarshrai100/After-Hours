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


    [SerializeField] private TrafficLane currentLane;
    private TrafficLane spawnLane;
    private TrafficIntersection intersection;
    private TrafficLane outgoingLane;

    private bool enteringIntersection;
    private bool waitingForIntersection;

    private Vector3 turnP0;
    private Vector3 turnP1;
    private Vector3 turnP2;
    private Vector3 turnP3;

    private float intersectionProgress;


    private TrafficRoute route;
    private TrafficManager trafficManager;

    private int currentPointIndex;
    private Transform currentTarget;

    private float currentSpeed;

    private TrafficIntersection.TurnDirection currentTurn;
    private bool goingStraight;

    public void SetRoute(
        TrafficRoute newRoute,
        TrafficManager newTrafficManager,
        TrafficLane newLane = null,
        TrafficIntersection newIntersection = null)
    {
        route = newRoute;
        trafficManager = newTrafficManager;
        currentLane = newLane;
        spawnLane = newLane;

        intersection = newIntersection;
        outgoingLane = null;

        currentPointIndex = 1;
        currentSpeed = moveSpeed;

        if (route != null)
            currentTarget = route.GetPoint(currentPointIndex);
    }

    private void Update()
    {
        if (route == null || currentTarget == null)
            return;

        if (enteringIntersection)
        {
            FollowIntersectionTurn();
            return;
        }

        if (goingStraight)
        {
            ContinueStraightThroughIntersection();
            return;
        }

        if (waitingForIntersection)
        {
            if (intersection == null || intersection.TryEnter(this))
            {
                waitingForIntersection = false;

                currentSpeed = moveSpeed;

                BeginIntersectionTurn();
            }
            else
            {
                currentSpeed = Mathf.MoveTowards(
                    currentSpeed,
                    0f,
                    brakingSpeed * Time.deltaTime
                );

                return;
            }

            return;
        }

        CheckIntersection();

        if (enteringIntersection || goingStraight || waitingForIntersection)
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
                if (trafficManager != null && spawnLane != null)
                    trafficManager.SpawnReplacement(spawnLane);

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

    private void CheckIntersection()
    {
        if (intersection == null || currentLane == null)
            return;

        Vector3 center = intersection.GetIntersectionCenter();

        Vector3 toIntersection = center - transform.position;
        toIntersection.y = 0f;

        float distance = toIntersection.magnitude;

        if (distance > intersection.IntersectionHalfSize + 2f)
            return;

        Vector3 direction = currentLane.Direction;

        if (Vector3.Dot(direction, toIntersection.normalized) < 0.5f)
            return;

        if (!intersection.TryEnter(this))
        {
            waitingForIntersection = true;

            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                brakingSpeed * Time.deltaTime
            );

            return;
        }

        BeginIntersectionTurn();
    }

    private void BeginIntersectionTurn()
    {
        currentTurn = intersection.GetRandomTurn();

        outgoingLane =
            intersection.GetOutgoingLane(
                currentLane,
                currentTurn
            );

        if (outgoingLane == null)
        {
            if (intersection != null)
                intersection.Exit(this);

            return;
        }

        if (currentTurn == TrafficIntersection.TurnDirection.Straight)
        {
            goingStraight = true;
            return;
        }

        intersection.GetTurnPoints(
            currentLane,
            outgoingLane,
            out turnP0,
            out turnP1,
            out turnP2,
            out turnP3
        );

        turnP0 = transform.position;

        turnP0.y = transform.position.y;
        turnP1.y = transform.position.y;
        turnP2.y = transform.position.y;
        turnP3.y = transform.position.y;

        intersectionProgress = 0f;
        enteringIntersection = true;
    }

    private void FollowIntersectionTurn()
    {
        float curveLength =
            Vector3.Distance(turnP0, turnP1) +
            Vector3.Distance(turnP1, turnP2) +
            Vector3.Distance(turnP2, turnP3);

        intersectionProgress +=
            (currentSpeed * Time.deltaTime) /
            Mathf.Max(curveLength, 0.01f);

        intersectionProgress =
            Mathf.Clamp01(intersectionProgress);

        Vector3 position =
            CalculateCubicBezier(
                intersectionProgress,
                turnP0,
                turnP1,
                turnP2,
                turnP3
            );

        Vector3 tangent =
            CalculateCubicBezierDerivative(
                intersectionProgress,
                turnP0,
                turnP1,
                turnP2,
                turnP3
            );

        tangent.y = 0f;

        if (tangent.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    tangent.normalized,
                    Vector3.up
                );
        }

        transform.position = position;

        if (intersectionProgress >= 1f)
        {
            transform.position = turnP3;

            enteringIntersection = false;

            CompleteIntersectionTurn();
        }
    }

    private Vector3 CalculateCubicBezier(
    float t,
    Vector3 a,
    Vector3 b,
    Vector3 c,
    Vector3 d)
    {
        float u = 1f - t;

        return
            u * u * u * a +
            3f * u * u * t * b +
            3f * u * t * t * c +
            t * t * t * d;
    }

    private Vector3 CalculateCubicBezierDerivative(
        float t,
        Vector3 a,
        Vector3 b,
        Vector3 c,
        Vector3 d)
    {
        float u = 1f - t;

        return
            3f * u * u * (b - a) +
            6f * u * t * (c - b) +
            3f * t * t * (d - c);
    }


    private void CompleteIntersectionTurn()
    {
        goingStraight = false;

        if (intersection != null)
            intersection.Exit(this);

        if (outgoingLane == null || outgoingLane.Route == null)
            return;

        currentLane = outgoingLane;
        route = outgoingLane.Route;
        currentPointIndex = 1;
        currentTarget = route.GetPoint(currentPointIndex);
        currentSpeed = moveSpeed;
    }

    private void ContinueStraightThroughIntersection()
    {
        Vector3 direction = currentLane.Direction;

        transform.position +=
            direction * currentSpeed * Time.deltaTime;

        transform.rotation =
            Quaternion.LookRotation(direction);

        Vector3 center =
            intersection.GetIntersectionCenter();

        float distance =
            Vector3.Distance(transform.position, center);

        if (distance >= intersection.IntersectionHalfSize + 2f)
        {
            goingStraight = false;

            if (intersection != null)
                intersection.Exit(this);
        }
    }

}