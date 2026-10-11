using UnityEngine;

public class MouseSelectionManager2D : MonoBehaviour
{
    [Header("Mouse Detection")]
    public Camera mainCamera;
    public LayerMask selectableLayers;

    private HoverSelectable2D currentHoveredObject;
    private HoverSelectable2D selectedCharacterHighlight;

    private RepairCharacter selectedCharacter;
    private CrewCombatant selectedCrew;

    void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        if (mainCamera == null)
        {
            return;
        }
    
        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(
                Input.mousePosition
            );
    
        Vector2 mousePoint =
            new Vector2(
                mouseWorldPosition.x,
                mouseWorldPosition.y
            );
    
        Collider2D[] hitColliders =
            Physics2D.OverlapPointAll(
                mousePoint,
                selectableLayers
            );
    
        Collider2D hoverCollider =
            FindHoverPriorityCollider(hitColliders);
    
        RepairCharacter hoveredCharacter = null;
        EnemyController hoveredEnemy = null;
        DamageRepairPoint hoveredDamage = null;
    
        if (hoverCollider != null)
        {
            hoveredCharacter =
                hoverCollider.GetComponentInParent<
                    RepairCharacter
                >();
    
            hoveredEnemy =
                hoverCollider.GetComponentInParent<
                    EnemyController
                >();
    
            hoveredDamage =
                hoverCollider.GetComponentInParent<
                    DamageRepairPoint
                >();
        }
    
        UpdateHoverOutline(
            hoveredCharacter,
            hoveredEnemy,
            hoveredDamage
        );
    
