using UnityEngine;

public class PedestrianController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Ground Following")]
    [SerializeField] private float groundCheckHeight = 2f;
    [SerializeField] private float groundCheckDistance = 5f;
    [SerializeField] private float groundFollowSpeed = 8f;
    [SerializeField] private float feetOffset = 1f;

    private PedestrianRoute route;
    private PedestrianSpawner spawner;

    private int currentPointIndex;
    private Transform currentTarget;

    public void SetRoute(
        PedestrianRoute newRoute,
        PedestrianSpawner newSpawner)
    {
        route = newRoute;
        spawner = newSpawner;

        currentPointIndex = 1;

        if (route != null)
            currentTarget = route.GetPoint(currentPointIndex);
    }

    private void Update()
    {
        if (route == null || currentTarget == null)
            return;

        MoveTowardsTarget();
        FollowGround();
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction =
            currentTarget.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.05f)
        {
            if (currentPointIndex >= route.PointCount - 1)
            {
                if (spawner != null)
                    spawner.PedestrianDestroyed();

                Destroy(gameObject);
                return;
            }

            currentPointIndex++;

            currentTarget =
                route.GetPoint(currentPointIndex);

            return;
        }

        transform.position +=
            direction.normalized *
            moveSpeed *
            Time.deltaTime;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void FollowGround()
    {
        Vector3 rayOrigin =
            transform.position +
            Vector3.up * groundCheckHeight;

        RaycastHit[] hits = Physics.RaycastAll(
            rayOrigin,
            Vector3.down,
            groundCheckDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        float closestGroundY = float.MinValue;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            if (hit.point.y > closestGroundY)
                closestGroundY = hit.point.y;
        }

        if (closestGroundY == float.MinValue)
            return;

        float targetY =
            closestGroundY + feetOffset;

        Vector3 position = transform.position;

        position.y = Mathf.MoveTowards(
            position.y,
            targetY,
            groundFollowSpeed * Time.deltaTime
        );

        transform.position = position;
    }
}