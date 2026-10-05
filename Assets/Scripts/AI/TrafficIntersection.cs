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

    private TrafficController occupyingCar;

    public enum TurnDirection
    {
        Straight,
        Left,
        Right
    }


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

        incomingDirection.y = 0f;
        outgoingDirection.y = 0f;

        incomingDirection.Normalize();
        outgoingDirection.Normalize();

        // Find the lateral offset of the incoming lane.
        Vector3 incomingOffset =
            incomingLane.StartPosition - center;

        incomingOffset -=
            Vector3.Project(
                incomingOffset,
                incomingDirection
            );

        // Find the lateral offset of the outgoing lane.
        Vector3 outgoingOffset =
            outgoingLane.StartPosition - center;

        outgoingOffset -=
            Vector3.Project(
                outgoingOffset,
                outgoingDirection
            );

        startPoint =
            center -
            incomingDirection * intersectionHalfSize +
            incomingOffset;

        endPoint =
            center +
            outgoingDirection * intersectionHalfSize +
            outgoingOffset;

        float controlDistance =
            intersectionHalfSize - laneOffset;

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

    public TurnDirection GetRandomTurn()
    {
        float roll = Random.value;

        if (roll < 0.50f)
            return TurnDirection.Straight;

        if (roll < 0.75f)
            return TurnDirection.Left;

        return TurnDirection.Right;
    }

    public void DebugTurnDecision(TurnDirection turn)
    {
        Debug.Log(
            $"Intersection decision: {turn}"
        );
    }

    public TrafficLane GetOutgoingLane(
    TrafficLane incomingLane,
    TurnDirection turn)
    {
        if (incomingLane == null)
            return null;

        Vector3 incomingDirection =
            incomingLane.Direction;

        incomingDirection.y = 0f;
        incomingDirection.Normalize();

        // Eastbound
        if (incomingDirection == Vector3.right)
        {
            switch (turn)
            {
                case TurnDirection.Straight:
                    return incomingLane;

                case TurnDirection.Left:
                    return northboundLane;

                case TurnDirection.Right:
                    return southboundLane;
            }
        }

        // Westbound
        if (incomingDirection == Vector3.left)
        {
            switch (turn)
            {
                case TurnDirection.Straight:
                    return incomingLane;

                case TurnDirection.Left:
                    return southboundLane;

                case TurnDirection.Right:
                    return northboundLane;
            }
        }

        // Northbound
        if (incomingDirection == Vector3.forward)
        {
            switch (turn)
            {
                case TurnDirection.Straight:
                    return incomingLane;

                case TurnDirection.Left:
                    return westboundLane;

                case TurnDirection.Right:
                    return eastboundLane;
            }
        }

        // Southbound
        if (incomingDirection == Vector3.back)
        {
            switch (turn)
            {
                case TurnDirection.Straight:
                    return incomingLane;

                case TurnDirection.Left:
                    return eastboundLane;

                case TurnDirection.Right:
                    return westboundLane;
            }
        }

        return null;
    }

    public bool IsOccupiedByOther(TrafficController car)
    {
        return occupyingCar != null && occupyingCar != car;
    }

    public bool TryEnter(TrafficController car)
    {
        if (occupyingCar != null && occupyingCar != car)
            return false;

        occupyingCar = car;
        Debug.Log($"Intersection entered by {car.name}");
        return true;
    }

    public void Exit(TrafficController car)
    {
        if (occupyingCar == car)
        {
            occupyingCar = null;
            Debug.Log($"Intersection released by {car.name}");
        }
    }
}