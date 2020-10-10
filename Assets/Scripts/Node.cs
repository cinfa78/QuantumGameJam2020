using System;
using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.U2D;

public class Node : MonoBehaviour, IPointerClickHandler {
    public int value;

    [ColorUsage(true, true)] public Color color;
    public Node[] exitNodes;
    [FormerlySerializedAs("meshRenderer")] [FormerlySerializedAs("spriteRenderer")]
    public MeshRenderer nodeOutline;
    public MeshRenderer nodeFilled;
    public MeshRenderer nodeGlow;
    [FormerlySerializedAs("text")] public TMP_Text label;
    public GameObject graphics;
    public GameObject linePrefab;
    private SpriteShapeController[] exitingLines;
    public bool endNode;
    [ReadOnly] private bool canBeSelected;
    public event Action<Node> Selected;
    public event Action<Node> Showed;
    
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
    private bool selectedByPlayer;
    public bool SelectedByPlayer {
        get => selectedByPlayer;
        set {
            selectedByPlayer = value;
            if (selectedByPlayer) {
                ToggleFilled();
            }
        }
    }

    private void Awake() {
        nodeOutline.material = Instantiate(nodeOutline.material);
        nodeOutline.material.color = color;
        nodeOutline.material.SetColor("_Color", color);
        nodeFilled.material = Instantiate(nodeFilled.material);
        nodeFilled.enabled = false;
        nodeGlow.enabled = false;
        label.text = value > 0 ? $"{value}" : "";
        label.enabled = false;
    }

    private void Start() {
        exitingLines = new SpriteShapeController[exitNodes.Length];
        int i = 0;
        foreach (var exitNode in exitNodes) {
            var newSpriteShape = Instantiate(linePrefab, graphics.transform);
            newSpriteShape.transform.position = transform.position + Vector3.forward;
            SpriteShapeController spriteShape = newSpriteShape.GetComponent<SpriteShapeController>();
            spriteShape.spline.SetPosition(1, exitNode.transform.position - graphics.transform.position + Vector3.forward);
            exitingLines[i] = newSpriteShape.GetComponent<SpriteShapeController>();
            i++;
        }
        graphics.SetActive(false);
        foreach (SpriteShapeController spriteShapeController in exitingLines) {
            spriteShapeController.enabled = false;
            spriteShapeController.spline.SetPosition(1, Vector3.forward * 4);
        }
        label.enabled = false;
    }

    public void ToggleValue() {
        label.enabled = !label.enabled;
        if (label.enabled) {
            label.text = value > 0 ? $"{value}" : "";
        }
    }

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
        foreach (SpriteShapeController spriteShapeController in exitingLines) {
            spriteShapeController.enabled = show;
            spriteShapeController.spline.SetPosition(1, Vector3.forward * 4);
        }
        float timer = 0;
        const float duration = 1f;
        while (timer < duration) {
            float t = timer / duration;
            i = 0;
            foreach (var exitNode in exitNodes) {
                exitingLines[i].spline.SetPosition(1, Vector3.Lerp(Vector3.forward * 4, exitNode.transform.position - graphics.transform.position + Vector3.forward, t));
                i++;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        i = 0;
        foreach (var exitNode in exitNodes) {
            exitingLines[i].spline.SetPosition(1, exitNode.transform.position - graphics.transform.position + Vector3.forward);
            i++;
        }
        if (!endNode) {
            ToggleValue();
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
        label.text = $"{value}";
    }
#endif

    public void OnPointerClick(PointerEventData eventData) {
        if (CanBeSelected) {
            Selected?.Invoke(this);
            SelectedByPlayer = true;
        }
    }
}