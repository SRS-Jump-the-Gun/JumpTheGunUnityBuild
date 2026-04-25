using UnityEngine;
using System;


//when adding sound, make sure to add corresponding category here and add audio file to SoundManager game object
//To call sound effect in script call it like so:  SoundManager.PlaySound(SoundType.{Sound name here});

public enum SoundType
{
    SHOTGUN,
    BACKGROUND_MUSIC 
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;

    private static SoundManager instance;

    private AudioSource audioSource;   // For gunshots/SFX    
    private AudioSource musicSource; // For looping BGM

private void Awake()
    {
        instance = this;
        
        // setup two audio source components in the sound manager game object ( one fo sfx and the other for music)
        AudioSource[] sources = GetComponents<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource = sources[1];
        audioSource = sources[1];
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        instance.audioSource.PlayOneShot(randomClip, volume);
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