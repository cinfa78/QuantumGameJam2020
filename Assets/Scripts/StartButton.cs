using System;
using DG.Tweening;
using UnityEngine;

public class StartButton : MonoBehaviour {
    public AudioSource confirmAudioSource;
    private CanvasGroup canvasGroup;

    private void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start() {
        GameLoader.Instance.MenuShowed += OnMenuShowed;
        GameLoader.Instance.GameLoading += OnGameLoading;
    }

    private void OnDestroy() {
        GameLoader.Instance.MenuShowed -= OnMenuShowed;
        GameLoader.Instance.GameLoading -= OnGameLoading;
    }

    private void OnGameLoading() {
        canvasGroup.DOFade(0, .5f);
    }

    private void OnMenuShowed() {
        canvasGroup.DOFade(1, .5f);
    }

    public void ButtonClicked() {
        if (canvasGroup.alpha > 0.8f) {
            confirmAudioSource.Play();
            GameLoader.Instance.LoadGameScene();
        }
    }
}