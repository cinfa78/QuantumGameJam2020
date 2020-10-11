using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.U2D;

public class Node : MonoBehaviour, IPointerClickHandler {
    public int value;

    [Serializable]
    public struct Arc {
        public Node source;
        public Node target;
        public SpriteShapeController spriteShapeController;
    }

    [ColorUsage(true, true)] public Color color;
    [ColorUsage(true, true)] public Color arcColor;
    public Node[] exitNodes;
    public Arc[] exitArcs;
    [FormerlySerializedAs("meshRenderer")] [FormerlySerializedAs("spriteRenderer")]
    public MeshRenderer nodeOutline;
    public MeshRenderer nodeFilled;
    public MeshRenderer nodeGlow;
    [FormerlySerializedAs("text")] public TMP_Text label;
    public GameObject graphics;
    public GameObject linePrefab;
    //private SpriteShapeController[] exitingLines;
    public bool endNode;
    public float showTime = 0.1f;
    [ReadOnly, SerializeField] private bool canBeSelected;
    public bool CanBeSelected {
        get => canBeSelected;
        set {
            canBeSelected = value;
            if (canBeSelected) {
                nodeOutline.enabled = true;
            }
            else {
                nodeOutline.enabled = false;
            }
        }
    }
    [ReadOnly, SerializeField] private bool selectedByPlayer;
    public bool SelectedByPlayer {
        get => selectedByPlayer;
        set {
            selectedByPlayer = value;
            nodeFilled.enabled = selectedByPlayer;
        }
    }

    public event Action<Node> Selected;
    public event Action<Node> Showed;

    private void Awake() {
        nodeOutline.material = Instantiate(nodeOutline.material);
        nodeOutline.material.color = color;
        nodeOutline.material.SetColor("_Color", new Color(2, 2, 2, 1));
        nodeFilled.material = Instantiate(nodeFilled.material);
        nodeFilled.enabled = false;
        nodeGlow.enabled = false;
        label.text = value > 0 ? $"{value}" : "";
        label.enabled = false;
    }

    private void Start() {
        exitArcs = new Arc[exitNodes.Length];
        //exitingLines = new SpriteShapeController[exitNodes.Length];
        int i = 0;
        foreach (var exitNode in exitNodes) {
            var newSpriteShape = Instantiate(linePrefab, transform);
            newSpriteShape.transform.position = transform.position + Vector3.forward;
            SpriteShapeController spriteShape = newSpriteShape.GetComponent<SpriteShapeController>();
            newSpriteShape.GetComponent<SpriteShapeRenderer>().materials[1] = Instantiate(newSpriteShape.GetComponent<SpriteShapeRenderer>().materials[1]);
            newSpriteShape.GetComponent<SpriteShapeRenderer>().materials[1].color = arcColor;
            spriteShape.spline.SetPosition(1, exitNode.transform.position - transform.position + Vector3.forward);
            exitArcs[i] = new Arc {source = this, target = exitNode, spriteShapeController = spriteShape};
            //exitingLines[i] = newSpriteShape.GetComponent<SpriteShapeController>();
            i++;
        }
        graphics.SetActive(false);
        foreach (var arc in exitArcs) {
            var spriteShapeController = arc.spriteShapeController;
            spriteShapeController.enabled = false;
            spriteShapeController.spline.SetPosition(1, Vector3.forward * 4);
        }
        label.enabled = false;
    }
    //
    // public void ToggleValue() {
    //     label.enabled = !label.enabled;
    //     if (label.enabled) {
    //         label.text = value > 0 ? $"{value}" : "";
    //     }
    // }

    public void SetColor(Color color) {
        nodeFilled.material.color = color;
    }

    public void ToggleGlow() {
        nodeGlow.enabled = !nodeGlow.enabled;
    }

    public void ToggleFilled() {
        nodeFilled.enabled = !nodeFilled.enabled;
    }

    public void ShowNode(bool show) {
        StartCoroutine(Showing(show));
    }

    private IEnumerator Showing(bool show) {
        int i = 0;

        graphics.SetActive(show);
        foreach (Arc arc in exitArcs) {
            var spriteShapeController = arc.spriteShapeController;
            spriteShapeController.enabled = show;
            spriteShapeController.spline.SetPosition(1, Vector3.forward * 4);
        }
        float timer = 0;
        float duration = showTime;
        while (timer < duration) {
            float t = timer / duration;
            i = 0;
            foreach (var arc in exitArcs) {
                arc.spriteShapeController.spline.SetPosition(1,
                    Vector3.Lerp(Vector3.forward * 4, (arc.target.transform.position - arc.source.transform.position) + Vector3.forward, Mathf.Sin(t * Mathf.PI * 0.5f)));
                i++;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        i = 0;
        foreach (var arc in exitArcs) {
            arc.spriteShapeController.spline.SetPosition(1, (arc.target.transform.position - arc.source.transform.position) + Vector3.forward);
            i++;
        }
        label.enabled = !endNode;
        if (label.enabled) {
            label.GetComponent<CanvasGroup>().alpha = 0;
            label.GetComponent<CanvasGroup>().DOFade(1, 0.2f);
        }
        Showed?.Invoke(this);
        foreach (var node in exitNodes) {
            node.ShowNode(show);
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos() {
        Handles.Label(transform.position, $"{value}");
        foreach (var exitNode in exitNodes) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, exitNode.transform.position);
        }
        label.text = value > 0 ? $"{value}" : "";
    }
#endif

    public void OnPointerClick(PointerEventData eventData) {
        if (CanBeSelected) {
            CanBeSelected = false;
            SelectedByPlayer = true;
            Selected?.Invoke(this);
        }
    }

    public void ColorExitLine(Node target, Color newColor) {
        foreach (var arc in exitArcs) {
            if (arc.target == target) {
                arc.spriteShapeController.GetComponent<SpriteShapeRenderer>().materials[1].color = newColor;
                break;
            }
        }
    }
}