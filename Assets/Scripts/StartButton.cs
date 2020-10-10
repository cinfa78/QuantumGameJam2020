using DG.Tweening;
using UnityEngine;

public class StartButton : MonoBehaviour {
    private void Start() {
        GameLoader.Instance.MenuShowed += OnMenuShowed;
        GameLoader.Instance.GameLoading += OnGameLoading;
    }

    private void OnDestroy() {
        GameLoader.Instance.MenuShowed -= OnMenuShowed;
        GameLoader.Instance.GameLoading -= OnGameLoading;
    }

    private void OnGameLoading() {
        GetComponent<CanvasGroup>().DOFade(0, .5f);
    }

    private void OnMenuShowed() {
        GetComponent<CanvasGroup>().DOFade(1, .5f);
    }

    public void ButtonClicked() {
        GameLoader.Instance.LoadGameScene(); 
    }
}