using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RhythmTool;
using UnityEngine;
using Random = UnityEngine.Random;

public class JukeBox : MonoBehaviour
{
    
    
    public TrackManager trackManager;
    public RhythmEventProvider eventProvider;
    
    [Header("Coin Effects")]
    public Material coinMaterial;
    public Color coinColor1;
    public Color coinColor2;
    
    [Header("Cube Effects")]
    public Material cubeEffectAMaterial;
    public Material cubeEffectBMaterial;
    public Color cubeAColor1;
    public Color cubeAColor2;
    public Color cubeBColor1;
    public Color cubeBColor2;
    //private bool _effectIsPulsing = false;
    
    public bool randomTrack;

    private RhythmPlayer _rhythmPlayer;
    [HideInInspector]
    public Track currentTrack;
    private AudioSource _audioSource;

    private int beatcount;

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
        float secondsPerBeat = CalculateSecondsPerBeat(beat);
        PulseCoinColor((secondsPerBeat) / 2f);
        PulseCubeEffectColor(secondsPerBeat / 2f);
        
        print(beatcount);
        beatcount++;
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
            currentTrack = Array.Find(trackManager.tracks, track => track.trackName == "Seven Twenty");
            
        }
    }

    private float CalculateSecondsPerBeat(Beat beat)
    {
        return 60f / beat.bpm; ;
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
    
    
    private void PulseCoinColor(float halfBeat)
    {

        Sequence coinPulseSequence = DOTween.Sequence();
        
        // TWO COLOR SEQUENCE WITH TWO VARIABLES
        coinPulseSequence.Append(coinMaterial.DOColor(coinColor2, "_BaseColor", halfBeat));
        coinPulseSequence.Insert(0, coinMaterial.DOColor(coinColor2, "_EmissionColor", halfBeat));
        coinPulseSequence.Append(coinMaterial.DOColor(coinColor1, "_BaseColor", halfBeat));
        coinPulseSequence.Insert(1, coinMaterial.DOColor(coinColor1, "_EmissionColor", halfBeat));

        coinPulseSequence.Play();
    }

    private void ResetMaterialsColor()
    {
        coinMaterial.SetColor("_BaseColor", coinColor2);
        coinMaterial.SetColor("_EmissionColor", coinColor2);
        cubeEffectAMaterial.SetColor("_BaseColor", cubeAColor1);
        cubeEffectAMaterial.SetColor("_EmissionColor", cubeAColor1);
        cubeEffectBMaterial.SetColor("_BaseColor", cubeBColor1);
        cubeEffectBMaterial.SetColor("_EmissionColor", cubeBColor1);
    }
    
    
}
