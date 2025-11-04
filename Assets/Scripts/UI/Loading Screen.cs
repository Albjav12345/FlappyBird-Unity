using CarterGames.Assets.AudioManager;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    private float loadingCanvasTime = 2.2f;
    
    [Header("Drag")]
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private Animator loadingCanvasAnimator;
    [SerializeField] private Animator loadingTextAnimator;

    void Start()
    {
        StartCoroutine(CanvasTextFade());
    }



    private IEnumerator CanvasTextFade()
    {
        MusicManager.Play("Game SoundTrack");

        loadingTextAnimator.SetTrigger("ActiveLoadingFadeText");
        
        yield return new WaitForSeconds(loadingCanvasTime);
        loadingCanvasAnimator.SetTrigger("ActiveLoadingCanvasFade");

        yield return new WaitForSeconds(0.3f);
        loadingCanvas.SetActive(false);
    }
}
