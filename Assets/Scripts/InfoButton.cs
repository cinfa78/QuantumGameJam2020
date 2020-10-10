using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InfoButton : MonoBehaviour {
    public CanvasGroup infoBox;
    private void Start() {
        GameLoader.Instance.MenuShowed += OnMenuShowed;
        GameLoader.Instance.GameLoading += OnGameLoading;
    }

    private void OnGameLoading() {
        GetComponent<Button>().enabled = false;
        GetComponent<CanvasGroup>().DOFade(0, .5f);
    }

    private void OnMenuShowed() {
        GetComponent<CanvasGroup>().DOFade(1, .5f);
    }

    private void OnDestroy() {
        GameLoader.Instance.MenuShowed -= OnMenuShowed;
        GameLoader.Instance.GameLoading -= OnGameLoading;
    }
    public void ShowInfo() {
        infoBox.gameObject.SetActive(true);
        infoBox.DOFade(1, .5f);
        GetComponent<CanvasGroup>().DOFade(0, .5f);
        infoBox.DOFade(1, .5f);
    }
}