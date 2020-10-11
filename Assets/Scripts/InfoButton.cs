using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InfoButton : MonoBehaviour {
    public AudioSource confirmAudioSource;
    public CanvasGroup infoBox;
    private CanvasGroup canvasGroup;

    private void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start() {
        GameLoader.Instance.MenuShowed += OnMenuShowed;
        GameLoader.Instance.GameLoading += OnGameLoading;
    }

    private void OnGameLoading() {
        GetComponent<Button>().enabled = false;
        canvasGroup.DOFade(0, .5f);
    }

    private void OnMenuShowed() {
        canvasGroup.DOFade(1, .5f);
    }

    private void OnDestroy() {
        GameLoader.Instance.MenuShowed -= OnMenuShowed;
        GameLoader.Instance.GameLoading -= OnGameLoading;
    }

    public void ShowInfo() {
        if (canvasGroup.alpha > 0.8) {
            confirmAudioSource.Play();
            infoBox.gameObject.SetActive(true);
            infoBox.DOFade(1, .5f);
            GetComponent<CanvasGroup>().DOFade(0, .5f);
            infoBox.DOFade(1, .5f);
        }
    }
}