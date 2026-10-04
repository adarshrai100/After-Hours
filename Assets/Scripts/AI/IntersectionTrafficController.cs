using UnityEngine;

public class IntersectionTrafficController : MonoBehaviour
{
    [Header("Intersection")]
    [SerializeField] private TrafficIntersection intersection;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 6f;

    [Header("Test")]
    [SerializeField] private Vector3 incomingDirection = Vector3.right;
    [SerializeField] private Vector3 outgoingDirection = Vector3.forward;

    private Vector3 turnPoint;
    private bool turning;

    private void Start()
    {
        if (intersection == null)
            return;

        turnPoint = intersection.GetTurnPoint(
            incomingDirection,
            outgoingDirection
        );
    }

    private void Update()
    {
        if (intersection == null)
            return;

        Vector3 target =
            turning
                ? intersection.GetIntersectionCenter()
                : turnPoint;

        MoveTowards(target);
    }

    private void MoveTowards(Vector3 target)
    {
        Vector3 direction =
            target - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.25f)
        {
            if (!turning)
            {
                turning = true;
                return;
            }

            return;
        }

        Vector3 movementDirection =
            direction.normalized;

        transform.position +=
            movementDirection *
            moveSpeed *
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