﻿using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RhythmTool;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class JukeBox : MonoBehaviour
{
    [Header("Basic settings")]
    public TrackManager trackManager;
    public RhythmEventProvider eventProvider;
    public bool randomTrack;

    [Header("Coin Effects")]
    public Material coinMaterial;
    //public Color coinColor1; // MIGHT NOT NEED
    //public Color coinColor2; // MIGHT NOT NEED
    [Range(0f, 0.01f)]
    public float pulseErrorMargin;
    
    [Header("Cube Effects")]
    public Material cubeEffectAMaterial;
    public Material cubeEffectBMaterial;
    public Color cubeAColor1;
    public Color cubeAColor2;
    public Color cubeBColor1;
    public Color cubeBColor2;

    // --------------- Private variables here ---------------
    private RhythmPlayer _rhythmPlayer;
    [HideInInspector]
    public Track currentTrack;
    private AudioSource _audioSource;
    private Track<Beat> _beats;
    

    private void Awake()
    {
        InitializeComponents();   
        ResetMaterialsColor();
        DoTweenSetup();
        eventProvider.Register<Beat>(OnBeat);
        PickTrackToPlay();
        LoadTrackConfig();
        LoadTrack();
    }

    void Start()
    {
        
    }

    private void DoTweenSetup()
    {
        DOTween.Init();
        DOTween.defaultAutoPlay = AutoPlay.AutoPlayTweeners;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// Is called on each beat of the song
    /// </summary>
    /// <param name="beat">The current beat</param>
    private void OnBeat(Beat beat)
    {
        BeatPulseController(beat);
    }

    private void BeatPulseController(Beat beat)
    {
        //Get the current time in the song
        float time = _rhythmPlayer.time;

        //Find the index of this beat
        int thisIndex = _beats.GetIndex(time);

        // Check if song isn't over
        if (thisIndex < _beats.count)
        {
            // Calculate the half of the difference in seconds between beats
            float pulseTime = (CalculateBeatDifference(beat, thisIndex) / 2f ) - pulseErrorMargin;

            // Pulse the elements on screen based on the modified difference minus an error margin
            PulseCoinColor(pulseTime);
            PulseCubeEffectColor(pulseTime);
        }
    }

    private float CalculateBeatDifference(Beat beat, int index)
    {
        // Get the next beat
        Beat nextBeat = _beats[index];

        // Take the difference in seconds between the next beat and this one
        return nextBeat.timestamp - beat.timestamp;
    }


    private void InitializeComponents()
    {
        _audioSource = GetComponent<AudioSource>();
        _rhythmPlayer = GetComponent<RhythmPlayer>();
    }
    
    private void LoadTrackConfig()
    {
        _audioSource.volume = currentTrack.volume;
        _audioSource.pitch = currentTrack.pitch;
    }

    private void LoadTrack()
    {
        _audioSource.clip = currentTrack.track;
        _rhythmPlayer.rhythmData = currentTrack.rhythmData;
        _beats = _rhythmPlayer.rhythmData.GetTrack<Beat>();
    }
    
    private void PickTrackToPlay()
    {
        if (randomTrack)
        {
            int trackIndex = Random.Range(0, trackManager.tracks.Length);
            currentTrack = trackManager.tracks[trackIndex];
        }
        else
        {
            // TODO: REMOVE HARDCODE
            currentTrack = Array.Find(trackManager.tracks, track => track.trackName == "Cubic Z");
            
        }
    }

    private void PulseCubeEffectColor(float time)
    {
        Sequence cubeEffectPulseSequence = DOTween.Sequence();
        
        // TWO COLOR SEQUENCE FOR ONE CUBE EFFECT
        cubeEffectPulseSequence.Append(cubeEffectAMaterial.DOColor(cubeAColor2, "_BaseColor", time));
        cubeEffectPulseSequence.Insert(0, cubeEffectAMaterial.DOColor(cubeAColor2, "_EmissionColor", time));
        cubeEffectPulseSequence.Insert(0, cubeEffectBMaterial.DOColor(cubeBColor2, "_BaseColor", time));
        cubeEffectPulseSequence.Insert(0, cubeEffectBMaterial.DOColor(cubeBColor2, "_EmissionColor", time));
        
        cubeEffectPulseSequence.Append(cubeEffectAMaterial.DOColor(cubeAColor1, "_BaseColor", time));
        cubeEffectPulseSequence.Insert(1, cubeEffectAMaterial.DOColor(cubeAColor1, "_EmissionColor", time));
        cubeEffectPulseSequence.Insert(1,cubeEffectBMaterial.DOColor(cubeBColor1, "_BaseColor", time));
        cubeEffectPulseSequence.Insert(1, cubeEffectBMaterial.DOColor(cubeBColor1, "_EmissionColor", time));

        cubeEffectPulseSequence.Play();
    }

    public void PlaySong()
    {
        _audioSource.Play();
    }

    private void PulseCoinColor(float time)
    {
        Sequence coinPulseSequence = DOTween.Sequence();

        coinPulseSequence.Append(coinMaterial.DOFloat(-0.15f, "_Glow", time));
        coinPulseSequence.Append(coinMaterial.DOFloat(0f, "_Glow", time));

        coinPulseSequence.Play();
    }

    private void ResetMaterialsColor()
    {
        
        coinMaterial.SetFloat("_Glow", 0f);
        //coinMaterial.SetColor("_BaseColor", coinColor2);
        //coinMaterial.SetColor("_EmissionColor", coinColor2);
        cubeEffectAMaterial.SetColor("_BaseColor", cubeAColor1);
        cubeEffectAMaterial.SetColor("_EmissionColor", cubeAColor1);
        cubeEffectBMaterial.SetColor("_BaseColor", cubeBColor1);
        cubeEffectBMaterial.SetColor("_EmissionColor", cubeBColor1);
    }
    
    
}
