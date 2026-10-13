using System.Collections;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance;

    [Header("Cannon Sounds")]
    public AudioClip cannonballFlightSound;
    public AudioClip cannonballHitSound;

    [Header("Combat Sound")]
    public AudioClip combatSound;
    public float combatSoundDuration = 3f;

    [Header("Volume")]
    [Range(0f, 1f)]
    public float cannonballVolume = 1f;

    [Range(0f, 1f)]
    public float combatVolume = 1f;

    private AudioSource combatAudioSource;
    private Coroutine combatStopRoutine;

    void Awake()
    {
        Instance = this;

        combatAudioSource =
            gameObject.AddComponent<AudioSource>();

        combatAudioSource.playOnAwake = false;
        combatAudioSource.loop = true;
        combatAudioSource.spatialBlend = 0f;
    }

    public void PlayCannonballFlight(Vector3 position)
    {
        if (cannonballFlightSound == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(
            cannonballFlightSound,
            position,
            cannonballVolume
        );
    }

    public void PlayCannonballHit(Vector3 position)
    {
        if (cannonballHitSound == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(
            cannonballHitSound,
            position,
            cannonballVolume
        );
    }

    public void PlayCombatStart()
    {
        if (combatSound == null)
        {
            return;
        }

        if (combatStopRoutine != null)
        {
            StopCoroutine(combatStopRoutine);
        }

        combatAudioSource.clip = combatSound;
        combatAudioSource.volume = combatVolume;
        combatAudioSource.Play();

        combatStopRoutine =
            StartCoroutine(StopCombatSound());
    }

    IEnumerator StopCombatSound()
    {
        yield return new WaitForSecondsRealtime(
            combatSoundDuration
        );

        combatAudioSource.Stop();
    }
}