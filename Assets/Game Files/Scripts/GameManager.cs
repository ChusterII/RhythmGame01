using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using GameState;
using MEC;
using RhythmTool;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public enum ScoringGrade
{
    Perfect,
    Great,
    Ok,
    Bad
}

namespace GameState
{
    public enum CurrentState
    {
        Intro,
        Main,
        Outro,
        Ending,
        Ended
    }
}


[System.Serializable]
public class ScoringValues
{
    public ScoringGrade scoringGrade;
    public int scoreValue;
    public string textValue;
    public Color32 textColor;
}

public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    [HideInInspector] public int totalScore;
    [HideInInspector] public int multiplier;
    [HideInInspector] public int lastScore;


    [Header("Configuration")] public RhythmEventProvider eventProvider;
    public RhythmPlayer rhythmPlayer;
    public AudioSource audioSource; // puede que sobre
    public UIManager uiManager;
    public JukeBox jukebox;
    public SpawnManager spawnManager;
    public PlayerMove player;
    public LayerMask spawnLayer;
    public SFXPlayer sfxPlayer;

    [Header("Scoring")]
    public List<ScoringValues> scoringValues;

    [Space(10f)]
    [Tooltip("Value of each consecutive PERFECT beat, multiplied by consecutive beats")]
    public int consecutiveScoreValue;

    [Space(10f)]
    public int maxBadValue;
    public int minOkValue;
    public int maxOkValue;
    public int minGreatValue;
    public int maxGreatValue;

    [Header("Multiplier")] 
    public Material emptyMultiplierMaterial;
    public Material filledMultiplierMaterial;
    public Material maxMultiplierMaterial;
    public Color normalMultiplierColor;
    public Color maxMultiplierColor;

    // --------------- Private variables here ---------------
    private List<Beat> _listBeat;
    private Track<Beat> _trackBeat;
    private Dictionary<ScoringGrade, int> _scoreValuesDictionary;
    private int _multiplierProgressCounter;
    private bool _reachedMaxMultiplier;
    private ScoringGrade _scoringGrade;
    private ScoringGrade _currentScoringGrade;
    private ScoringGrade _lastScoringGrade;
    private bool _coinCollected;
    private int _consecutiveMultiplier;
    private int _consecutiveScore;
    private Collider[] _overlapColliders;
    private bool _isConsecutiveScore;
    private float _songTimer;
    [HideInInspector]
    public float songTotalTime;
    [HideInInspector] 
    public CurrentState state;
    private bool _isFadingOut;
    private bool _isTurningOff;


    // Start is called before the first frame update
    void Start()
    {
        InitializeScore();
        InitializeDictionary();
        InitializeStartingVariables();

        Timing.RunCoroutine(_PlayGame());
        
    }

    /// <summary>
    /// Initialization: methods that initialize or reset values
    /// </summary>
    #region Initialization

    private void InitializeStartingVariables()
    {
        player.movementEnabled = false;
        _coinCollected = false;
        _lastScoringGrade = ScoringGrade.Bad;
        _overlapColliders = new Collider[3];
        _isFadingOut = false;
    }

    private void InitializeScore()
    {
        totalScore = 0;
        multiplier = 1;
        _consecutiveMultiplier = 0; // Starts at 0 because we're not counting the first "Perfect" for consecutiveness purposes
        _multiplierProgressCounter = -1; // Starts at -1 so it's the same as the index of the multiplierProgress array.
        uiManager.SetMaxMultiplierText(false);
    }

    private void InitializeDictionary()
    {
        _scoreValuesDictionary = new Dictionary<ScoringGrade, int>();
        foreach (ScoringValues s in scoringValues)
        {
            _scoreValuesDictionary.Add(s.scoringGrade, s.scoreValue);
        }
    }
    
    private void SetConsecutiveVariables(int multiplierValue)
    {
        _consecutiveMultiplier = multiplierValue;
        _isConsecutiveScore = false;
        _consecutiveScore = 0;
    }
    
    #endregion
    
    private IEnumerator<float> _PlayGame()
    {
        // Wait a few seconds
        yield return Timing.WaitForSeconds(1f);
        
        // Start playing the song
        jukebox.PlaySong();
        
        // Starts the countdown and waits until it's done
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(uiManager._StartCountdown()));
        
        // Enables spawning
        spawnManager.spawnEnabled = true;
        
        // Enable movement
        player.movementEnabled = true;
    }

    private void Update()
    {
        // Keeps the remaining song time
        _songTimer = songTotalTime - rhythmPlayer.time;
        
        // If the player clicked and a coin was not collected, he loses the consecutive multiplier.
        if (!_coinCollected)
        {
            if (Input.GetMouseButtonDown(0))
            {
                CheckGroundClicking();
            }
        }

        uiManager.congratulationsText.text = _songTimer.ToString();
        
        // Ending conditions
        if (_songTimer < 15f && !_isFadingOut && state == CurrentState.Main)
        {
            print("Entered Outro");
            _isFadingOut = true;
            state = CurrentState.Outro;
            print("Starting fade:");
            audioSource.DOFade(0f, 10f).Play();
            sfxPlayer.Play("CrowdClap");
            Timing.RunCoroutine(_DespawnCoins(5f));
        }

        
        if (_songTimer < 7.5f && !_isTurningOff && state == CurrentState.Outro)
        {
            
            print("Entered Ending");
            _isTurningOff = true;
            state = CurrentState.Ending;
           //Timing.RunCoroutine(_DespawnCoins());
            Timing.RunCoroutine(_DespawnPlayer());
            // Play clapping sound
            
        }

        

        /*
        if (!rhythmPlayer.isPlaying && state == CurrentState.Ending)
        {
            print("Entered End");
            state = CurrentState.Ended;
        }*/

    }

    /// <summary>
    /// Score calculations: methods for calculating player score
    /// </summary>
    /// <returns></returns>
    #region Score Calculations
    
    public ScoringGrade CalculateScore()
    {
        // Get a timing score based on the destruction of a coin and the beat timing
        float timingScore = GetTimingScore();

        // Get a scoring grade depending on the timing score
        _scoringGrade = GetScoringGrade(timingScore);
        
        // Setup and calculate the multiplier based on the Scoring Grade
        MultiplierSetup(_scoringGrade);

        if (_scoringGrade == ScoringGrade.Perfect && _lastScoringGrade == ScoringGrade.Perfect)
        {
            // Increase the consecutive Perfect counter
            _consecutiveMultiplier++;

            // We set the floating text to appear and adjust the score accordingly
            _isConsecutiveScore = true;
            _consecutiveScore = (_consecutiveMultiplier - 1) * consecutiveScoreValue;
        }
        else
        {
            if (_scoringGrade == ScoringGrade.Perfect)
            {
                // We set everything to 0 except the multiplier counter, this is the first item in the chain
                SetConsecutiveVariables(1);
            }
            else
            {
                // We set everything to 0, back to the start of the chain
                SetConsecutiveVariables(0);
            }
        }
        
        // We save the current scoring grade
        _lastScoringGrade = _scoringGrade;
        
        // We turn off the coin collected flag that Update uses
        _coinCollected = false;
        
        // We set the score for the coin
        lastScore = IncreaseScore((_scoreValuesDictionary[_scoringGrade] * multiplier) + _consecutiveScore);

        return _scoringGrade;
        
    }

    private void MultiplierSetup(ScoringGrade scoringGrade)
    {
        if (scoringGrade == ScoringGrade.Perfect)
        {
            _multiplierProgressCounter++;

            if (_multiplierProgressCounter < 5)
            {
                // If the counter ain't full, keep lighting the capsules
                //uiManager.multiplierProgress[_multiplierProgressCounter].material = filledMultiplierMaterial;
                uiManager.UpdateMultiplierProgress(_multiplierProgressCounter, filledMultiplierMaterial);
            }
            else
            {
                if (!_reachedMaxMultiplier)
                {
                    // If the capsules are all lit, increase the multiplier
                    multiplier++;

                    if (multiplier >= 4)
                    {
                        // If we're at max multiplier, we can't go higher!
                        uiManager.StartCoroutine("ChainExplosion", maxMultiplierColor);
                        uiManager.SetMultiplierProgress(maxMultiplierMaterial);
                        
                        uiManager.SetMaxMultiplierText(true);

                        _reachedMaxMultiplier = true;

                        // TODO: CAN USE THIS TO SAVE THE LONGEST STREAK
                        _multiplierProgressCounter = 5;
                    }
                    else
                    {
                        // If we aren't at max multiplier, just turn the capsules off
                        uiManager.StartCoroutine("ChainExplosion", normalMultiplierColor);
                        uiManager.SetMultiplierProgress(emptyMultiplierMaterial);

                        _multiplierProgressCounter = -1;
                    }

                    // Update the multiplier on screen
                    uiManager.UpdateMultiplier(multiplier.ToString()); 
                }
            }
        }
        else
        {
            // Lose the counter and the multiplier
            _multiplierProgressCounter = -1;
            multiplier = 1;
            _reachedMaxMultiplier = false;
            uiManager.SetMultiplierProgress(emptyMultiplierMaterial);

            // Update the multiplier on screen
            uiManager.UpdateMultiplier(multiplier.ToString()); 
            uiManager.SetMaxMultiplierText(false);
        }
    }

    private float GetTimingScore()
    {
        //Find the Beats track
        Track<Beat> beats = rhythmPlayer.rhythmData.GetTrack<Beat>();

        //Get the current time in the song
        float time = rhythmPlayer.time;

        //Find the index of the next beat
        int index = beats.GetIndex(time);

        // Get the next beat
        Beat beat = beats[index];

        //Check if the previous beat isn't closer
        if (index > 1 && time - beats[index - 1].timestamp < beat.timestamp - time)
            beat = beats[index - 1];

        //Now use the beat's timestamp and bpm to determine the timing score
        float difference = Mathf.Abs(beat.timestamp - time);
        float beatLength = 60 / beat.bpm;

        //The score is based on the difference between half the beat length and how close the closest beat is
        //If we're on a beat it's 100 and if it's exactly in between 2 beats it's 0
        return Mathf.Lerp(100, 0, difference / (beatLength / 2));
        //}
    }

    private ScoringGrade GetScoringGrade(float timingScore)
    {
        ScoringGrade scoringGrade;
        if (timingScore <= maxBadValue)
        {
            scoringGrade = ScoringGrade.Bad;
        }
        else
        {
            if (timingScore >= minOkValue && timingScore <= maxOkValue)
            {
                scoringGrade = ScoringGrade.Ok;
            }
            else
            {
                if (timingScore >= minGreatValue && timingScore <= maxGreatValue)
                {
                    scoringGrade = ScoringGrade.Great;
                }
                else
                {
                    // Larger than 70
                    scoringGrade = ScoringGrade.Perfect;
                }
            }
        }

        return scoringGrade;
    }

    private int IncreaseScore(int scoreValue)
    {
        totalScore += scoreValue;
        return scoreValue;
    }
    
    #endregion
    
    private void CheckGroundClicking()
    {
        // Check if the player will NOT destroy a coin after the move
        int overlapSize = Physics.OverlapSphereNonAlloc(player.GetDestination(), 0.501f, _overlapColliders, spawnLayer);
        if (overlapSize <= 0)
        {
            // Player clicked somewhere except in a coin, so he loses the consecutive multiplier.
            _consecutiveMultiplier = 0;

            // We set all the flags to 0/Bad to reset the progress
            _isConsecutiveScore = false;
            _consecutiveScore = 0;
            _lastScoringGrade = ScoringGrade.Bad;
        }
    }

    public void ShowScore()
    {
        uiManager.UpdateScore();
    }

    public void CoinCollected()
    {
        _coinCollected = true;
    }

    public bool IsConsecutiveScore()
    {
        return _isConsecutiveScore;
    }

    /// <summary>
    /// Deactivation region: for all methods that are called at the end of the song
    /// </summary>
    #region Deactivations

    private void DeactivateCubeEffect()
    {
        // Turn off cube effect
        Sequence cubeEffectTurnOffSequence = DOTween.Sequence();
        
        cubeEffectTurnOffSequence.Append(jukebox.cubeEffectAMaterial.DOColor(Color.gray, "_BaseColor", 2f));
        cubeEffectTurnOffSequence.Insert(0, jukebox.cubeEffectAMaterial.DOColor(Color.gray, "_EmissionColor", 2f));
        cubeEffectTurnOffSequence.Insert(0, jukebox.cubeEffectBMaterial.DOColor(Color.gray, "_BaseColor", 4f));
        cubeEffectTurnOffSequence.Insert(0, jukebox.cubeEffectBMaterial.DOColor(Color.gray, "_EmissionColor", 4f));
        
        cubeEffectTurnOffSequence.Play();
    }

    private IEnumerator<float> _DespawnPlayer()
    {
        // Disable player movement
        player.movementEnabled = false;
        
        yield return Timing.WaitForSeconds(1f);
        
        // Shrink Player
        player.GetComponent<Animator>().Play("Player Despawn Animation");
    }

    private IEnumerator<float> _DespawnCoins(float duration)
    {
        // Disable spawning
        spawnManager.spawnEnabled = false;
        print("Disabled spawning");
        
        // Find all active coins
        GameObject[] remainingCoins = GameObject.FindGameObjectsWithTag("Coin");
        print(remainingCoins.Length);
        
        // Make each coin small and disappear
        foreach (GameObject coin in remainingCoins)
        {
            // TODO: NOT WORKING!
            coin.gameObject.transform.DOScale(new Vector3(0, 0, 0), 5f).Play();
            print("descaling coin...");
        }
        
        yield return Timing.WaitForSeconds(1f);
        
        // Set each coin to inactive
        foreach (GameObject coin in remainingCoins)
        {
            coin.gameObject.SetActive(false);
        }
    }

    #endregion

    public void ShowEndScreen()
    {
        // 
    }

}
