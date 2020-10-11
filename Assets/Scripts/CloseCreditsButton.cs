using UnityEngine;
using UnityEngine.UI;

public class CloseCreditsButton : MonoBehaviour {
    public AudioSource audioSource;
    private Button button;

    private void Awake() {
        GameLoader.Instance.FadeInDone += OnFadeInDone;
        button = GetComponent<Button>();
    }

    private void OnFadeInDone() {
        button.enabled = true;
        GameLoader.Instance.FadeInDone -= OnFadeInDone;
    }

    public void CloseCredits() {
        if (button.enabled) {
            audioSource.Play();
            GameLoader.Instance.LoadMenuScene();
            button.enabled = false;
        }
    }
}