        if (Input.GetMouseButtonDown(0))
        {
            Collider2D clickCollider =
                FindClickPriorityCollider(hitColliders);
    
            RepairCharacter clickedCharacter = null;
            EnemyController clickedEnemy = null;
            DamageRepairPoint clickedDamage = null;
    
            if (clickCollider != null)
            {
                clickedCharacter =
                    clickCollider.GetComponentInParent<
                        RepairCharacter
                    >();
    
                clickedEnemy =
                    clickCollider.GetComponentInParent<
                        EnemyController
                    >();
    
                clickedDamage =
                    clickCollider.GetComponentInParent<
                        DamageRepairPoint
                    >();
            }
    
            HandleClick(
                clickedCharacter,
                clickedEnemy,
                clickedDamage
            );
        }
    }

    void UpdateHoverOutline(
        RepairCharacter hoveredCharacter,
        EnemyController hoveredEnemy,
        DamageRepairPoint hoveredDamage
    )
    {
        HoverSelectable2D nextHoveredObject = null;

        // No character selected:
        // only characters may highlight.
        if (selectedCharacter == null)
        {
            if (hoveredCharacter != null &&
                hoveredCharacter.CanBeSelected)
            {
                nextHoveredObject =
                    hoveredCharacter.GetComponentInParent<
                        HoverSelectable2D
                    >();
            }
        }
        // Character selected:
        // only enemies and damage may highlight.
        else
        {
            if (hoveredEnemy != null &&
                hoveredEnemy.CanBeSelected)
            {
                nextHoveredObject =
                    hoveredEnemy.GetComponentInParent<
                        HoverSelectable2D
                    >();
            }
            else if (hoveredDamage != null &&
                     hoveredDamage.CanBeSelected)
            {
                nextHoveredObject =
                    hoveredDamage.GetComponentInParent<
                        HoverSelectable2D
                    >();
            }
        }

        if (nextHoveredObject == currentHoveredObject)
        {
            return;
        }

        if (currentHoveredObject != null &&
            currentHoveredObject != selectedCharacterHighlight)
        {
            currentHoveredObject.SetHovered(false);
        }

        currentHoveredObject = nextHoveredObject;

        if (currentHoveredObject != null &&
            currentHoveredObject != selectedCharacterHighlight)
        {
            currentHoveredObject.SetHovered(true);
        }
    }

    void HandleClick(
        RepairCharacter clickedCharacter,
        EnemyController clickedEnemy,
        DamageRepairPoint clickedDamage
    )
    {
        // First selection must always be a character.
        if (selectedCharacter == null)
        {
            if (clickedCharacter != null &&
                clickedCharacter.CanBeSelected)
            {
                SelectCharacter(clickedCharacter);
            }

            return;
        }

        // Clicking the selected character again deselects it.
        if (clickedCharacter == selectedCharacter)
        {
            ClearSelection();

            Debug.Log("Character deselected.");

            return;
        }

        // Ignore other characters while one is selected.
        if (clickedCharacter != null)
        {
            return;
        }

        // Select an enemy as the secondary target.
        if (clickedEnemy != null &&
            clickedEnemy.CanBeSelected)
        {
            if (selectedCrew == null)
            {
                Debug.LogWarning(
                    "The selected character needs a CrewCombatant script."
                );

                return;
            }

            bool engaged =
                clickedEnemy.EngageWithCrew(selectedCrew);

            if (engaged)
            {
                Debug.Log(
                    "Enemy selected for combat: " +
                    clickedEnemy.name
                );

                ClearSelection();
            }

            return;
        }

        // Select a damage point as the secondary target.
        if (clickedDamage != null &&
            clickedDamage.CanBeSelected)
        {
            Debug.Log(
                "Damage selected for repair: " +
                clickedDamage.name
            );

            selectedCharacter.BeginRepair(clickedDamage);

            ClearSelection();
        }
    }

    void SelectCharacter(RepairCharacter character)
    {
        ClearSelection();

        selectedCharacter = character;

        selectedCrew =
            character.GetComponent<CrewCombatant>();

        selectedCharacterHighlight =
            character.GetComponentInParent<
                HoverSelectable2D
            >();

        if (selectedCharacterHighlight != null)
        {
            selectedCharacterHighlight.SetHovered(true);
        }

        Debug.Log(
            "Selected character: " +
            character.name
        );
    }

    void ClearSelection()
    {
        if (currentHoveredObject != null &&
            currentHoveredObject != selectedCharacterHighlight)
        {
            currentHoveredObject.SetHovered(false);
        }

        if (selectedCharacterHighlight != null)
        {
            selectedCharacterHighlight.SetHovered(false);
        }

        currentHoveredObject = null;
        selectedCharacterHighlight = null;
        selectedCharacter = null;
        selectedCrew = null;
    }

    Collider2D FindHoverPriorityCollider(
        Collider2D[] hitColliders
    )
    {
        // Before a character is selected,
        // characters always have priority.
        if (selectedCharacter == null)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                RepairCharacter character =
                    hitColliders[i].GetComponentInParent<
                        RepairCharacter
                    >();

                if (character != null &&
                    character.CanBeSelected)
                {
                    return hitColliders[i];
                }
            }

            return null;
        }

        // After selecting a character,
        // prioritize enemies and damage points.
        for (int i = 0; i < hitColliders.Length; i++)
        {
            EnemyController enemy =
                hitColliders[i].GetComponentInParent<
                    EnemyController
                >();

            DamageRepairPoint damage =
                hitColliders[i].GetComponentInParent<
                    DamageRepairPoint
                >();

            if (enemy != null ||
                damage != null)
            {
                return hitColliders[i];
            }
        }

        return null;
    }

    Collider2D FindClickPriorityCollider(
    Collider2D[] hitColliders
)
{
    // If a character is already selected,
    // check that character first so clicking it
    // can deselect it.
    if (selectedCharacter != null)
    {
        for (int i = 0; i < hitColliders.Length; i++)
        {
            RepairCharacter character =
                hitColliders[i].GetComponentInParent<
                    RepairCharacter
                >();

            if (character == selectedCharacter)
            {
                return hitColliders[i];
            }
        }
    }

    // If no character is selected,
    // characters have priority.
    if (selectedCharacter == null)
    {
        for (int i = 0; i < hitColliders.Length; i++)
        {
            RepairCharacter character =
                hitColliders[i].GetComponentInParent<
                    RepairCharacter
                >();

            if (character != null &&
                character.CanBeSelected)
            {
                return hitColliders[i];
            }
        }
    }

    // After a character is selected,
    // search for an enemy or damage point.
    for (int i = 0; i < hitColliders.Length; i++)
        {
            EnemyController enemy =
                hitColliders[i].GetComponentInParent<
                    EnemyController
                >();

            DamageRepairPoint damage =
                hitColliders[i].GetComponentInParent<
                    DamageRepairPoint
                >();

            if (enemy != null ||
                damage != null)
            {
                return hitColliders[i];
            }
        }

        return null;
    }
}