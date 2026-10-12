using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    [Header("Gold")]
    public int currentGold = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddGold(int amount)
    {
        currentGold += amount;

        Debug.Log(
            "Gold gained: " +
            amount +
            " | Total gold: " +
            currentGold
        );
    }
}