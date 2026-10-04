using UnityEngine;

public class IntersectionTurnController : MonoBehaviour
{
    [Header("Intersection")]
    [SerializeField] private TrafficIntersection intersection;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Turn")]
    [SerializeField] private float turnStartDistance = 5f;
    [SerializeField] private float controlDistance = 4f;

    [Header("Prototype Lanes")]
    [SerializeField] private TrafficLane incomingLane;
    [SerializeField] private TrafficLane outgoingLane;

    private Vector3 p0;
    private Vector3 p1;
    private Vector3 p2;
    private Vector3 p3;

    private float progress;
    private bool turning;
    private bool turnComplete;

    private void Update()
    {
        if (intersection == null)
            return;

        if (turning)
        {
            FollowTurn();
        }
        else if (turnComplete)
        {
            ContinueNorthbound();
        }
        else
        {
            ApproachIntersection();
        }
    }

    private void ApproachIntersection()
    {
        Vector3 center = intersection.GetIntersectionCenter();

        // Eastbound.
        Vector3 direction = Vector3.right;

        float distance =
            Vector3.Distance(transform.position, center);

        if (distance <= turnStartDistance)
        {
            BeginTurn();
            return;
        }

        transform.position +=
            direction * moveSpeed * Time.deltaTime;

        transform.rotation =
            Quaternion.LookRotation(direction);
    }

    private void BeginTurn()
    {
        if (incomingLane == null || outgoingLane == null)
            return;

        Vector3 calculatedStart;
        Vector3 calculatedControl1;
        Vector3 calculatedControl2;
        Vector3 calculatedEnd;

        intersection.GetTurnPoints(
            incomingLane,
            outgoingLane,
            out calculatedStart,
            out calculatedControl1,
            out calculatedControl2,
            out calculatedEnd
        );

        // Start exactly where the car currently is.
        p0 = transform.position;

        // Keep the calculated outgoing geometry.
        p1 = calculatedControl1;
        p2 = calculatedControl2;
        p3 = calculatedEnd;

        // Preserve the car's current height.
        p0.y = transform.position.y;
        p1.y = transform.position.y;
        p2.y = transform.position.y;
        p3.y = transform.position.y;

        progress = 0f;
        turning = true;
    }

    private void FollowTurn()
    {
        float curveLength =
            Vector3.Distance(p0, p1) +
            Vector3.Distance(p1, p2) +
            Vector3.Distance(p2, p3);

        progress +=
            (moveSpeed * Time.deltaTime) /
            Mathf.Max(curveLength, 0.01f);

        progress = Mathf.Clamp01(progress);

        Vector3 position =
            CubicBezier(
                progress,
                p0,
                p1,
                p2,
                p3
            );

        Vector3 tangent =
            CubicBezierDerivative(
                progress,
                p0,
                p1,
                p2,
                p3
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

        if (progress >= 1f)
        {
            turning = false;
            turnComplete = true;
        }
    }

    private Vector3 CubicBezier(
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

    private Vector3 CubicBezierDerivative(
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

    private void ContinueNorthbound()
    {
        Vector3 direction = Vector3.forward;

        transform.position +=
            direction * moveSpeed * Time.deltaTime;

        transform.rotation =
            Quaternion.LookRotation(direction);
    }

    private TrafficLane FindNorthboundLane()
    {
        TrafficLane[] lanes =
            FindObjectsByType<TrafficLane>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (TrafficLane lane in lanes)
        {
            if (lane.Direction == Vector3.forward)
                return lane;
        }

        return null;
    }
}