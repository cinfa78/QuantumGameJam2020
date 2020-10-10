using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameLoader : MonoBehaviour {
    [FormerlySerializedAs("backgroundMusic")]
    public AudioClip menuBackgroundMusic;
    [FormerlySerializedAs("backgroundMusic")]
    public AudioClip gameBackgroundMusic;
    public CanvasGroup faderGroup;
    private AudioSource[] audioSources;
    public static GameLoader Instance;
    private bool isLoading;

    private void Awake() {
        audioSources = GetComponents<AudioSource>();
        audioSources[0].volume = 0;
        audioSources[0].clip = menuBackgroundMusic;
        audioSources[1].volume = 0;
        audioSources[1].clip = gameBackgroundMusic;
        if (Instance != null) {
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
        faderGroup.alpha = 1;
    }

    private IEnumerator ShowingMenu() {
        isLoading = true;
        audioSources[0].Play();
        float timer = 0;
        float firstFadeDuration = 2;
        while (timer < firstFadeDuration) {
            faderGroup.alpha = Mathf.Lerp(1, 0f, timer / firstFadeDuration);
            audioSources[0].volume = Mathf.Lerp(0.5f, 1f, timer / firstFadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        audioSources[0].volume = 1;
        faderGroup.alpha = 0;
        isLoading = false;
    }

    private void Start() {
        StartCoroutine(ShowingMenu());
    }

    public void LoadGameScene() {
        // audioSources[0].Play();
        // audioSources[1].Play();
        if (!isLoading) {
            StartCoroutine(Loading("Game"));
        }
    }

    public void LoadMenuScene() {
        if (!isLoading)
            StartCoroutine(Loading("Menu"));
    }

    private IEnumerator Loading(string sceneName) {
        isLoading = true;
        bool loadingGame = sceneName == "Game";
        float timer = 0;
        const float fadeDuration = 1;
        while (timer < fadeDuration) {
            faderGroup.alpha = Mathf.Lerp(0, 1f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        faderGroup.alpha = 1;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone) {
            yield return null;
        }
        audioSources[1].Play();
        const float startVolume = 0;
        const float endVolume = 1;
        timer = 0;
        const float duration = 2;
        while (timer < duration) {
            float t = timer / duration;
            if (loadingGame) {
                audioSources[0].volume = Mathf.Lerp(endVolume, startVolume, t);
                audioSources[1].volume = Mathf.Lerp(startVolume, endVolume, t);
            }
            else {
                audioSources[1].volume = Mathf.Lerp(endVolume, startVolume, t);
                audioSources[0].volume = Mathf.Lerp(startVolume, endVolume, t);
            }
            faderGroup.alpha = Mathf.Lerp(1, 0f, t);
            timer += Time.deltaTime;
            yield return null;
        }
        audioSources[0].volume = loadingGame ? 0 : 1;
        audioSources[1].volume = loadingGame ? 1 : 0;
        audioSources[0].Stop();
        faderGroup.alpha = 0;
        isLoading = false;
    }
}