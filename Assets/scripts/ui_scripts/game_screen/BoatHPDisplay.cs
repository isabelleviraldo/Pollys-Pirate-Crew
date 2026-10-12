using TMPro;
using UnityEngine;

public class BoatHPDisplay : MonoBehaviour
{
    public BoatDamageController boatController;

    [Header("SVG Boat Image")]
    public GameObject boatImage;

    [Header("HP Text")]
    public TMP_Text hpText;

    void Update()
    {
        if (boatController == null)
        {
            return;
        }

        int currentHP =
            boatController.CurrentBoatHealth;

        if (hpText != null)
        {
            hpText.text =
                currentHP.ToString();
        }

        if (boatImage != null)
        {
            boatImage.SetActive(
                currentHP > 0
            );
        }
    }
}