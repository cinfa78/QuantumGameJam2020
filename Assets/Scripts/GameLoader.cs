using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour {
    public AudioClip menuBackgroundMusic;
    [Range(0, 1f)] public float menuVolume;
    public AudioClip gameBackgroundMusic;
    [Range(0, 1f)] public float gameVolume;
    public AudioClip creditsBackgroundMusic;
    [Range(0, 1f)] public float creditsVolume;
    public CanvasGroup faderGroup;
    private AudioSource[] audioSources;
    public static GameLoader Instance;
    private bool isLoading;
    public event Action MenuShowed;
    public event Action MenuLoaded;
    public event Action GameLoading;
    public event Action FadeInDone;

    private void Awake() {
        audioSources = GetComponents<AudioSource>();
        audioSources[0].volume = 0;
        audioSources[0].clip = menuBackgroundMusic;
        audioSources[1].volume = 0;
        audioSources[1].clip = gameBackgroundMusic;
        audioSources[2].volume = 0;
        audioSources[2].clip = creditsBackgroundMusic;
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
        float startFadeAlpha = faderGroup.alpha;
        while (timer < firstFadeDuration) {
            faderGroup.alpha = Mathf.Lerp(startFadeAlpha, 0f, timer / firstFadeDuration);
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

    public void LoadCreditsScene() {
        if (!isLoading) {
            GameLoading?.Invoke();
            StartCoroutine(Loading("Credits"));
        }
    }

    public void LoadMenuScene() {
        if (!isLoading)
            StartCoroutine(Loading("Menu"));
    }

    private IEnumerator Loading(string sceneName) {
        isLoading = true;
        bool isLoadingMenu = sceneName == "Menu";
        bool isLoadingGame = sceneName == "Game";
        bool isLoadingCredits = sceneName == "Credits";
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

        if (isLoadingGame) {
            audioSources[1].Play();
        }
        if (isLoadingCredits) {
            audioSources[2].Play();
        }

        timer = 0;
        const float duration = 2;
        while (timer < duration) {
            float t = timer / duration;
            if (isLoadingGame) {
                audioSources[0].volume = Mathf.Lerp(menuVolume, 0, t);
                audioSources[1].volume = Mathf.Lerp(0, gameVolume, t);
            }
            else if (isLoadingCredits) {
                audioSources[2].volume = Mathf.Lerp(creditsVolume, 0, t);
                audioSources[0].volume = Mathf.Lerp(0, menuVolume, t);
            }
            else if (isLoadingMenu) {
                if (audioSources[1].isPlaying) {
                    audioSources[1].volume = Mathf.Lerp(audioSources[1].volume, 0, t);
                }
                if (audioSources[2].isPlaying) {
                    audioSources[2].volume = Mathf.Lerp(audioSources[2].volume, 0, t);
                }
                audioSources[0].volume = Mathf.Lerp(0, menuVolume, t);
            }
            faderGroup.alpha = Mathf.Lerp(1, 0f, t);
            timer += Time.deltaTime;
            yield return null;
        }
        FadeInDone?.Invoke();
        audioSources[0].volume = isLoadingGame ? 0 : menuVolume;
        audioSources[1].volume = isLoadingGame ? gameVolume : 0;
        audioSources[2].volume = isLoadingCredits ? creditsVolume : 0;
        audioSources[0].Stop();
        faderGroup.alpha = 0;
        isLoading = false;
        if (!isLoadingGame && !isLoadingCredits) {
            MenuLoaded?.Invoke();
        }
    }
}