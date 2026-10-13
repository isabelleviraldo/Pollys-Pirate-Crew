using TMPro;
using UnityEngine;

public class GoldDisplay : MonoBehaviour
{
    public TMP_Text goldText;

    void Update()
    {
        if (goldText == null ||
            GameFlowManager.Instance == null)
        {
            return;
        }

        goldText.text =
            GameFlowManager.Instance.totalGold.ToString();
    }
}