using UnityEngine;

public class TaxiWallet : MonoBehaviour
{
    private int credits;

    public int Credits => credits;

    public void AddCredits(int amount)
    {
        if (amount <= 0)
            return;

        credits += amount;

        Debug.Log($"Earned {amount} credits. Total credits: {credits}");
    }
}