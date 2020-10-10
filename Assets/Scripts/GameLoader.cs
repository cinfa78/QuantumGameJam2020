using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour {
    public AudioClip menuBackgroundMusic;
    [Range(0, 1f)] public float menuVolume;
    public AudioClip gameBackgroundMusic;
    [Range(0, 1f)] public float gameVolume;
    public CanvasGroup faderGroup;
    private AudioSource[] audioSources;
    public static GameLoader Instance;
    private bool isLoading;
    public event Action MenuShowed;
    public event Action MenuLoaded;
    public event Action GameLoading;

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
        MenuLoaded += OnMenuLoaded;
    }

    private IEnumerator ShowingMenu() {
        isLoading = true;
        audioSources[0].Play();
        float timer = 0;
        float firstFadeDuration = 2;
        while (timer < firstFadeDuration) {
            faderGroup.alpha = Mathf.Lerp(1, 0f, timer / firstFadeDuration);
            audioSources[0].volume = Mathf.Lerp(0.5f, menuVolume, timer / firstFadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        audioSources[0].volume = 1;
        faderGroup.alpha = 0;
        isLoading = false;
        MenuShowed?.Invoke();
    }

    private void OnMenuLoaded() {
        StartCoroutine(ShowingMenu());
    }

    private void Start() {
        OnMenuLoaded();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    public void LoadGameScene() {
        // audioSources[0].Play();
        // audioSources[1].Play();
        if (!isLoading) {
            GameLoading?.Invoke();
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
        timer = 0;
        const float duration = 2;
        while (timer < duration) {
            float t = timer / duration;
            if (loadingGame) {
                audioSources[0].volume = Mathf.Lerp(menuVolume, 0, t);
                audioSources[1].volume = Mathf.Lerp(0, gameVolume, t);
            }
            else {
                audioSources[1].volume = Mathf.Lerp(gameVolume, 0, t);
                audioSources[0].volume = Mathf.Lerp(0, menuVolume, t);
            }
            faderGroup.alpha = Mathf.Lerp(1, 0f, t);
            timer += Time.deltaTime;
            yield return null;
        }
        audioSources[0].volume = loadingGame ? 0 : menuVolume;
        audioSources[1].volume = loadingGame ? gameVolume : 0;
        audioSources[0].Stop();
        faderGroup.alpha = 0;
        isLoading = false;
        if (!loadingGame)
            MenuLoaded?.Invoke();
    }
}