using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class SKPProtractorGizmo : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Transform targetObject;
    public float snapAngle = 5f;
    public float radius = 1.2f;

    [Header("VR Connection (REQUIRED)")]
    [Tooltip("Drag the Right Hand Controller under XR Origin here")]
    public Transform rightControllerTransform;

    [Header("Visuals & Colors")]
    public Color faceColor = new Color(0.2f, 0.6f, 1f, 0.15f);
    public Color edgeColor = new Color(0.2f, 0.6f, 1f, 0.8f);
    public Color tickColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

    [Header("Tick Dimensions")]
    public float majorTickLength = 0.15f;
    public float minorTickLength = 0.05f;

    private LineRenderer baseCircle;
    private TextMeshPro angleText;
    private float _cachedRadius;

    private InputAction rightTriggerAction;
    private bool isRotating = false;
    private float initialHitAngle;
    private float initialObjectRotationY;
    private Vector3 centerPoint;

    private void Awake()
    {
        baseCircle = GetComponent<LineRenderer>();

        if (Application.isPlaying)
        {
            if (targetObject == null && transform.parent != null)
            {
                targetObject = transform.parent;
            }

            SetupRuntimeVisuals();
            rightTriggerAction = new InputAction(name: "RightTrigger", type: InputActionType.Button, binding: "<XRController>{RightHand}/triggerButton");
        }
        else
        {
            DrawDetailedProtractor();
            _cachedRadius = radius;
        }
    }

    private void OnEnable()
    {
        if (Application.isPlaying && rightTriggerAction != null)
        {
            rightTriggerAction.Enable();
            rightTriggerAction.started += OnTriggerPressed;
            rightTriggerAction.canceled += OnTriggerReleased;
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying && rightTriggerAction != null)
        {
            rightTriggerAction.Disable();
            rightTriggerAction.started -= OnTriggerPressed;
            rightTriggerAction.canceled -= OnTriggerReleased;
            isRotating = false;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (Mathf.Abs(radius - _cachedRadius) > 0.001f)
            {
                DrawDetailedProtractor();
                _cachedRadius = radius;
            }
        }
        else
        {
            HandleRotation();
        }
    }

    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        if (targetObject == null) return;

        if (rightControllerTransform == null)
        {
            Debug.LogError("[SKP Protractor] ERROR: Right hand not assigned! Please drag your right hand object into the 'Right Controller Transform' field in the Inspector.");
            return;
        }

        if (!gameObject.activeInHierarchy) return;

        isRotating = true;
        centerPoint = transform.position;

        Vector3 handPos = rightControllerTransform.position;
        Vector3 flattenedHand = new Vector3(handPos.x, centerPoint.y, handPos.z);
        Vector3 dir = flattenedHand - centerPoint;

        initialHitAngle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        initialObjectRotationY = targetObject.eulerAngles.y;

        if (angleText != null) angleText.text = "0°";
    }

    private void OnTriggerReleased(InputAction.CallbackContext context)
    {
        isRotating = false;

        if (angleText != null) angleText.text = "";

        gameObject.SetActive(false);
    }

    private void HandleRotation()
    {
        if (angleText != null && Camera.main != null)
        {
            angleText.transform.rotation = Camera.main.transform.rotation;
        }

        if (isRotating && targetObject != null && rightControllerTransform != null)
        {
            Vector3 handPos = rightControllerTransform.position;
            Vector3 currentHit = new Vector3(handPos.x, centerPoint.y, handPos.z);
            Vector3 dir = currentHit - centerPoint;

            float currentAngle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
            float deltaAngle = currentAngle - initialHitAngle;

            if (deltaAngle > 180) deltaAngle -= 360;
            if (deltaAngle < -180) deltaAngle += 360;

            float rawDesiredRot = initialObjectRotationY - deltaAngle;
            float finalSnappedRot = Mathf.Round(rawDesiredRot / snapAngle) * snapAngle;

            targetObject.rotation = Quaternion.Euler(targetObject.eulerAngles.x, finalSnappedRot, targetObject.eulerAngles.z);

            if (angleText != null)
            {
                float actualDelta = initialObjectRotationY - finalSnappedRot;
                string sign = actualDelta > 0 ? "+" : (actualDelta < 0 ? "-" : "");
                angleText.text = sign + Mathf.Abs(actualDelta).ToString("F0") + "°";
            }
        }
    }

    private void SetupRuntimeVisuals()
    {
        DrawDetailedProtractor();

        if (angleText == null)
        {
            Transform textTrans = transform.Find("AngleText");
            GameObject textObj = textTrans != null ? textTrans.gameObject : new GameObject("AngleText");

            if (textTrans == null)
            {
                textObj.transform.SetParent(transform);
                textObj.transform.localPosition = new Vector3(0, 0.4f, 0);
            }

            angleText = textObj.GetComponent<TextMeshPro>();
            if (angleText == null) angleText = textObj.AddComponent<TextMeshPro>();

            angleText.alignment = TextAlignmentOptions.Center;
            angleText.fontSize = 5;
            angleText.color = Color.yellow;
            angleText.text = "";
        }
    }

    private void DrawDetailedProtractor()
    {
        if (baseCircle == null)
        {
            baseCircle = GetComponent<LineRenderer>();
            if (baseCircle == null) baseCircle = gameObject.AddComponent<LineRenderer>();
        }

        baseCircle.useWorldSpace = false;
        baseCircle.loop = true;
        baseCircle.startWidth = 0.015f;
        baseCircle.endWidth = 0.015f;

        if (baseCircle.sharedMaterial == null)
            baseCircle.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

        baseCircle.startColor = edgeColor;
        baseCircle.endColor = edgeColor;

        int segments = 64;
        baseCircle.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float rad = Mathf.Deg2Rad * (i * 360f / segments);
            baseCircle.SetPosition(i, new Vector3(Mathf.Cos(rad) * radius, 0, Mathf.Sin(rad) * radius));
        }

        Transform visualsTrans = transform.Find("ProtractorVisuals");
        if (visualsTrans == null)
        {
            GameObject visualsObj = new GameObject("ProtractorVisuals");
            visualsObj.transform.SetParent(transform, false);
            visualsTrans = visualsObj.transform;
        }

        BuildFaceMesh(visualsTrans);
        BuildTicksMesh(visualsTrans);
    }

    private void BuildFaceMesh(Transform parent)
    {
        Transform faceTrans = parent.Find("Face");
        if (faceTrans == null)
        {
            GameObject faceObj = new GameObject("Face");
            faceObj.transform.SetParent(parent, false);
            faceTrans = faceObj.transform;
        }

        MeshFilter mf = faceTrans.GetComponent<MeshFilter>();
        if (mf == null) mf = faceTrans.gameObject.AddComponent<MeshFilter>();

        MeshRenderer mr = faceTrans.GetComponent<MeshRenderer>();
        if (mr == null) mr = faceTrans.gameObject.AddComponent<MeshRenderer>();

        if (mr.sharedMaterial == null) mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        mr.sharedMaterial.color = faceColor;

        int segments = 64;
        Vector3[] verts = new Vector3[segments + 1];
        int[] tris = new int[segments * 3];

        verts[0] = Vector3.zero;
        for (int i = 0; i < segments; i++)
        {
            float rad = Mathf.Deg2Rad * (i * 360f / segments);
            verts[i + 1] = new Vector3(Mathf.Cos(rad) * radius, 0, Mathf.Sin(rad) * radius);
        }

        for (int i = 0; i < segments; i++)
        {
            tris[i * 3] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = (i + 1 == segments) ? 1 : i + 2;
        }

        Mesh mesh = new Mesh { name = "ProtractorFace" };
        mesh.vertices = verts;
        mesh.triangles = tris;
        mf.sharedMesh = mesh;
    }

    private void BuildTicksMesh(Transform parent)
    {
        Transform ticksTrans = parent.Find("Ticks");
        if (ticksTrans == null)
        {
            GameObject ticksObj = new GameObject("Ticks");
            ticksObj.transform.SetParent(parent, false);
            ticksTrans = ticksObj.transform;
        }

        MeshFilter mf = ticksTrans.GetComponent<MeshFilter>();
        if (mf == null) mf = ticksTrans.gameObject.AddComponent<MeshFilter>();

        MeshRenderer mr = ticksTrans.GetComponent<MeshRenderer>();
        if (mr == null) mr = ticksTrans.gameObject.AddComponent<MeshRenderer>();

        if (mr.sharedMaterial == null) mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        mr.sharedMaterial.color = tickColor;

        int numTicks = 360 / 5;
        Vector3[] verts = new Vector3[numTicks * 2];
        int[] indices = new int[numTicks * 2];

        for (int i = 0; i < numTicks; i++)
        {
            float angle = i * 5f;
            float rad = angle * Mathf.Deg2Rad;

            float currentTickLength = (i % 3 == 0) ? majorTickLength : minorTickLength;

            verts[i * 2] = new Vector3(Mathf.Cos(rad) * radius, 0, Mathf.Sin(rad) * radius);
            verts[i * 2 + 1] = new Vector3(Mathf.Cos(rad) * (radius - currentTickLength), 0, Mathf.Sin(rad) * (radius - currentTickLength));

            indices[i * 2] = i * 2;
            indices[i * 2 + 1] = i * 2 + 1;
        }

        Mesh mesh = new Mesh { name = "ProtractorTicks" };
        mesh.vertices = verts;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        mf.sharedMesh = mesh;
    }
}