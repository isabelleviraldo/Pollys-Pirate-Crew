using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairCharacter : MonoBehaviour
{
    [Header("Movement")]
    public float movementSpeed = 2f;
    public GameObject centerWaypoint;

    [Header("Repair Travel Arc")]
    public float repairTravelTime = 1f;
    public float repairArcHeight = 1.5f;

    [Header("Walking Frames")]
    public GameObject[] walkingFrames;

    [Header("Repair Frames")]
    public GameObject[] repairFrames = new GameObject[2];
    public float repairFrameTime = 0.5f;

    [Header("Repair Speed")]
    public float repairSpeedMultiplier = 1f;

    [Header("Repair Collision")]
    public Collider2D characterBodyCollider;

    [Header("Repair Sorting")]
    public string repairSortingLayer = "Damage";
    public int repairSortingOrder = 100;

    private Renderer[] frameRenderers;
    private string[] originalSortingLayers;
    private int[] originalSortingOrders;

    private Rigidbody2D rb;
    private walking_script automaticWalking;
    private bool isBusy;

    private RigidbodyType2D originalBodyType;
    private float originalGravityScale;
    private bool originalColliderEnabled;

    public bool CanBeSelected
    {
        get { return !isBusy; }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        automaticWalking = GetComponent<walking_script>();

        SetWalkingFramesVisible(true);
        SetRepairFramesVisible(false);

        List<Renderer> renderers =
            new List<Renderer>();

        AddFrameRenderers(
            walkingFrames,
            renderers
        );

        AddFrameRenderers(
            repairFrames,
            renderers
        );

        frameRenderers =
            renderers.ToArray();

        originalSortingLayers =
            new string[frameRenderers.Length];

        originalSortingOrders =
            new int[frameRenderers.Length];

        for (int i = 0; i < frameRenderers.Length; i++)
        {
            originalSortingLayers[i] =
                frameRenderers[i].sortingLayerName;

            originalSortingOrders[i] =
                frameRenderers[i].sortingOrder;
        }
    }

    public void BeginRepair(
        DamageRepairPoint damage
    )
    {
        if (isBusy ||
            damage == null ||
            !damage.CanBeSelected ||
            damage.repairStandPoint == null)
        {
            return;
        }

        StartCoroutine(
            RepairRoutine(damage)
        );
    }

    IEnumerator RepairRoutine(
        DamageRepairPoint damage
    )
    {
        isBusy = true;
        damage.SetBeingRepaired(true);

        if (automaticWalking != null)
        {
            automaticWalking.enabled = false;
        }

        SaveAndDisablePhysics();

        SetRepairSorting(true);

        SetWalkingFramesVisible(true);
        SetRepairFramesVisible(false);

        ApplyRepairFacing(
            damage.repairFacing
        );

        Vector3 repairPosition =
            damage.repairStandPoint.transform.position;

        // Jump in an arc to the damage.
        yield return StartCoroutine(
            MoveInArc(repairPosition)
        );

        ApplyRepairFacing(
            damage.repairFacing
        );

        SetWalkingFramesVisible(false);
        SetRepairFramesVisible(true);

        float actualRepairTime =
            damage.repairTime /
            Mathf.Max(
                0.01f,
                repairSpeedMultiplier
            );

        yield return StartCoroutine(
            PlayRepairAnimation(actualRepairTime)
        );

        SetRepairFramesVisible(false);
        SetWalkingFramesVisible(true);

        damage.FinishRepair();

        // Jump back up to the deck.
        if (centerWaypoint != null)
        {
            yield return StartCoroutine(
                MoveInArc(
                    centerWaypoint.transform.position
                )
            );
        }

        RestorePhysics();

        SetRepairSorting(false);

        if (automaticWalking != null)
        {
            automaticWalking.enabled = true;
            automaticWalking.ResumeInRandomDirection();
        }

        isBusy = false;
    }

    IEnumerator MoveInArc(
        Vector3 targetPosition
    )
    {
        Vector3 startPosition =
            transform.position;

        Vector3 target =
            new Vector3(
                targetPosition.x,
                targetPosition.y,
                transform.position.z
            );

        float elapsedTime = 0f;

        while (elapsedTime < repairTravelTime)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    Mathf.Max(
                        0.01f,
                        repairTravelTime
                    )
                );

            Vector3 currentPosition =
                Vector3.Lerp(
                    startPosition,
                    target,
                    progress
                );

            // Creates an upward arc.
            // The character is descending during
            // the second half of the movement.
            float arcOffset =
                4f *
                repairArcHeight *
                progress *
                (1f - progress);

            currentPosition.y += arcOffset;

            transform.position =
                currentPosition;

            yield return null;
        }

        transform.position = target;
    }

    IEnumerator PlayRepairAnimation(
        float duration
    )
    {
        if (repairFrames == null ||
            repairFrames.Length < 2 ||
            repairFrames[0] == null ||
            repairFrames[1] == null)
        {
            yield return new WaitForSeconds(
                duration
            );

            yield break;
        }

        float elapsedTime = 0f;
        int frame = 0;

        while (elapsedTime < duration)
        {
            repairFrames[0].SetActive(
                frame == 0
            );

            repairFrames[1].SetActive(
                frame == 1
            );

            yield return new WaitForSeconds(
                repairFrameTime
            );

            elapsedTime += repairFrameTime;
            frame = 1 - frame;
        }

        repairFrames[0].SetActive(false);
        repairFrames[1].SetActive(false);
    }

    void ApplyRepairFacing(
        RepairFacingDirection direction
    )
    {
        bool facingRight =
            direction ==
            RepairFacingDirection.Right;

        ApplyFacing(
            walkingFrames,
            facingRight
        );

        ApplyFacing(
            repairFrames,
            facingRight
        );
    }

    void ApplyFacing(
        GameObject[] frames,
        bool facingRight
    )
    {
        if (frames == null)
        {
            return;
        }

        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i] == null)
            {
                continue;
            }

            Vector3 scale =
                frames[i].transform.localScale;

            // Artwork faces left by default.
            // Negative X makes it face right.
            scale.x =
                facingRight
                    ? -Mathf.Abs(scale.x)
                    : Mathf.Abs(scale.x);

            frames[i].transform.localScale =
                scale;
        }
    }

    void SaveAndDisablePhysics()
    {
        if (rb != null)
        {
            originalBodyType = rb.bodyType;
            originalGravityScale = rb.gravityScale;

            rb.bodyType =
                RigidbodyType2D.Kinematic;

            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
        }

        if (characterBodyCollider != null)
        {
            originalColliderEnabled =
                characterBodyCollider.enabled;

            characterBodyCollider.enabled = false;
        }
    }

    void RestorePhysics()
    {
        if (characterBodyCollider != null)
        {
            characterBodyCollider.enabled =
                originalColliderEnabled;
        }

        if (rb != null)
        {
            rb.bodyType = originalBodyType;
            rb.gravityScale =
                originalGravityScale;

            rb.velocity = Vector2.zero;
        }

        Physics2D.SyncTransforms();
    }

    void SetRepairSorting(
        bool repairing
    )
    {
        for (int i = 0; i < frameRenderers.Length; i++)
        {
            if (repairing)
            {
                frameRenderers[i].sortingLayerName =
                    repairSortingLayer;

                frameRenderers[i].sortingOrder =
                    repairSortingOrder;
            }
            else
            {
                frameRenderers[i].sortingLayerName =
                    originalSortingLayers[i];

                frameRenderers[i].sortingOrder =
                    originalSortingOrders[i];
            }
        }
    }

    void AddFrameRenderers(
        GameObject[] frames,
        List<Renderer> renderers
    )
    {
        if (frames == null)
        {
            return;
        }

        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i] == null)
            {
                continue;
            }

            Renderer renderer =
                frames[i].GetComponent<Renderer>();

            if (renderer != null)
            {
                renderers.Add(renderer);
            }
        }
    }

    void SetWalkingFramesVisible(
        bool visible
    )
    {
        if (walkingFrames == null)
        {
            return;
        }

        for (int i = 0; i < walkingFrames.Length; i++)
        {
            if (walkingFrames[i] != null)
            {
                walkingFrames[i].SetActive(
                    visible
                );
            }
        }
    }

    void SetRepairFramesVisible(
        bool visible
    )
    {
        if (repairFrames == null)
        {
            return;
        }

        for (int i = 0; i < repairFrames.Length; i++)
        {
            if (repairFrames[i] != null)
            {
                repairFrames[i].SetActive(
                    visible
                );
            }
        }
    }
}