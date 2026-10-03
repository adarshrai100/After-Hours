using System.Collections;
using UnityEngine;

public class PedestrianCollision : MonoBehaviour
{
    [SerializeField] private float knockbackDistance = 1.2f;
    [SerializeField] private float reactionDuration = 0.25f;
    [SerializeField] private float disappearDelay = 0.5f;

    private bool hasBeenHit;

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenHit || !other.CompareTag("Player"))
            return;

        hasBeenHit = true;

        TaxiController taxiController =
            other.GetComponent<TaxiController>();

        if (taxiController != null)
            taxiController.StopImmediately();

        PedestrianSpawner spawner =
            FindFirstObjectByType<PedestrianSpawner>();

        if (spawner != null)
            spawner.PedestrianDestroyed();

        StartCoroutine(HitReaction(other.transform));
    }

    private IEnumerator HitReaction(Transform taxi)
    {
        Vector3 startPosition = transform.position;

        Vector3 direction =
            transform.position - taxi.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            direction = transform.forward;

        direction.Normalize();

        Vector3 targetPosition =
            startPosition + direction * knockbackDistance;

        float elapsed = 0f;

        while (elapsed < reactionDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(elapsed / reactionDuration);

            transform.position =
                Vector3.Lerp(startPosition, targetPosition, t);

            transform.Rotate(
                Vector3.right,
                360f * Time.deltaTime
            );

            yield return null;
        }

        yield return new WaitForSeconds(disappearDelay);

        Destroy(gameObject);
    }
}