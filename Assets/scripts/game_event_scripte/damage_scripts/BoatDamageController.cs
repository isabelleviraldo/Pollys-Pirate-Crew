using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BoatDamageController : MonoBehaviour
{
    [Header("Cannonball")]
    public GameObject cannonballPrefab;
    public Transform cannonballSpawnLeft;
    public Transform cannonballSpawnRight;

    public float minimumCooldown = 3f;
    public float maximumCooldown = 7f;
    public float cannonballTravelTime = 1f;
    public float decoyContinuationDuration = 1f;

    public float cannonballArcHeight = 2f;
    public float cannonballRotationDegrees = 180f;

    [Header("Damage Points 1-7")]
    public Transform[] damagePoints = new Transform[7];

    [Header("Harmless Cannonball Points")]
    public Transform[] harmlessCannonballPoints = new Transform[3];

    [Header("Damage Visuals 1-7")]
    public GameObject[] damageVisuals = new GameObject[7];

    [Header("Mast")]
    public GameObject mastObject;

    [Header("Future Boat Damage")]
    public int[] damageValues = new int[7]
    {
        0, 0, 0, 0, 0, 0, 0
    };

    public int totalBoatDamage = 0;

    [Header("Boat Health")]
    public int maximumBoatHealth = 100;
    
    public int CurrentBoatHealth
    {
        get
        {
            return Mathf.Clamp(
                maximumBoatHealth -
                totalBoatDamage,
                0,
                maximumBoatHealth
            );
        }
    }
    
    public float BoatHealthPercent
    {
        get
        {
            return (float)CurrentBoatHealth /
                   maximumBoatHealth;
        }
    }

    [Header("Future Game Over")]
    public UnityEvent onGameOver;

    private bool[] damageAlreadyApplied = new bool[7];

    private struct CannonballTarget
    {
        public Transform point;
        public int damageIndex;

        public CannonballTarget(
            Transform point,
            int damageIndex
        )
        {
            this.point = point;
            this.damageIndex = damageIndex;
        }
    }

    void Start()
    {
        // Hide all damage visuals at the beginning.
        for (int i = 0; i < damageVisuals.Length; i++)
        {
            if (damageVisuals[i] != null)
            {
                damageVisuals[i].SetActive(false);
            }
        }

        StartCoroutine(CannonballLoop());
    }

    IEnumerator CannonballLoop()
    {
        while (true)
        {
            float cooldown = Random.Range(
                minimumCooldown,
                maximumCooldown
            );

            yield return new WaitForSeconds(cooldown);

            List<CannonballTarget> availableTargets =
                GetAvailableTargets();

            if (availableTargets.Count == 0)
            {
                yield break;
            }

            int randomIndex = Random.Range(
                0,
                availableTargets.Count
            );

            CannonballTarget selectedTarget =
                availableTargets[randomIndex];

            yield return StartCoroutine(
                FireCannonballAtTarget(selectedTarget)
            );
        }
    }

    List<CannonballTarget> GetAvailableTargets()
    {
        List<CannonballTarget> targets =
            new List<CannonballTarget>();

        // Add damage points that have not been hit yet.
        for (int i = 0; i < 7; i++)
        {
            if (!damageAlreadyApplied[i] &&
                damagePoints[i] != null &&
                damageVisuals[i] != null)
            {
                targets.Add(
                    new CannonballTarget(
                        damagePoints[i],
                        i
                    )
                );
            }
        }

        // Add harmless points.
        for (int i = 0; i < harmlessCannonballPoints.Length; i++)
        {
            if (harmlessCannonballPoints[i] != null)
            {
                targets.Add(
                    new CannonballTarget(
                        harmlessCannonballPoints[i],
                        -1
                    )
                );
            }
        }

        return targets;
    }

    IEnumerator FireCannonballAtTarget(
        CannonballTarget target
    )
    {
        if (cannonballPrefab == null)
        {
            Debug.LogWarning(
                "Cannonball prefab has not been assigned."
            );
    
            yield break;
        }
    
        Transform spawnPoint;
    
        if (Random.value < 0.5f)
        {
            spawnPoint = cannonballSpawnLeft;
        }
        else
        {
            spawnPoint = cannonballSpawnRight;
        }
    
        if (spawnPoint == null)
        {
            Debug.LogWarning(
                "A cannonball spawn point has not been assigned."
            );
    
            yield break;
        }
    
        GameObject cannonball = Instantiate(
            cannonballPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.PlayCannonballFlight(
                cannonball.transform.position
            );
        }
    
        Vector3 startPosition =
            cannonball.transform.position;
    
        Vector3 targetPosition =
            target.point.position;
    
        // Decoys continue moving after reaching their point.
        float totalDuration;
    
        if (target.damageIndex < 0)
        {
            totalDuration =
                cannonballTravelTime +
                decoyContinuationDuration;
        }
        else
        {
            totalDuration =
                cannonballTravelTime;
        }
    
        float elapsedTime = 0f;
    
        float rotationDirection =
            Random.value < 0.5f ? -1f : 1f;
    
        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
    
            // This can go above 1 for decoy points.
            float arcProgress =
                elapsedTime / cannonballTravelTime;
    
            float rotationProgress =
                Mathf.Clamp01(
                    elapsedTime / totalDuration
                );
    
            // Unclamped is important because the cannonball
            // must continue past the decoy point.
            Vector3 currentPosition =
                Vector3.LerpUnclamped(
                    startPosition,
                    targetPosition,
                    arcProgress
                );
    
            // Continue the same parabolic arc past the decoy.
            float arcOffset =
                4f *
                cannonballArcHeight *
                arcProgress *
                (1f - arcProgress);
    
            currentPosition.y += arcOffset;
    
            cannonball.transform.position =
                currentPosition;
    
            // Complete approximately half a rotation
            // across the entire movement.
            float currentRotation =
                rotationDirection *
                cannonballRotationDegrees *
                rotationProgress;
    
            cannonball.transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    currentRotation
                );
    
            yield return null;
        }
    
        Destroy(cannonball);
        
        if (target.damageIndex >= 0)
        {
            if (GameAudioManager.Instance != null)
            {
                GameAudioManager.Instance.PlayCannonballHit(
                    targetPosition
                );
            }
        
            ShowDamage(target.damageIndex);
        }
    }

    void ShowDamage(int damageIndex)
    {
        if (damageAlreadyApplied[damageIndex])
        {
            return;
        }

        damageAlreadyApplied[damageIndex] = true;

        // Show the existing damage visual.
        damageVisuals[damageIndex].SetActive(true);

        // Damage 7 removes the mast.
        if (damageIndex == 6)
        {
            if (mastObject != null)
            {
                mastObject.SetActive(false);
            }
        }

        // Store the damage value for future health logic.
        if (damageIndex < damageValues.Length)
        {
            totalBoatDamage += damageValues[damageIndex];
        }

        // Add game-over behavior later.
        //
        // if (totalBoatDamage >= 100)
        // {
        //     onGameOver.Invoke();
        // }
    }

    public void RepairDamage(int damageIndex)
    {
        if (damageIndex < 0 ||
            damageIndex >= damageAlreadyApplied.Length)
        {
            return;
        }
    
        if (!damageAlreadyApplied[damageIndex])
        {
            return;
        }
    
        damageAlreadyApplied[damageIndex] = false;
    
        if (damageIndex < damageValues.Length)
        {
            totalBoatDamage -= damageValues[damageIndex];
            totalBoatDamage =
                Mathf.Max(0, totalBoatDamage);
        }
    
        // If damage 7 was repaired, restore the mast.
        if (damageIndex == 6 &&
            mastObject != null)
        {
            mastObject.SetActive(true);
        }
    }
}