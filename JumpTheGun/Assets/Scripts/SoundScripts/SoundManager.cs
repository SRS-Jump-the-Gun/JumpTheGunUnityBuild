using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//when adding sound, make sure to add corresponding category here and add audio file to SoundManager game object
//To call sound effect in script call it like so:  SoundManager.PlaySound(SoundType.{Sound name here});
public enum SoundType
{
    SHOTGUN,
    BACKGROUND_MUSIC,
    REVOLVER_RELOAD,   // Plays once per bullet inserted into the revolver
    REVOLVER_CHAMBER,  // Plays once when the revolver reload is fully complete
    SHOTGUN_RELOAD      // Plays once per shell inserted into the shotgun
}

[ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;

    private static SoundManager instance;

    private AudioSource audioSource;  // For gunshots/SFX
    private AudioSource musicSource;  // For looping BGM

    // Tracks which SoundTypes are currently playing.
    // Each PlaySound call adds the type and starts a coroutine that removes it
    // once the clip finishes, making IsPlayingSound reliable with PlayOneShot.
    private HashSet<SoundType> playingSounds = new HashSet<SoundType>();

    private void Awake()
    {
        instance = this;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        musicSource.volume = PlayerPrefs.GetFloat("Volume", 0.5f);
    }

    public static void SetMusicVolume(float volume)
    {
        instance.musicSource.volume = volume;
    }


    public static void PlaySound(SoundType sound, float volume = 1)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        instance.audioSource.PlayOneShot(randomClip, volume);
        instance.StartCoroutine(instance.TrackSound(sound, randomClip.length));
    }

    public static void PlayMusic(SoundType sound, float volume = 0.5f)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip clip = clips[0];

        instance.musicSource.clip = clip;
        instance.musicSource.volume = volume;
        instance.musicSource.loop = true;
        instance.musicSource.Play();
    }

    // Returns true while a PlayOneShot for this SoundType is still playing
    public static bool IsPlayingSound(SoundType sound)
    {
        return instance.playingSounds.Contains(sound);
    }

    // Registers the sound as active then removes it after the clip finishes
    private IEnumerator TrackSound(SoundType sound, float duration)
    {
        playingSounds.Add(sound);
        yield return new WaitForSeconds(duration);
        playingSounds.Remove(sound);
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            soundList[i].name = names[i];
        }
    }
#endif
}

[Serializable]
public struct SoundList
{
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
    public AudioClip[] Sounds => sounds;
}
