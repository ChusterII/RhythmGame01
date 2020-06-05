using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SFXSound
{
    public string sfxName;
    public AudioClip sfxClip;
    [Range(0f, 1f)]
    public float volume;
    [Range(-3f, 3f)]
    public float pitch;
}

public class SFXManager : MonoBehaviour
{
    public List<SFXSound> sfxSounds;
}
