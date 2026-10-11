using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;

    [Header("Spawn Locations")]
    public Transform spawnLeft;
    public Transform spawnRight;

    [Header("Landing Locations")]
    public Transform landingLeft;
    public Transform landingRight;

    [Header("Spawn Timing")]
    public float minimumCooldown = 3f;
    public float maximumCooldown = 7f;
    public float initialDelay = 1f;

    [Header("Spawn Limit")]
    public int maxEnemiesPerGame = 10;

    [Header("Shared Boat Ledge")]
    public Transform leftLedgeSnapPoint;
    public float leftLedgeX = -2f;
    public float ledgeResetDistance = 1f;

    private int enemiesSpawned;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (enemiesSpawned < maxEnemiesPerGame)
        {
            float cooldown = Random.Range(
                minimumCooldown,
                maximumCooldown
            );

            yield return new WaitForSeconds(cooldown);

            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning(
                "EnemySpawner needs an enemy prefab."
            );

            return;
        }

        bool spawnFromLeft =
            Random.value < 0.5f;

        Transform spawnPoint =
            spawnFromLeft
                ? spawnLeft
                : spawnRight;

        Transform landingPoint =
            spawnFromLeft
                ? landingLeft
                : landingRight;

        if (spawnPoint == null ||
            landingPoint == null)
        {
            Debug.LogWarning(
                "EnemySpawner is missing a spawn or landing point."
            );

            return;
        }

        GameObject enemyObject =
            Instantiate(
                enemyPrefab,
                spawnPoint.position,
                Quaternion.identity
            );

        EnemyController enemy =
            enemyObject.GetComponent<EnemyController>();

        walking_script enemyWalker =
            enemyObject.GetComponent<walking_script>();
        
        if (enemyWalker != null)
        {
            enemyWalker.SetLedgeSettings(
                leftLedgeSnapPoint,
                leftLedgeX,
                ledgeResetDistance
            );
        }

        if (enemy == null)
        {
            Debug.LogError(
                "The enemy prefab needs EnemyController."
            );

            Destroy(enemyObject);
            return;
        }

        /*
         * Your artwork faces left by default.
         *
         * Coming from the left:
         * Enemy moves right, so direction is 1.
         *
         * Coming from the right:
         * Enemy moves left, so direction is -1.
         */
        int startingDirection =
            spawnFromLeft ? 1 : -1;

        enemy.Initialize(
            landingPoint,
            startingDirection
        );

        enemiesSpawned++;

        Debug.Log(
            "Enemy spawned from " +
            (spawnFromLeft ? "left." : "right.")
        );
    }
}