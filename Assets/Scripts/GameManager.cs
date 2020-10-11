using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
//using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
    [Serializable]
    public enum State {
        Intro = 0,
        FirstMove = 1,
        GamePlay = 2,
        End = 3
    }

    public AudioClip[] notes;
    [ColorUsage(true, true)] public Color startNodeColor;
    [ColorUsage(true, true)] public Color endNodeColor;
    [ColorUsage(true, true)] public Color playerColor;
    [ColorUsage(true, true)] public Color aiColor;
    [ReadOnly] public State state;
    public CanvasGroup inGameBackground;
    public CanvasGroup startBackground;
    public CanvasGroup endBackground;
    public GameObject scoreContainer;
    public GameObject interfaceContainer;
    public GameObject nodesContainer;
    public GameObject instructionsContainer;
    public GameObject retryButton;
    public TMP_Text scoreLabel;
    public TMP_Text targetScoreLabel;
    public TMP_Text resultLabel;
    public Image resultImage;
    public float fadeTime = 2;
    public Gradient resultImageGradient;
    public MeshRenderer leafMeshRenderer;
    public Node rootNode;
    private Node endNode;
    private Node[] nodes;
    [ReadOnly, SerializeField] private int turn;
    public int scoreMod;
    [ReadOnly, SerializeField] private int score;
    public int Score {
        get => score;
        set {
            score = value;
            scoreLabel.text = $"{score}";
        }
    }
    [ReadOnly] public int targetScore;
    [SerializeField, ReadOnly] private List<Node> aiPath;
    [SerializeField, ReadOnly] private List<Node> playerPath;
    [SerializeField, ReadOnly] private Node playerCurrentNode;
    private AudioSource audioSource;
    private List<int> notesSequence;

    private int lastPickedNote = -1;

    public event Action Faded;

    private void Awake() {
        turn = 0;
        startBackground.alpha = 1;
        inGameBackground.alpha = 0;
        endBackground.alpha = 0;
        state = State.Intro;
        aiPath = new List<Node>();
        targetScoreLabel.text = "0";
        scoreLabel.text = "0";
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        notesSequence = new List<int>();
        interfaceContainer.SetActive(false);
        retryButton.GetComponent<CanvasGroup>().alpha = 0;
        retryButton.SetActive(false);
    }

    private void Start() {
        targetScoreLabel.color = aiColor;
        scoreLabel.color = playerColor;
        nodes = FindObjectsOfType<Node>();
        foreach (Node node in nodes) {
            node.Selected += OnNodeSelected;
            node.CanBeSelected = false;
            if (node.endNode) {
                endNode = node;
                endNode.Showed += OnEndNodeShowed;
            }
        }
        if (endNode == null) {
            Debug.LogError($"MISSING END NODE");
        }
        leafMeshRenderer.material = Instantiate(leafMeshRenderer.material);
        leafMeshRenderer.material.SetFloat("_Fade", 0);
        Faded += StartGame;
        scoreContainer.GetComponent<CanvasGroup>().alpha = 0;
        FadeToGame();
    }

    private void OnEndNodeShowed(Node obj) {
        state = State.FirstMove;
        playerCurrentNode = rootNode;
        playerPath.Add(rootNode);
        foreach (var exitNode in playerCurrentNode.exitNodes) {
            exitNode.CanBeSelected = true;
        }
        interfaceContainer.SetActive(true);
        UpdateResult();
    }

    private void StartGame() {
        Faded -= StartGame;
        rootNode.ShowNode(true);

        rootNode.SetColor(startNodeColor);
        rootNode.nodeFilled.enabled = true;
        endNode.SetColor(endNodeColor);
        endNode.nodeFilled.enabled = true;
    }

    private void FadeToGame() {
        StartCoroutine(FadingBackground(startBackground, inGameBackground));
    }

    private void FadeToEnd() {
        nodesContainer.SetActive(false);
        Faded += OnEndFading;
        StartCoroutine(FadingBackground(inGameBackground, endBackground));
        StartCoroutine(ColoringLeaf());
        StartCoroutine(ReplayNotes());
    }

    private IEnumerator ReplayNotes() {
        for (int i = 0; i < notesSequence.Count; i++) {
            audioSource.PlayOneShot(notes[notesSequence[i]]);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnEndFading() {
        nodesContainer.SetActive(true);
        retryButton.SetActive(true);
        retryButton.GetComponent<CanvasGroup>().DOFade(1, 1f);
    }

    private IEnumerator ColoringLeaf() {
        float timer = 0;
        float duration = fadeTime;
        while (timer < duration) {
            float t = timer / duration;
            leafMeshRenderer.material.SetFloat("_Fade", Mathf.Lerp(0, 1, t));
            timer += Time.deltaTime;
            yield return null;
        }
        leafMeshRenderer.material.SetFloat("_Fade", 1);
    }

    private IEnumerator FadingBackground(CanvasGroup a, CanvasGroup b) {
        float timer = 0;
        float duration = fadeTime * 1.5f;
        while (timer < duration) {
            float t = timer / duration;
            a.alpha = Mathf.Lerp(1, 0, t);
            b.alpha = Mathf.Lerp(0, 1, t);
            timer += Time.deltaTime;
            yield return null;
        }
        a.alpha = 0;
        b.alpha = 1;
        Faded?.Invoke();
    }

    public void CalculateAiPath() {
        Node currentNode = rootNode;
        aiPath.Add(currentNode);
        var choices = new Node[currentNode.exitNodes.Length - 1];
        Debug.Log($"choices found {choices.Length}");
        int i = 0;
        foreach (var exitNode in currentNode.exitNodes) {
            if (!exitNode.SelectedByPlayer) {
                choices[i] = exitNode;
                i++;
            }
        }
        while (currentNode != endNode) {
            if (choices.Length > 0) {
                Node chosen = choices[Random.Range(0, choices.Length)];
                //chosen.SetColor(aiColor);
                aiPath.Add(chosen);
                //Debug.Log($"Adding {chosen} to AI path");
                targetScore += chosen.value;
                UpdateResult();
                targetScoreLabel.text = $"{targetScore}";
                if (currentNode != rootNode && currentNode != endNode) {
                    currentNode.SetColor(aiColor);
                    currentNode.label.color = Color.white;
                }
                chosen.nodeFilled.enabled = true;
                currentNode.ColorExitLine(chosen, aiColor);
                currentNode = chosen;
                choices = currentNode.exitNodes;
            }
            //Debug.Log($"choices found {choices.Length}");
        }
        state = State.GamePlay;
    }

    private void PickNode(Node pickedNode) {
        PlayRandomNote();
        playerPath.Add(pickedNode);
        Score += pickedNode.value;
        UpdateResult();
        //Score %= scoreMod;
        pickedNode.SetColor(playerColor);
        if (playerCurrentNode != rootNode && playerCurrentNode != endNode) {
            playerCurrentNode.SetColor(playerColor);
        }
        playerCurrentNode.ColorExitLine(pickedNode, playerColor);
        pickedNode.SelectedByPlayer = true;
        pickedNode.label.color = Color.black;
        foreach (var exitNode in playerCurrentNode.exitNodes) {
            exitNode.CanBeSelected = false;
        }
        playerCurrentNode = pickedNode;
        foreach (var exitNode in playerCurrentNode.exitNodes) {
            exitNode.CanBeSelected = true;
        }
        if (playerCurrentNode == endNode) {
            state = State.End;
            FadeToEnd();
        }
        turn++;
    }

    private void PlayRandomNote() {
        int pickedNote = 0;
        do {
            pickedNote = Random.Range(0, notes.Length);
        } while (pickedNote == lastPickedNote);
        notesSequence.Add(pickedNote);
        audioSource.PlayOneShot(notes[pickedNote]);
        lastPickedNote = pickedNote;
    }

    private void OnNodeSelected(Node pickedNode) {
        switch (state) {
            case State.Intro:
                break;
            case State.FirstMove:
                PickNode(pickedNode);
                CalculateAiPath();
                scoreContainer.GetComponent<CanvasGroup>().DOFade(1, 1f);
                break;
            case State.GamePlay:
                PickNode(pickedNode);
                if (turn == 3) {
                    instructionsContainer.GetComponent<CanvasGroup>().DOFade(0, 1f);
                }
                break;
            case State.End:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateResult() {
        int result = Mathf.Abs(5 - (Mathf.Abs(targetScore - score) % scoreMod));
        resultLabel.text = $"{result}";
        StartCoroutine(UpdatingResult(result / 5f));
    }

    private IEnumerator UpdatingResult(float targetValue) {
        float startValue = resultImage.fillAmount;
        float endValue = targetValue;
        float timer = 0;
        float duration = 0.2f;
        while (timer < duration) {
            float t = timer / duration;
            resultImage.fillAmount = Mathf.Lerp(startValue, endValue, t);
            resultImage.color = resultImageGradient.Evaluate(Mathf.Lerp(startValue, endValue, t));
            resultLabel.color = resultImage.color;
            timer += Time.deltaTime;
            yield return null;
        }
        resultImage.fillAmount = targetValue;
        resultImage.color = resultImageGradient.Evaluate(targetValue);
        resultLabel.color = resultImage.color;
    }

    public void Retry() {
        GameLoader.Instance.LoadGameScene();
    }

    public void BackToMenu() {
        GameLoader.Instance.LoadMenuScene();
    }
}