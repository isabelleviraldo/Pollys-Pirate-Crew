using System.Collections;
using UnityEngine;

[System.Serializable]
public class WalkFrame
{
    public GameObject frameObject;
}

public class walking_script : MonoBehaviour
{
    [Header("Walking")]
    public float walkSpeed = 2f;
    public float leftLimit = -7f;
    public float rightLimit = 7f;
    public bool startMovingRight = true;

    [Header("External Movement Control")]
    public bool movementEnabled = true;
    public bool animationEnabled = true;

    [Header("Animation")]
    public WalkFrame[] walkFrames;
    public float secondsPerFrame = 0.5f;

    [Header("Frame Positioning")]
    public Transform framesRoot;

    [Header("Left Ledge")]
    public float leftLedgeX = -2f;
    public Transform leftLedgeSnapPoint;
    public float resetDistance = 1f;

    [Header("Smooth Ledge Movement")]
    public float ledgeMoveDuration = 0.25f;
    public float ledgeJumpHeight = 0.25f;

    private Rigidbody2D rb;
    private int direction;
    private int currentFrame;
    private float animationTimer;

    private bool hasSnappedToLeftLedge;
    private bool movingToLedge;

    private Vector3[] originalScales;
    private RigidbodyType2D originalBodyType;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            originalBodyType = rb.bodyType;
        }

        direction =
            startMovingRight
                ? 1
                : -1;

        // FramesRoot must be a child object,
        // never the character/enemy root itself.
        if (framesRoot != null &&
            framesRoot != transform)
        {
            framesRoot.localPosition = Vector3.zero;
            framesRoot.localRotation =
                Quaternion.identity;
        }

        if (walkFrames == null)
        {
            walkFrames = new WalkFrame[0];
        }

        originalScales =
            new Vector3[walkFrames.Length];

        for (int i = 0; i < walkFrames.Length; i++)
        {
            if (walkFrames[i].frameObject == null)
            {
                continue;
            }

            Transform frameTransform =
                walkFrames[i].frameObject.transform;

            // Each frame must be a child of FramesRoot.
            if (frameTransform != transform)
            {
                frameTransform.localPosition =
                    Vector3.zero;

                frameTransform.localRotation =
                    Quaternion.identity;
            }

            originalScales[i] =
                frameTransform.localScale;
        }

        if (walkFrames.Length > 0)
        {
            SetFrame(0);
        }

        ApplyFacing();
    }

    void Update()
    {
        if (!animationEnabled)
        {
            return;
        }

        if (walkFrames == null ||
            walkFrames.Length <= 1)
        {
            return;
        }

        animationTimer += Time.deltaTime;

        if (animationTimer >= secondsPerFrame)
        {
            animationTimer = 0f;

            int nextFrame =
                currentFrame + 1;

            if (nextFrame >= walkFrames.Length)
            {
                nextFrame = 0;
            }

            SetFrame(nextFrame);
        }
    }

    void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        // Completely stop movement when externally disabled.
        if (!movementEnabled)
        {
            if (!movingToLedge)
            {
                rb.velocity = Vector2.zero;
            }

            return;
        }

        // Do not apply normal movement during ledge travel.
        if (movingToLedge)
        {
            return;
        }

        Vector2 velocity =
            rb.velocity;

        velocity.x =
            direction * walkSpeed;

        rb.velocity =
            velocity;

        // Move onto the left ledge.
        if (direction < 0 &&
            !hasSnappedToLeftLedge &&
            rb.position.x <= leftLedgeX &&
            leftLedgeSnapPoint != null)
        {
            hasSnappedToLeftLedge = true;

            StartCoroutine(
                MoveSmoothlyToLedge()
            );

            return;
        }

        // Allow another ledge crossing after moving away.
        if (direction > 0 &&
            rb.position.x >
                leftLedgeX + resetDistance)
        {
            hasSnappedToLeftLedge = false;
        }

        // Reverse at boundaries.
        if (rb.position.x <= leftLimit)
        {
            SetDirection(1);
        }
        else if (rb.position.x >= rightLimit)
        {
            SetDirection(-1);
        }
    }

    IEnumerator MoveSmoothlyToLedge()
    {
        movingToLedge = true;

        rb.velocity =
            Vector2.zero;

        rb.bodyType =
            RigidbodyType2D.Kinematic;

        Vector2 startPosition =
            rb.position;

        Vector2 targetPosition =
            leftLedgeSnapPoint.position;

        float elapsedTime = 0f;

        while (elapsedTime < ledgeMoveDuration)
        {
            elapsedTime +=
                Time.fixedDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    Mathf.Max(
                        0.01f,
                        ledgeMoveDuration
                    )
                );

            float smoothProgress =
                progress *
                progress *
                (3f - 2f * progress);

            Vector2 newPosition =
                Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    smoothProgress
                );

            newPosition.y +=
                Mathf.Sin(
                    progress * Mathf.PI
                ) * ledgeJumpHeight;

            rb.MovePosition(newPosition);

            yield return new WaitForFixedUpdate();
        }

        rb.position =
            targetPosition;

        rb.bodyType =
            originalBodyType;

        rb.velocity =
            new Vector2(
                direction * walkSpeed,
                0f
            );

        movingToLedge = false;
    }

    void SetFrame(int frameIndex)
    {
        currentFrame =
            frameIndex;

        for (int i = 0; i < walkFrames.Length; i++)
        {
            if (walkFrames[i].frameObject == null)
            {
                continue;
            }

            walkFrames[i].frameObject.SetActive(
                i == currentFrame
            );
        }

        ApplyFacing();
    }

    void SetDirection(int newDirection)
    {
        if (newDirection == direction)
        {
            return;
        }

        direction =
            newDirection;

        ApplyFacing();
    }

    void ApplyFacing()
    {
        for (int i = 0; i < walkFrames.Length; i++)
        {
            if (walkFrames[i].frameObject == null)
            {
                continue;
            }

            Transform frameTransform =
                walkFrames[i].frameObject.transform;

            Vector3 scale =
                originalScales[i];

            // Artwork faces left by default.
            // Right = negative X.
            // Left = positive X.
            scale.x =
                -Mathf.Abs(scale.x) *
                direction;

            frameTransform.localScale =
                scale;
        }
    }

    public void ResumeInRandomDirection()
    {
        hasSnappedToLeftLedge =
            false;

        int randomDirection =
            Random.value < 0.5f
                ? -1
                : 1;

        SetDirection(randomDirection);
    }

    public void SetMovementEnabled(
        bool enabled
    )
    {
        movementEnabled =
            enabled;

        if (!enabled &&
            rb != null &&
            !movingToLedge)
        {
            rb.velocity =
                Vector2.zero;
        }
    }

    public void SetDirectionExternal(
        int newDirection
    )
    {
        SetDirection(
            newDirection >= 0
                ? 1
                : -1
        );
    }

    public void SetDirectionToward(
        float targetX
    )
    {
        if (Mathf.Abs(
            targetX -
            transform.position.x
        ) < 0.05f)
        {
            return;
        }

        SetDirection(
            targetX >
                transform.position.x
                ? 1
                : -1
        );
    }

    public void SetLedgeSettings(
        Transform snapPoint,
        float ledgePositionX,
        float ledgeResetDistance
    )
    {
        leftLedgeSnapPoint = snapPoint;
        leftLedgeX = ledgePositionX;
        resetDistance = ledgeResetDistance;
    }

    public void SetAnimationEnabled(
        bool enabled
    )
    {
        animationEnabled =
            enabled;
    }
}