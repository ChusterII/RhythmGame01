using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    public TextMeshProUGUI maxMultiplierText;
    
    [Header("Multiplier Effects")] 
    public ParticleSystem[] multiplierEffects;

    [Header("Jukebox Info Panel")] 
    public TextMeshPro songNameText;
    public TextMeshPro byText;
    public TextMeshPro artistNameText;
    public JukeBox jukeBox;
    public Material borderMaterial;
    public Material infoPanelMaterial;
    public Color originalBorderColor;


    // Start is called before the first frame update
    void Start()
    {
        scoreText.text = "0";
        multiplierText.text = "1";

        songNameText.text = jukeBox.currentTrack.trackName;
        artistNameText.text = jukeBox.currentTrack.artist;

        DOTween.defaultAutoPlay = AutoPlay.None;

        borderMaterial.SetColor("_BaseColor", originalBorderColor);
        infoPanelMaterial.SetFloat("DissolveValue", 1f);

        StartCoroutine("JukeBoxInfoPanelSequence");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator JukeBoxInfoPanelSequence()
    {
        JukeBoxInfoPanelFadeIn();
        yield return new WaitForSeconds(5f);
        JukeBoxInfoPanelFadeOut();
    }

    private void JukeBoxInfoPanelFadeIn()
    {
        Sequence fadeInSequence = DOTween.Sequence();

        print("Fading in");
        fadeInSequence.Append(borderMaterial.DOFade(1f, "_BaseColor", 1f));
        fadeInSequence.Join(infoPanelMaterial.DOFloat(0f, "DissolveValue", 2.5f));
        fadeInSequence.Append(songNameText.DOFade(1f, 1f));
        fadeInSequence.Join(byText.DOFade(1f, 1f));
        fadeInSequence.Join(artistNameText.DOFade(1f, 1f));
    }
    
    private void JukeBoxInfoPanelFadeOut()
    {
        Sequence fadeOutSequence = DOTween.Sequence();

        print("Fading out");
        fadeOutSequence.Append(songNameText.DOFade(0f, 0.5f));
        fadeOutSequence.Append(byText.DOFade(0f, 0.5f));
        fadeOutSequence.Append(artistNameText.DOFade(0f, 0.5f));
        fadeOutSequence.Append(borderMaterial.DOFade(0f, "_BaseColor", 1f));
        fadeOutSequence.Join(infoPanelMaterial.DOFloat(1f, "DissolveValue", 2f));

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
        multiplierText.gameObject.SetActive(!value);
    }

    
}
