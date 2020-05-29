using System.Collections;
using System.Collections.Generic;
using RhythmTool;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Track
{

    public string trackName;
    public string artist;
    
    public AudioClip track;

    [Range(0f, 1f)]
    public float volume;
    [Range(-3f, 3f)]
    public float pitch;

    [Tooltip("Can be set by looking at the RhythmData for each song")]
    public float trackBpm;

    public RhythmData rhythmData;

}
