using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
//using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
    [Serializable]
    public enum State {
        Intro = 0,
        FirstMove = 1,
        GamePlay = 2,
        End = 3
    }

    [ColorUsage(false, true)] public Color playerColor;
    [ColorUsage(false, true)] public Color aiColor;
    [ReadOnly] public State state;
    public CanvasGroup inGameBackground;
    public CanvasGroup startBackground;
    public CanvasGroup endBackground;
    public float fadeTime = 2;

    public MeshRenderer leafMeshRenderer;
    public Node rootNode;
    private Node endNode;
    private Node[] nodes;
    [ReadOnly] private int turn;
    [ReadOnly] public int score;
    [ReadOnly] public int targetScore;
    [SerializeField, ReadOnly] private List<Node> aiPath;
    [SerializeField, ReadOnly] private List<Node> playerPath;
    [SerializeField, ReadOnly] private Node playerCurrentNode;

    public event Action Faded;

    private void Awake() {
        turn = 0;
        startBackground.alpha = 1;
        inGameBackground.alpha = 0;
        endBackground.alpha = 0;
        state = State.Intro;
        aiPath = new List<Node>();
    }

    private void Start() {
        nodes = FindObjectsOfType<Node>();
        foreach (Node node in nodes) {
            node.Selected += PlayerPickedNode;
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
        FadeToGame();
    }

    private void OnEndNodeShowed(Node obj) {
        state = State.FirstMove;
    }

    private void StartGame() {
        Faded -= StartGame;
        rootNode.ShowNode(true);
        playerCurrentNode = rootNode;
        playerPath.Add(rootNode);
        foreach (var exitNode in playerCurrentNode.exitNodes) {
            exitNode.CanBeSelected = true;
        }
    }

    public void FadeToGame() {
        StartCoroutine(FadingBackground(startBackground, inGameBackground));
    }

    public void FadeToEnd() {
        StartCoroutine(FadingBackground(inGameBackground, endBackground));
        StartCoroutine(ColoringLeaf());
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
        Node[] choices = new Node[currentNode.exitNodes.Length - 1];
        int i = 0;
        foreach (var exitNode in currentNode.exitNodes) {
            if (!exitNode.SelectedByPlayer) {
                choices[i] = exitNode;
                i++;
            }
        }
        while (currentNode != endNode) {
            Node chosen = choices[Random.Range(0, choices.Length)];
            chosen.SetColor(aiColor);
            aiPath.Add(chosen);
            targetScore += chosen.value;
            currentNode = chosen;
            choices = currentNode.exitNodes;
        }
    }

    private void PlayerPickedNode(Node pickedNode) {
        switch (state) {
            case State.Intro:
                break;
            case State.FirstMove:
                playerPath.Add(pickedNode);
                pickedNode.SetColor(playerColor);
                foreach (var exitNode in playerCurrentNode.exitNodes) {
                    exitNode.CanBeSelected = false;
                }
                playerCurrentNode = pickedNode;
                foreach (var exitNode in playerCurrentNode.exitNodes) {
                    exitNode.CanBeSelected = true;
                }
                CalculateAiPath();
                break;
            case State.GamePlay:
                break;
            case State.End:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}