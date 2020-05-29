using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CubeEffectA : MonoBehaviour, ICubeEffect
{

    public Color colorA;
    public Color colorB;
    
    private Material _material;
    private bool _isPulsing = false;
    private Track _currentTrack;
    
    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<Renderer>().material;
        _currentTrack = FindObjectOfType<JukeBox>().currentTrack;
        SetMaterialDefaultColor();
    }

    private void SetMaterialDefaultColor()
    {
        _material.SetColor("_BaseColor", colorA);
        _material.SetColor("_EmissionColor", colorA);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isPulsing)
        {
            
        }
    }

    public void PulseColor()
    {
        Sequence pulseColor = DOTween.Sequence();

        pulseColor.Append(_material.DOColor(colorB, "_BaseColor", 0f));
    }

}
