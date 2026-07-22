using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[ExecuteAlways]
[RequireComponent(typeof(XRBaseInteractable))]
public class BoxSelectVisualizerParent : MonoBehaviour
{
    private XRBaseInteractable interactable;

    [Header("Target Setup")]
    [Tooltip("The BoxCollider used as the boundary for the selection visual.")]
    public BoxCollider targetCollider;
    private LineRenderer lineRenderer;

    [Space(10)]
    [Header("Selection State")]
    [Tooltip("Indicates whether this object is currently selected.")]
    public bool isSelected = false;

    [Space(10)]
    [Header("Line Visual Settings")]
    public Color defaultColor = Color.white;
    public Color selectedColor = Color.green;
    [Range(0.001f, 0.1f)]
    public float lineWidth = 0.015f;

    [Space(10)]
    [Header("UI & Typography Settings")]
    [Tooltip("Vertical offset for the information icon.")]
    public float iconYOffset = 0.2f;
    public float iconSize = 1f;
    public Color iconColor = Color.cyan;
    public Vector3 textLocalPosition = new Vector3(0, -0.15f, 0);
    public float textSize = 0.7f;
    public Color textColor = Color.white;

    [Space(10)]
    [Header("Animation Settings")]
    [Tooltip("Duration (in seconds) for the UI fade-in and fade-out effects.")]
    [Range(0.1f, 2f)]
    public float fadeDuration = 0.3f;

    private GameObject generatedContainer;
    private TextMesh iconTextMesh;
    private TextMesh commandTextMesh;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();

        if (targetCollider == null)
        {
            Debug.LogError($"[ERROR] Missing targetCollider on {gameObject.name}!");
            return;
        }

        if (!interactable.colliders.Contains(targetCollider))
        {
            interactable.colliders.Add(targetCollider);
        }

