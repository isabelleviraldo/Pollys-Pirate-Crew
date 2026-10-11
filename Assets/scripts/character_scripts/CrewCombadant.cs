using System.Collections;
using UnityEngine;

public enum CombatJoinResult
{
    Rejected,
    Fighting
}

public class CrewCombatant : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Combat Damage")]
    public float damagePerSecond = 5f;

    [Header("Fighting Movement")]
    public float movementSpeed = 3f;
    public float combatStandDistance = 1.2f;

    [Header("Fighting Animation")]
    public GameObject[] combatFrames;
    public float combatFrameTime = 0.25f;

    [Header("Combat Jump")]
    public float combatTravelTime = 1f;
    public float combatArcHeight = 1f;

    [Header("Boat Floor")]
    public LayerMask boatFloorLayer;
    public float floorRaycastStartHeight = 5f;
    public float floorRaycastDistance = 20f;
    public Collider2D characterBodyCollider;
    public float extraFloorOffset = 0.02f;

    [Header("Death Animation")]
    public GameObject[] deathFrames;
    public float deathFrameTime = 0.25f;

    private float currentHealth;

    private walking_script walker;
    private Rigidbody2D rb;

    private EnemyController activeEnemy;

    private Coroutine combatMovementRoutine;
    private Coroutine combatAnimationRoutine;

    private Vector3[] originalCombatScales;
    private RigidbodyType2D originalBodyType;

    private bool isFighting;
    private bool combatPositionReached;

    private bool isDead;
    private Coroutine deathAnimationRoutine;

    public bool IsDead
    {
        get { return isDead; }
    }

    public float CurrentHealth
    {
        get { return currentHealth; }
    }

    public bool IsBusy
    {
        get { return activeEnemy != null; }
    }

    void Awake()
    {
        currentHealth = maxHealth;

        walker =
            GetComponent<walking_script>();

        rb =
            GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            originalBodyType =
                rb.bodyType;
        }

        if (combatFrames == null)
        {
            combatFrames =
                new GameObject[0];
        }

        originalCombatScales =
            new Vector3[combatFrames.Length];

        for (int i = 0;
             i < combatFrames.Length;
             i++)
        {
            if (combatFrames[i] != null)
            {
                originalCombatScales[i] =
                    combatFrames[i]
                        .transform
                        .localScale;

                combatFrames[i].SetActive(false);
            }
        }

        // Add the death-frame setup here.
        if (deathFrames == null)
        {
            deathFrames =
                new GameObject[0];
        }

        for (int i = 0;
             i < deathFrames.Length;
             i++)
        {
            if (deathFrames[i] != null)
            {
                deathFrames[i].SetActive(false);
            }
        }
    }

    void Update()
    {
        if (isDead)
        {
            return;
        }

        if (activeEnemy == null &&
            isFighting)
        {
            EndFighting();
        }

        ProcessActiveCombat();
    }

    public CombatJoinResult TryJoinCombat(
        EnemyController enemy
    )
    {
        if (enemy == null ||
            activeEnemy != null ||
            !enemy.CanBeSelected)
        {
            return CombatJoinResult.Rejected;
        }

        activeEnemy =
            enemy;

        enemy.EnterCombat(this);

        BeginFighting(enemy);

        return CombatJoinResult.Fighting;
    }

    void BeginFighting(
        EnemyController enemy
    )
    {
        isFighting = true;
        combatPositionReached = false;

        if (walker != null)
        {
            walker.SetMovementEnabled(false);
            walker.SetAnimationEnabled(false);
        }

        if (rb != null)
        {
            rb.velocity =
                Vector2.zero;

            rb.bodyType =
                RigidbodyType2D.Kinematic;
        }

        HideWalkingFrames();
        ApplyCombatFacing(enemy);

        if (combatMovementRoutine != null)
        {
            StopCoroutine(
                combatMovementRoutine
            );
        }

        combatMovementRoutine =
            StartCoroutine(
                MoveNextToEnemy(enemy)
            );

        if (combatAnimationRoutine != null)
        {
            StopCoroutine(
                combatAnimationRoutine
            );
        }

        combatAnimationRoutine =
            StartCoroutine(
                PlayCombatAnimation()
            );
    }

    IEnumerator MoveNextToEnemy(
        EnemyController enemy
    )
    {
        if (enemy == null)
        {
            yield break;
        }

        Vector3 enemyPosition =
            enemy.transform.position;

        float directionFromEnemy =
            transform.position.x <= enemyPosition.x
                ? -1f
                : 1f;

        float targetX =
            enemyPosition.x +
            directionFromEnemy *
            combatStandDistance;

        Vector2 rayOrigin =
            new Vector2(
                targetX,
                Mathf.Max(
                    transform.position.y,
                    enemyPosition.y
                ) + floorRaycastStartHeight
            );

        RaycastHit2D floorHit =
            Physics2D.Raycast(
                rayOrigin,
                Vector2.down,
                floorRaycastDistance,
                boatFloorLayer
            );

        float targetY;

        if (floorHit.collider != null)
        {
            float floorOffset =
                extraFloorOffset;

            if (characterBodyCollider != null)
            {
                floorOffset +=
                    characterBodyCollider.bounds.extents.y;
            }

            targetY =
                floorHit.point.y +
                floorOffset;
        }
        else
        {
            Debug.LogWarning(
                "No boat floor found below combat position."
            );

            targetY =
                transform.position.y;
        }

        Vector3 startPosition =
            transform.position;

        Vector3 targetPosition =
            new Vector3(
                targetX,
                targetY,
                transform.position.z
            );

        // Face the enemy before jumping.
        ApplyCombatFacing(enemy);

        float elapsedTime = 0f;

        while (elapsedTime < combatTravelTime)
        {
            elapsedTime +=
                Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    Mathf.Max(
                        0.01f,
                        combatTravelTime
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
                combatArcHeight *
                progress *
                (1f - progress);

            currentPosition.y +=
                arcOffset;

            transform.position =
                currentPosition;

            yield return null;
        }

        transform.position =
            targetPosition;

        combatPositionReached = true;

        ApplyCombatFacing(enemy);
    }

    IEnumerator PlayCombatAnimation()
    {
        if (combatFrames == null ||
            combatFrames.Length == 0)
        {
            yield break;
        }

        int currentFrame = 0;

        while (isFighting &&
               activeEnemy != null)
        {
            for (int i = 0;
                 i < combatFrames.Length;
                 i++)
            {
                if (combatFrames[i] != null)
                {
                    combatFrames[i].SetActive(
                        i == currentFrame
                    );
                }
            }

            yield return new WaitForSeconds(
                combatFrameTime
            );

            currentFrame++;

            if (currentFrame >= combatFrames.Length)
            {
                currentFrame = 0;
            }
        }
    }

    void ProcessActiveCombat()
    {
        if (activeEnemy == null ||
            !combatPositionReached)
        {
            return;
        }

        float damage =
            damagePerSecond *
            Time.deltaTime;

        activeEnemy.ReceiveDamage(
            damage,
            this
        );

        if (activeEnemy.damagePerSecondToCrew > 0f)
        {
            ReceiveDamage(
                activeEnemy.damagePerSecondToCrew *
                Time.deltaTime
            );
        }
    }

    public void LeaveCombat(
        EnemyController enemy
    )
    {
        if (enemy != activeEnemy)
        {
            return;
        }

        activeEnemy =
            null;

        EndFighting();
    }

    void EndFighting()
    {
        isFighting = false;
        combatPositionReached = false;

        if (combatMovementRoutine != null)
        {
            StopCoroutine(
                combatMovementRoutine
            );

            combatMovementRoutine =
                null;
        }

        if (combatAnimationRoutine != null)
        {
            StopCoroutine(
                combatAnimationRoutine
            );

            combatAnimationRoutine =
                null;
        }

        HideCombatFrames();
        ShowWalkingFrames();

        if (rb != null)
        {
            rb.bodyType =
                originalBodyType;

            rb.velocity =
                Vector2.zero;
        }

        if (walker != null)
        {
            walker.SetAnimationEnabled(true);
            walker.SetMovementEnabled(true);
            walker.ResumeInRandomDirection();
        }
    }

    void ApplyCombatFacing(
        EnemyController enemy
    )
    {
        if (enemy == null)
        {
            return;
        }

        bool facingRight =
            enemy.transform.position.x >
            transform.position.x;

        for (int i = 0;
             i < combatFrames.Length;
             i++)
        {
            if (combatFrames[i] == null)
            {
                continue;
            }

            Vector3 scale =
                originalCombatScales[i];

            // Artwork faces left by default.
            scale.x =
                facingRight
                    ? -Mathf.Abs(scale.x)
                    : Mathf.Abs(scale.x);

            combatFrames[i].transform.localScale =
                scale;
        }
    }

    void HideWalkingFrames()
    {
        if (walker == null ||
            walker.walkFrames == null)
        {
            return;
        }

        for (int i = 0;
             i < walker.walkFrames.Length;
             i++)
        {
            if (walker.walkFrames[i].frameObject != null)
            {
                walker.walkFrames[i]
                    .frameObject
                    .SetActive(false);
            }
        }
    }

    void ShowWalkingFrames()
    {
        if (walker == null ||
            walker.walkFrames == null)
        {
            return;
        }

        for (int i = 0;
             i < walker.walkFrames.Length;
             i++)
        {
            if (walker.walkFrames[i].frameObject != null)
            {
                walker.walkFrames[i]
                    .frameObject
                    .SetActive(i == 0);
            }
        }
    }

    void HideCombatFrames()
    {
        for (int i = 0;
             i < combatFrames.Length;
             i++)
        {
            if (combatFrames[i] != null)
            {
                combatFrames[i].SetActive(false);
            }
        }
    }

    public bool IsEngagedWith(
        EnemyController enemy
    )
    {
        return activeEnemy == enemy;
    }

    public void ReceivePassingDamage(
        float amount
    )
    {
        if (isDead)
        {
            return;
        }
    
        ReceiveDamage(amount);
    }

    public void ReceiveDamage(
        float amount
    )
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= amount;

        currentHealth =
            Mathf.Max(
                0f,
                currentHealth
            );

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        isFighting = false;

        EnemyController enemyThatWasFighting =
            activeEnemy;

        activeEnemy = null;

        if (enemyThatWasFighting != null)
        {
            enemyThatWasFighting.EndCombatBecauseCrewDied(
                this
            );
        }

        combatPositionReached = false;

        if (combatMovementRoutine != null)
        {
            StopCoroutine(
                combatMovementRoutine
            );
        }

        if (combatAnimationRoutine != null)
        {
            StopCoroutine(
                combatAnimationRoutine
            );
        }

        if (walker != null)
        {
            walker.SetMovementEnabled(false);
            walker.SetAnimationEnabled(false);
        }

        if (rb != null)
        {
            rb.velocity =
                Vector2.zero;

            rb.bodyType =
                RigidbodyType2D.Kinematic;
        }

        HideWalkingFrames();
        HideCombatFrames();

        // Prevent further selection or physical interaction.
        Collider2D[] colliders =
            GetComponentsInChildren<Collider2D>(
                true
            );

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        deathAnimationRoutine =
            StartCoroutine(
                PlayDeathAnimation()
            );
    }

    IEnumerator PlayDeathAnimation()
    {
        if (deathFrames == null ||
            deathFrames.Length == 0)
        {
            gameObject.SetActive(false);
            yield break;
        }

        for (int i = 0;
             i < deathFrames.Length;
             i++)
        {
            for (int j = 0;
                 j < deathFrames.Length;
                 j++)
            {
                if (deathFrames[j] != null)
                {
                    deathFrames[j].SetActive(
                        j == i
                    );
                }
            }

            yield return new WaitForSeconds(
                deathFrameTime
            );
        }

        // Hide the final death image.
        for (int i = 0;
             i < deathFrames.Length;
             i++)
        {
            if (deathFrames[i] != null)
            {
                deathFrames[i].SetActive(false);
            }
        }

        // Remove the character after the animation.
        gameObject.SetActive(false);
    }
}