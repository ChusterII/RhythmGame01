using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    
    
    public Renderer coinRenderer;
    public Collider coinCollider;
    public TraumaInducer traumaInducer;
    public GameObject floatingTextObject;
    public TextMeshPro floatingText;
    
    private ObjectPooler _objectPooler;
    private GameObject _deathEffect;
    private GameManager _gameManager;
    private ScoringValues _scoreValue;
    

    private void Start()
    {
        _objectPooler = ObjectPooler.Instance;
        _gameManager = GameManager.Instance;
        floatingTextObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // before destroying, check if it was killed on a beat.
        ScoringGrade scoringGrade = _gameManager.CalculateScore();
        
        // Disable the mesh renderer and the mesh collider
        SetComponents(false);
        
        // Enable the floating text with the correct string and color
        ShowFloatingText(scoringGrade);
        
        // Update score
        _gameManager.ShowScore();
        
        // Finish mopping up the coin
        DestroyCoin();
    }

    private void SetComponents(bool value)
    {
        coinRenderer.enabled = value;
        coinCollider.enabled = value;
    }

    private void ShowFloatingText(ScoringGrade scoringGrade)
    {
        floatingTextObject.SetActive(true);
        _scoreValue = _gameManager.scoringValues.Find(values => values.scoringGrade == scoringGrade);
        floatingText.text = _scoreValue.textValue;
        floatingText.color = _scoreValue.textColor;
    }

    private void DestroyCoin()
    {
        _deathEffect = _objectPooler.SpawnFromPool("Death Effect", transform.position, Quaternion.Euler(-90, 0 ,0));
        traumaInducer.GenerateTrauma();
        Invoke("DisableFloatingText", 1.1f);
        Invoke("DisableEffect", 2.5f);
        Invoke("DisableCoin", 1f);
    }

    private void DisableEffect()
    {
        _deathEffect.SetActive(false);
    }

    private void DisableCoin()
    {
        // Deactivate the object
        gameObject.SetActive(false);
        
        // Set the components to active so the next spawns of this object can be seen and interacted with.
        SetComponents(true);
    }

    private void DisableFloatingText()
    {
        floatingTextObject.SetActive(false);
    }
}
