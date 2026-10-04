using UnityEngine;

public class TrafficLane : MonoBehaviour
{
    [Header("Lane")]
    [SerializeField] private TrafficRoute route;

    [Header("Direction")]
    [SerializeField] private Vector3 direction;

    public TrafficRoute Route => route;

    public Vector3 Direction
    {
        get
        {
            Vector3 result = direction;
            result.y = 0f;

            if (result.sqrMagnitude < 0.01f)
                return Vector3.forward;

            return result.normalized;
        }
    }

    public Vector3 StartPosition
    {
        get
        {
            if (route == null || route.PointCount == 0)
                return transform.position;

            return route.GetPoint(0).position;
        }
    }

    public Vector3 EndPosition
    {
        get
        {
            if (route == null || route.PointCount == 0)
                return transform.position;

            return route.GetPoint(route.PointCount - 1).position;
        }
    }
}