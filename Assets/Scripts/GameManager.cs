using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using RhythmTool;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public enum ScoringGrade
{
    Perfect,
    Great,
    Ok,
    Bad
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
    
    [HideInInspector]
    public int totalScore;
    [HideInInspector]
    public int multiplier;
    [HideInInspector] 
    public int lastScore;
    [HideInInspector]
    public ScoringGrade lastScoringGrade;

    [Header("Configuration")]
    public RhythmEventProvider eventProvider;
    public RhythmPlayer rhythmPlayer;
    public AudioSource audioSource; // puede que sobre
    public UIManager uiManager;
    public JukeBox jukebox;
    public SpawnManager spawnManager;
    public PlayerMove player;

    [Header("Scoring")]
    public List<ScoringValues> scoringValues;

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
    
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        InitializeScore();
        InitializeDictionary();
        InitializeStartingVariables();

        Timing.RunCoroutine(_PlayGame());
    }

    private void InitializeStartingVariables()
    {
        player.movementEnabled = false;
        lastScoringGrade = ScoringGrade.Bad;
    }

    private void InitializeScore()
    {
        totalScore = 0;
        multiplier = 1;
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
        if (Input.GetMouseButtonDown(0))
        {
            ScoringGrade currentScoringGrade = GetScoringGrade(GetTimingScore());
            print("This grade: " + currentScoringGrade + " |||| Last grade: " + lastScoringGrade);
            if (currentScoringGrade == ScoringGrade.Perfect && lastScoringGrade == ScoringGrade.Perfect)
            {
                print("Awesome");
                
            }
            else
            {
                print(currentScoringGrade);
            }

            lastScoringGrade = currentScoringGrade;
        }
    }


    public ScoringGrade CalculateScore()
    {
        // Get a timing score based on the destruction of a coin and the beat timing
        float timingScore = GetTimingScore();

        // Get a scoring grade depending on the timing score
        _scoringGrade = GetScoringGrade(timingScore);

        // Setup and calculate the multiplier based on the Scoring Grade
        MultiplierSetup(_scoringGrade);
        
        // Increase the score based on the grade multiplied by the current multiplier
        lastScore = IncreaseScore(_scoreValuesDictionary[_scoringGrade] * multiplier);

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

        //Get the beat at this index
        Beat beat = beats[index];

        //Check if the previous beat isn't closer
        if (index > 1 && time - beats[index - 1].timestamp < beat.timestamp - time)
            beat = beats[index - 1];

        //Now use the beat's timestamp and bpm to determine the timing score
        float difference = Mathf.Abs(beat.timestamp - time);
        float beatLength = 60 / beat.bpm;

        //The score is based on the difference between half the beat length and how close the closest beat is
        //If we're on a beat it's 100 and if it's exactly in between 2 beats it's 0
        return Mathf.Lerp(100, 0, difference / (beatLength / 2));;
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

    public void ShowScore()
    {
        uiManager.UpdateScore();
    }

}
