using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MEC;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Singleton

    public static UIManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    #endregion
    
    [Header("Configuration")]
    public GameManager gameManager;
    
    [Header("Scoring")]
    public TextMeshProUGUI scoreText;
    public Animator scoreTextAnimator;
    public TextMeshProUGUI addScoreText;
    public Animator addScoreTextAnimator;

    [Header("Multiplier")] 
    public Renderer[] multiplierProgress;
    public TextMeshProUGUI multiplierText;
    public Animator multiplierTextAnimator;
    public TextMeshProUGUI maxMultiplierText;
    public Animator maxMultiplierTextAnimator;
    
    [Header("Multiplier Effects")] 
    public ParticleSystem[] multiplierEffects;

    [Header("Jukebox Info Panel")] 
    public TextMeshPro songNameText;
    public TextMeshPro byText;
    public TextMeshPro artistNameText;
    public JukeBox jukeBox;
    public Material borderMaterial;
    public Material infoPanelMaterial;
    public Color startingBorderColor;

    [Header("Countdown")] 
    public TextMeshProUGUI countDownText;
    public Animator countDownTextAnimator;


    // Start is called before the first frame update
    void Start()
    {
        scoreText.text = "0";
        multiplierText.text = "1";

        songNameText.text = jukeBox.currentTrack.trackName;
        artistNameText.text = jukeBox.currentTrack.artist;

        DOTween.defaultAutoPlay = AutoPlay.None;

        borderMaterial.SetColor("_BaseColor", startingBorderColor);
        infoPanelMaterial.SetFloat("DissolveValue", 1f);

        Timing.RunCoroutine(JukeBoxInfoPanelSequence());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator<float> JukeBoxInfoPanelSequence()
    {
        yield return Timing.WaitForSeconds(0.5f);
        JukeBoxInfoPanelFadeIn();
        yield return Timing.WaitForSeconds(4.5f);
        JukeBoxInfoPanelFadeOut();
    }

    public IEnumerator<float> _StartCountdown()
    {
        SetCountdownText("4");
        yield return Timing.WaitForSeconds(1f);
        SetCountdownText("3");
        yield return Timing.WaitForSeconds(1f);
        SetCountdownText("2");
        yield return Timing.WaitForSeconds(1f);
        SetCountdownText("1");
        yield return Timing.WaitForSeconds(1f);
        SetCountdownText("Go!");
        yield return Timing.WaitForSeconds(1f);
    }

    private void SetCountdownText(string text)
    {
        countDownText.text = text;
        countDownTextAnimator.SetTrigger("PlayCountdown");
    }

    private void JukeBoxInfoPanelFadeIn()
    {
        Sequence fadeInSequence = DOTween.Sequence();

        fadeInSequence.Append(borderMaterial.DOFade(1f, "_BaseColor", 0.5f));
        fadeInSequence.Join(infoPanelMaterial.DOFloat(0f, "DissolveValue", 0.5f));
        fadeInSequence.Append(songNameText.DOFade(1f, 0.5f));
        fadeInSequence.Append(byText.DOFade(1f, 0.3f));
        fadeInSequence.Join(artistNameText.DOFade(1f, 0.3f));
    }
    
    private void JukeBoxInfoPanelFadeOut()
    {
        Sequence fadeOutSequence = DOTween.Sequence();

        fadeOutSequence.Append(songNameText.DOFade(0f, 0.25f));
        fadeOutSequence.Append(byText.DOFade(0f, 0.25f));
        fadeOutSequence.Append(artistNameText.DOFade(0f, 0.25f));
        fadeOutSequence.Append(borderMaterial.DOFade(0f, "_BaseColor", 0.5f));
        fadeOutSequence.Join(infoPanelMaterial.DOFloat(1f, "DissolveValue", 1.5f));
    }

    public void UpdateScore()
    {
        scoreText.text = gameManager.totalScore.ToString();
        scoreTextAnimator.SetTrigger("IncreaseScore");
        addScoreText.text = "+" + gameManager.lastScore;
        addScoreTextAnimator.SetTrigger("AddScore");
    }

    public void UpdateMultiplier(string multiplier)
    {
        multiplierText.text = multiplier;
        multiplierTextAnimator.SetTrigger("IncreaseMultiplier");
    }

    public void UpdateMultiplierProgress(int index, Material mat)
    {
        multiplierProgress[index].material = mat;
    }

    public void SetMultiplierProgress(Material mat)
    {
        foreach (Renderer r in multiplierProgress)
        {
            r.material = mat;
        }
    }

    public IEnumerator ChainExplosion(Color color)
    {
        for (int i = 0; i < multiplierEffects.Length; i++)
        {
            ParticleSystem.MainModule main = multiplierEffects[i].main;
            main.startColor = new ParticleSystem.MinMaxGradient(color);
            multiplierEffects[i].Play();
            yield return new WaitForSeconds(0.02f);
        }
    }

    public void SetMaxMultiplierText(bool value)
    {
        maxMultiplierText.gameObject.SetActive(value);
        if (value)
        {
            maxMultiplierTextAnimator.SetTrigger("IncreaseMultiplier");
        }
        multiplierText.gameObject.SetActive(!value);
    }

    
}
