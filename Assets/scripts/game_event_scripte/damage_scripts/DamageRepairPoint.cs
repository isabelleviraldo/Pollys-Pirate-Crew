using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RepairFacingDirection
{
    Left,
    Right
}

public class DamageRepairPoint : MonoBehaviour
{
    [Header("Damage")]
    public int damageIndex;
    public GameObject damageVisual;
    public BoatDamageController boatController;

    [Header("Character Destination")]
    public GameObject repairStandPoint;

    [Header("Repair Time")]
    public float repairTime = 3f;

    [Header("Character Facing")]
    public RepairFacingDirection repairFacing =
        RepairFacingDirection.Left;

    private bool isBeingRepaired;

    public bool CanBeSelected
    {
        get
        {
            return damageVisual != null &&
                   damageVisual.activeSelf &&
                   !isBeingRepaired;
        }
    }

    public bool IsBeingRepaired
    {
        get { return isBeingRepaired; }
    }

    public void SetBeingRepaired(bool value)
    {
        isBeingRepaired = value;
    }

    public void FinishRepair()
    {
        isBeingRepaired = false;

        if (damageVisual != null)
        {
            damageVisual.SetActive(false);
        }

        if (boatController != null)
        {
            boatController.RepairDamage(damageIndex);
        }
    }
}