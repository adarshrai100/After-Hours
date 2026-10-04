using UnityEngine;

public class TrafficIntersection : MonoBehaviour
{
    [Header("Intersection")]
    [SerializeField] private float intersectionHalfSize = 5f;
    [SerializeField] private float laneOffset = 2f;

    [Header("Incoming Lanes")]
    [SerializeField] private TrafficLane eastboundLane;
    [SerializeField] private TrafficLane westboundLane;
    [SerializeField] private TrafficLane northboundLane;
    [SerializeField] private TrafficLane southboundLane;

    public Vector3 GetTurnPoint(
        Vector3 incomingDirection,
        Vector3 outgoingDirection)
    {
        incomingDirection.y = 0f;
        outgoingDirection.y = 0f;

        incomingDirection.Normalize();
        outgoingDirection.Normalize();

        Vector3 turnPoint =
            transform.position
            - incomingDirection * intersectionHalfSize
            + outgoingDirection * intersectionHalfSize;

        turnPoint.y = 0f;

        return turnPoint;
    }

    public Vector3 GetIntersectionCenter()
    {
        return transform.position;
    }

    public float LaneOffset => laneOffset;

    public float IntersectionHalfSize => intersectionHalfSize;

    public void GetTurnPoints(
    TrafficLane incomingLane,
    TrafficLane outgoingLane,
    out Vector3 startPoint,
    out Vector3 controlPoint1,
    out Vector3 controlPoint2,
    out Vector3 endPoint)
    {
        Vector3 center = transform.position;

        Vector3 incomingDirection =
            incomingLane.Direction;

        Vector3 outgoingDirection =
            outgoingLane.Direction;

        float halfSize = intersectionHalfSize;

        // Determine the lateral position of the incoming lane.
        Vector3 incomingOffset =
            incomingLane.StartPosition - center;

        incomingOffset -=
            Vector3.Project(incomingOffset, incomingDirection);

        // Determine the lateral position of the outgoing lane.
        Vector3 outgoingOffset =
            outgoingLane.StartPosition - center;

        outgoingOffset -=
            Vector3.Project(outgoingOffset, outgoingDirection);

        startPoint =
            center -
            incomingDirection * halfSize +
            incomingOffset;

        endPoint =
            center +
            outgoingDirection * halfSize +
            outgoingOffset;

        float controlDistance =
            halfSize - laneOffset;

        controlPoint1 =
            startPoint +
            incomingDirection * controlDistance;

        controlPoint2 =
            endPoint -
            outgoingDirection * controlDistance;

        startPoint.y = 0f;
        controlPoint1.y = 0f;
        controlPoint2.y = 0f;
        endPoint.y = 0f;
    }
}