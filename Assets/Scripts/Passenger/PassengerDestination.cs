using UnityEngine;

public class PassengerDestination : MonoBehaviour
{
    [SerializeField] private TaxiJobManager jobManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        jobManager.PassengerDelivered();
    }
}