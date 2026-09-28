using UnityEngine;

public class PassengerPickup : MonoBehaviour
{
    [SerializeField] private TaxiJobManager jobManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        jobManager.PassengerPickedUp();
    }
}