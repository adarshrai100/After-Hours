using UnityEngine;

public class TrafficRoute : MonoBehaviour
{
    [SerializeField] private Transform[] points;

    public int PointCount => points != null ? points.Length : 0;

    public Transform GetPoint(int index)
    {
        if (points == null || index < 0 || index >= points.Length)
            return null;

        return points[index];
    }
}