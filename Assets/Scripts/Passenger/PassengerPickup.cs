using UnityEngine;

public class PassengerPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        TaxiJobManager jobManager =
            FindFirstObjectByType<TaxiJobManager>();

        if (jobManager == null)
            return;

        bool pickedUp = jobManager.TryPickupPassenger(
            GetComponent<TaxiJobPoint>()
        );

        if (!pickedUp)
            return;

        PassengerController passenger =
            FindFirstObjectByType<PassengerController>();

        if (passenger != null)
        {
            passenger.EnterTaxi();
        }
    }
}