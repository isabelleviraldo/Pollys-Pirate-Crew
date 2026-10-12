using TMPro;
using UnityEngine;

public class GoldDisplay : MonoBehaviour
{
    public GoldManager goldManager;
    public TMP_Text goldText;

    void Start()
    {
        if (goldManager == null)
        {
            goldManager =
                FindObjectOfType<GoldManager>();
        }

        UpdateGoldText();
    }

    void Update()
    {
        UpdateGoldText();
    }

    void UpdateGoldText()
    {
        if (goldManager == null ||
            goldText == null)
        {
            return;
        }

        goldText.text =
            goldManager.currentGold.ToString();
    }
}