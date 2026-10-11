using TMPro;
using UnityEngine;

public class CharacterHPDisplay : MonoBehaviour
{
    public CrewCombatant character;
    public GameObject heartImage;
    public TMP_Text hpText;

    void Update()
    {
        if (character == null)
        {
            return;
        }

        int currentHP =
            Mathf.CeilToInt(
                character.CurrentHealth
            );

        if (hpText != null)
        {
            hpText.text =
                currentHP.ToString();
        }

        if (heartImage != null)
        {
            heartImage.SetActive(
                currentHP > 0
            );
        }
    }
}