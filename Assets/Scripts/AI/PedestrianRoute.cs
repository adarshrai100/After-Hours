using UnityEngine;

public class PedestrianRoute : MonoBehaviour
{
    [SerializeField] private Transform[] points;

    public Transform GetNextPoint(int currentIndex)
    {
        if (points == null || points.Length == 0)
            return null;

        int nextIndex = (currentIndex + 1) % points.Length;
        return points[nextIndex];
    }

    public int PointCount => points != null ? points.Length : 0;

    public Transform GetPoint(int index)
    {
        if (points == null || index < 0 || index >= points.Length)
            return null;

        return points[index];
    }
}