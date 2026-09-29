using UnityEngine;

public class TaxiJobPoint : MonoBehaviour
{
    public enum PointType
    {
        Pickup,
        Destination
    }

    [SerializeField] private PointType pointType;

    private GameObject marker;

    public PointType Type => pointType;

    private void Awake()
    {
        marker = transform.Find(
            pointType == PointType.Pickup
                ? "PickupMarker"
                : "DestinationMarker"
        )?.gameObject;
    }

    public void SetMarkerVisible(bool visible)
    {
        if (marker != null)
            marker.SetActive(visible);
    }
}