        lineRenderer = targetCollider.gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null) lineRenderer = targetCollider.gameObject.AddComponent<LineRenderer>();

        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        DrawBoxColliderBounds();
        SetLineColor(defaultColor);

        GenerateInfoAndTextFromCode();

        if (Application.isPlaying && generatedContainer != null)
        {
            SetTextAlpha(0f); // Start completely transparent
            generatedContainer.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (Application.isPlaying && interactable != null)
        {
            interactable.selectEntered.AddListener(OnClicked);
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying && interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnClicked);
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (targetCollider != null && lineRenderer != null) DrawBoxColliderBounds();
            UpdatePositions();
        }

        // VR Billboarding: Constrain rotation to the Y-axis (Yaw)
        if (generatedContainer != null && generatedContainer.activeSelf && Camera.main != null)
        {
            Vector3 lookDir = generatedContainer.transform.position - Camera.main.transform.position;
            lookDir.y = 0; // Nullify Pitch/Roll to keep the text upright

            if (lookDir != Vector3.zero)
            {
                generatedContainer.transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
    }

    private void OnClicked(SelectEnterEventArgs args)
    {
        isSelected = !isSelected;
        SetLineColor(isSelected ? selectedColor : defaultColor);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (args.interactorObject is XRGazeInteractor)
        {
            if (generatedContainer != null)
            {
                generatedContainer.SetActive(true);

                // Stop existing animations and initiate Fade In
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeRoutine(1f, false));
            }
        }
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (args.interactorObject is XRGazeInteractor)
        {
            if (generatedContainer != null && !isSelected)
            {
                // Initiate Fade Out and disable upon completion
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeRoutine(0f, true));
            }
        }
    }

    // --- ANIMATION SYSTEM ---
    private IEnumerator FadeRoutine(float targetAlpha, bool disableOnComplete)
    {
        float startAlpha = iconTextMesh != null ? iconTextMesh.color.a : 0f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            SetTextAlpha(newAlpha);
            yield return null;
        }

        SetTextAlpha(targetAlpha); // Snap to the target value

        if (disableOnComplete)
        {
            generatedContainer.SetActive(false);
        }
    }

    private void SetTextAlpha(float alpha)
    {
        if (iconTextMesh != null)
        {
            Color c = iconTextMesh.color;
            c.a = alpha;
            iconTextMesh.color = c;
        }
        if (commandTextMesh != null)
        {
            Color c = commandTextMesh.color;
            c.a = alpha;
            commandTextMesh.color = c;
        }
    }
    // -------------------------

    private void SetLineColor(Color color)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }

    private void DrawBoxColliderBounds()
    {
        Vector3 c = targetCollider.center;
        Vector3 s = targetCollider.size / 2f;

        Vector3[] points = new Vector3[] {
            c + new Vector3(-s.x, -s.y, -s.z), c + new Vector3(-s.x, -s.y, s.z),
            c + new Vector3(s.x, -s.y, s.z), c + new Vector3(s.x, -s.y, -s.z),
            c + new Vector3(-s.x, -s.y, -s.z), c + new Vector3(-s.x, s.y, -s.z),
            c + new Vector3(-s.x, s.y, s.z), c + new Vector3(-s.x, -s.y, s.z),
            c + new Vector3(-s.x, s.y, s.z), c + new Vector3(s.x, s.y, s.z),
            c + new Vector3(s.x, -s.y, s.z), c + new Vector3(s.x, s.y, s.z),
            c + new Vector3(s.x, s.y, -s.z), c + new Vector3(s.x, -s.y, -s.z),
            c + new Vector3(s.x, s.y, -s.z), c + new Vector3(-s.x, s.y, -s.z)
        };

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }

    private void GenerateInfoAndTextFromCode()
    {
        Transform existingContainer = transform.Find("DynamicInfoContainer");
        if (existingContainer != null) DestroyImmediate(existingContainer.gameObject);

        generatedContainer = new GameObject("DynamicInfoContainer");
        generatedContainer.transform.SetParent(this.transform);

        GameObject iconObj = new GameObject("IconText");
        iconObj.transform.SetParent(generatedContainer.transform);
        iconTextMesh = iconObj.AddComponent<TextMesh>();
        iconTextMesh.anchor = TextAnchor.MiddleCenter;
        iconTextMesh.alignment = TextAlignment.Center;
        iconTextMesh.text = "ℹ";

        GameObject cmdObj = new GameObject("CommandText");
        cmdObj.transform.SetParent(generatedContainer.transform);
        commandTextMesh = cmdObj.AddComponent<TextMesh>();
        commandTextMesh.anchor = TextAnchor.UpperCenter;
        commandTextMesh.alignment = TextAlignment.Center;

        string displayText = "🎤 Commands:\n";

        VoiceInteractable voiceCmd = GetComponent<VoiceInteractable>();

        if (voiceCmd != null)
        {
            string availableCmds = voiceCmd.GetAvailableCommandsText();

            if (!string.IsNullOrEmpty(availableCmds))
            {
                displayText += availableCmds;
            }
            else
            {
                displayText += "<color=gray>No interactions.</color>";
            }
        }
        else
        {
            displayText += "<color=red>No Voice Module.</color>";
        }

        if (iconTextMesh != null)
        {
            iconTextMesh.fontSize = 60;
            iconTextMesh.characterSize = 0.03f * iconSize;
            iconTextMesh.color = iconColor;
        }

        if (commandTextMesh != null)
        {
            commandTextMesh.text = displayText;
            commandTextMesh.fontSize = 40;
            commandTextMesh.characterSize = 0.02f * textSize;
            commandTextMesh.color = textColor;
        }

        UpdatePositions();
    }

    private void UpdatePositions()
    {
        if (generatedContainer != null && targetCollider != null)
        {
            Bounds bounds = targetCollider.bounds;
            generatedContainer.transform.position = bounds.center + new Vector3(0, bounds.extents.y + iconYOffset, 0);

            if (iconTextMesh != null) iconTextMesh.transform.localPosition = Vector3.zero;
            if (commandTextMesh != null) commandTextMesh.transform.localPosition = textLocalPosition;
        }
    }
}