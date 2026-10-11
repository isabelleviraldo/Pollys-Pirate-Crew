using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private enum EnemyState
    {
        Entering,
        Walking,
        Fighting,
        Dead
    }

    [Header("Health")]
    public float maxHealth = 30f;

    [Header("Combat")]
    public float passingDamagePerSecond = 2f;
    public float passingDamageRange = 0.7f;
    public float passingDamageVerticalRange = 1f;

    [Header("Entry Jump")]
    public float entryTravelTime = 1f;
    public float entryArcHeight = 1.5f;

    [Header("Combat Damage")]
    public float damagePerSecondToCrew = 3f;

    [Header("Reward")]
    public int goldReward = 100;

    private float currentHealth;

    private EnemyState state =
        EnemyState.Entering;

    private walking_script walker;
    private CrewCombatant combatCrew;

    private Rigidbody2D rb;
    private RigidbodyType2D originalBodyType;

    private bool combatReleased;

    public bool CanBeSelected
    {
        get
        {
            return state != EnemyState.Entering &&
                   state != EnemyState.Dead &&
                   combatCrew == null;
        }
    }
    
    public bool EngageWithCrew(CrewCombatant crew)
    {
        if (!CanBeSelected || crew == null)
        {
            return false;
        }
    
        CombatJoinResult result =
            crew.TryJoinCombat(this);
    
        return result != CombatJoinResult.Rejected;
    }

    public float CurrentHealth
    {
        get { return currentHealth; }
    }

    void Awake()
    {
        currentHealth = maxHealth;

        walker =
            GetComponent<walking_script>();

        rb =
            GetComponent<Rigidbody2D>();

        if (walker == null)
        {
            Debug.LogError(
                "Enemy needs walking_script attached."
            );
        }

        if (rb != null)
        {
            originalBodyType = rb.bodyType;
        }
    }

    public void Initialize(
        Transform landingPoint,
        int startingDirection
    )
    {
        if (walker != null)
        {
            walker.SetDirectionExternal(
                startingDirection
            );

            walker.SetMovementEnabled(false);
        }

        StartCoroutine(
            EnterShip(landingPoint)
        );
    }

    void Update()
    {
        if (state == EnemyState.Entering ||
            state == EnemyState.Dead)
        {
            return;
        }

        ApplyPassingDamage();

        // Only the selected enemy stops.
        if (state == EnemyState.Fighting)
        {
            return;
        }

        // Restore normal walking for every non-engaged enemy.
        if (state == EnemyState.Walking)
        {
            if (rb != null &&
                rb.bodyType != originalBodyType)
            {
                rb.bodyType =
                    originalBodyType;

                rb.velocity =
                    Vector2.zero;
            }

            if (walker != null)
            {
                walker.SetMovementEnabled(true);
            }
        }
    }

    IEnumerator EnterShip(
        Transform landingPoint
    )
    {
        if (landingPoint == null)
        {
            state = EnemyState.Walking;

            if (walker != null)
            {
                walker.SetMovementEnabled(true);
            }

            yield break;
        }

        state = EnemyState.Entering;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType =
                RigidbodyType2D.Kinematic;
        }

        Vector3 startPosition =
            transform.position;

        Vector3 targetPosition =
            new Vector3(
                landingPoint.position.x,
                landingPoint.position.y,
                transform.position.z
            );

        float elapsedTime = 0f;

        while (elapsedTime < entryTravelTime)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    Mathf.Max(
                        0.01f,
                        entryTravelTime
                    )
                );

            Vector3 currentPosition =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    progress
                );

            float arcOffset =
                4f *
                entryArcHeight *
                progress *
                (1f - progress);

            currentPosition.y += arcOffset;

            transform.position =
                currentPosition;

            yield return null;
        }

        transform.position =
            targetPosition;

        if (rb != null)
        {
            rb.bodyType =
                originalBodyType;
        }

        state = EnemyState.Walking;

        if (walker != null)
        {
            walker.SetMovementEnabled(true);
        }
    }

    void ApplyPassingDamage()
    {
        CrewCombatant[] crewMembers =
            FindObjectsOfType<CrewCombatant>();

        for (int i = 0; i < crewMembers.Length; i++)
        {
            CrewCombatant crew =
                crewMembers[i];

            if (crew == null ||
                crew.IsBusy)
            {
                continue;
            }

            float horizontalDistance =
                Mathf.Abs(
                    crew.transform.position.x -
                    transform.position.x
                );

            float verticalDistance =
                Mathf.Abs(
                    crew.transform.position.y -
                    transform.position.y
                );

            if (horizontalDistance <=
                    passingDamageRange &&
                verticalDistance <=
                    passingDamageVerticalRange)
            {
                crew.ReceivePassingDamage(
                    passingDamagePerSecond *
                    Time.deltaTime
                );
            }
        }
    }

    public void EnterCombat(
        CrewCombatant crew
    )
    {
        combatCrew = crew;
        state = EnemyState.Fighting;

        if (crew != null &&
            walker != null)
        {
            int facingDirection =
                crew.transform.position.x >
                transform.position.x
                    ? 1
                    : -1;

            walker.SetDirectionExternal(
                facingDirection
            );

            walker.SetMovementEnabled(false);
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType =
                RigidbodyType2D.Kinematic;
        }
    }

    public void ReceiveDamage(
        float amount,
        CrewCombatant source
    )
    {
        if (state == EnemyState.Dead)
        {
            return;
        }

        // Only the crew member currently fighting
        // this enemy can damage it.
        if (source == null ||
            source != combatCrew)
        {
            return;
        }

        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (state == EnemyState.Dead)
        {
            return;
        }
    
        state = EnemyState.Dead;
    
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddGold(
                goldReward
            );
        }
    
        if (walker != null)
        {
            walker.SetMovementEnabled(false);
        }
    
        ReleaseCombat();
    
        Destroy(gameObject);
    }

    void ReleaseCombat()
    {
        if (combatReleased)
        {
            return;
        }

        combatReleased = true;

        if (combatCrew != null)
        {
            CrewCombatant crew =
                combatCrew;

            combatCrew = null;

            crew.LeaveCombat(this);
        }
    }

    void OnDestroy()
    {
        ReleaseCombat();
    }

    public void EndCombatBecauseCrewDied(
        CrewCombatant crew
    )
    {
        if (combatCrew != crew)
        {
            return;
        }
    
        combatCrew = null;
        state = EnemyState.Walking;
    
        if (rb != null)
        {
            rb.bodyType =
                originalBodyType;
    
            rb.velocity =
                Vector2.zero;
    
            rb.angularVelocity = 0f;
        }
    
        if (walker != null)
        {
            walker.SetMovementEnabled(true);
        }
    }
}