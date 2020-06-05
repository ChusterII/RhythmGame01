using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    
    #region Singleton

    public static SFXPlayer Instance;

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    public SFXManager sfxManager;
    public AudioSource audioSource;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play(string sfxName)
    {
        SFXSound sfxSound = sfxManager.sfxSounds.Find(sound => sound.sfxName == sfxName);
        audioSource.volume = sfxSound.volume;
        audioSource.pitch = sfxSound.pitch;
        audioSource.clip = sfxSound.sfxClip;
        audioSource.Play();
    }
}
