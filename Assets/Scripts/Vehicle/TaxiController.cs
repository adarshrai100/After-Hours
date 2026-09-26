using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class TaxiController : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float maxForwardSpeed = 18f;
    [SerializeField] private float maxReverseSpeed = 7f;
    [SerializeField] private float naturalDeceleration = 4f;

    [Header("Steering")]
    [SerializeField] private float steeringSpeed = 75f;

    [Header("Collision")]
    [SerializeField] private float collisionSkin = 0.05f;

    private Rigidbody rb;

    private float throttleInput;
    private float steeringInput;
    private float currentSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        UpdateSpeed();
        RotateTaxi();
        MoveTaxi();
    }

    private void ReadInput()
    {
        throttleInput = 0f;
        steeringInput = 0f;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            throttleInput = 1f;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            throttleInput = -1f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            steeringInput = -1f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            steeringInput = 1f;
    }

    private void UpdateSpeed()
    {
        if (throttleInput > 0f)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                maxForwardSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }
        else if (throttleInput < 0f)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                -maxReverseSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                naturalDeceleration * Time.fixedDeltaTime
            );
        }
    }

    private void MoveTaxi()
    {
        if (Mathf.Abs(currentSpeed) < 0.01f)
            return;

        float movementDistance =
            Mathf.Abs(currentSpeed) *
            Time.fixedDeltaTime;

        Vector3 direction =
            currentSpeed >= 0f
                ? transform.forward
                : -transform.forward;

        if (rb.SweepTest(
                direction,
                out RaycastHit hit,
                movementDistance + collisionSkin,
                QueryTriggerInteraction.Ignore))
        {
            currentSpeed = 0f;

            float safeDistance =
                Mathf.Max(0f, hit.distance - collisionSkin);

            rb.MovePosition(
                rb.position + direction * safeDistance
            );

            return;
        }

        rb.MovePosition(
            rb.position + direction * movementDistance
        );
    }

    private void RotateTaxi()
    {
        if (Mathf.Abs(currentSpeed) < 0.05f)
            return;

        float speedFactor = Mathf.Clamp01(
            Mathf.Abs(currentSpeed) / maxForwardSpeed
        );

        float direction = currentSpeed >= 0f ? 1f : -1f;

        float rotationAmount =
            steeringInput *
            steeringSpeed *
            speedFactor *
            direction *
            Time.fixedDeltaTime;

        rb.MoveRotation(
            rb.rotation *
            Quaternion.Euler(0f, rotationAmount, 0f)
        );
    }
}