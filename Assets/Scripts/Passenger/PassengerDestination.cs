using UnityEngine;

public class PassengerDestination : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        TaxiJobManager jobManager =
            FindFirstObjectByType<TaxiJobManager>();

        if (jobManager == null)
            return;

        jobManager.TryDeliverPassenger(
            GetComponent<TaxiJobPoint>()
        );
    }
}