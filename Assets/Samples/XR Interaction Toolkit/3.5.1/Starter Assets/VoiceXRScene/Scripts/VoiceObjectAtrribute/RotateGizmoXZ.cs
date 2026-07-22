using GLTFast.Schema;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RotateGizmoXZ : XRBaseInteractable
{
    [Header("Döndürme Ayarlarý")]
    public Transform targetObject;
    public float snapAngle = 5f;
    public float radius = 1.2f;

    // Görsel Bileþenler (SKP Ýletkisi için)
    private LineRenderer circleRenderer;  // Ana çember
    private LineRenderer angleRenderer;   // Açý referans çizgileri (V Þekli)
    private BoxCollider dragCollider;

    // Hesaplama Deðiþkenleri
    private IXRSelectInteractor currentInteractor;
    private float initialHitAngle;
    private float initialObjectRotationY;
    private Vector3 centerPoint;
    private Vector3 startHitPoint;

    protected override void Awake()
    {
        base.Awake();
        SetupGizmoVisuals();
    }

    private void SetupGizmoVisuals()
    {
        // 1. ANA ÇEMBERÝ OLUÞTUR (Mavi/Turkuaz SKP Rengi)
        GameObject circleObj = new GameObject("GizmoCircle");
        circleObj.transform.SetParent(transform);
        circleObj.transform.localPosition = Vector3.zero;

        circleRenderer = circleObj.AddComponent<LineRenderer>();
        circleRenderer.useWorldSpace = false;
        circleRenderer.loop = true;
        circleRenderer.startWidth = 0.015f;
        circleRenderer.endWidth = 0.015f;
        circleRenderer.material = new UnityEngine.Material(Shader.Find("Sprites/Default"));
        circleRenderer.startColor = new Color(0.2f, 0.8f, 1f, 0.7f);
        circleRenderer.endColor = new Color(0.2f, 0.8f, 1f, 0.7f);

        int segments = 64;
        circleRenderer.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float rad = Mathf.Deg2Rad * (i * 360f / segments);
            circleRenderer.SetPosition(i, new Vector3(Mathf.Cos(rad) * radius, 0, Mathf.Sin(rad) * radius));
        }

        // 2. AÇI GÖSTERGE ÇÝZGÝLERÝNÝ OLUÞTUR (Baþlangýçta Kapalý - Sarý SKP Rengi)
        GameObject angleObj = new GameObject("AngleLines");
        angleObj.transform.SetParent(transform);
        angleObj.transform.localPosition = Vector3.zero;

        angleRenderer = angleObj.AddComponent<LineRenderer>();
        angleRenderer.useWorldSpace = true; // Elimizin dünya koordinatýný takip edeceði için true
        angleRenderer.positionCount = 3;    // [Baþlangýç Noktasý] -> [Merkez] -> [Mevcut El Noktasý]
        angleRenderer.startWidth = 0.02f;
        angleRenderer.endWidth = 0.02f;
        angleRenderer.material = new UnityEngine.Material(Shader.Find("Sprites/Default"));
        angleRenderer.startColor = Color.yellow;
        angleRenderer.endColor = Color.yellow;
        angleRenderer.enabled = false;

        // 3. ETKÝLEÞÝM ÝÇÝN ÇARPIÞMA KUTUSU (Disk)
        dragCollider = gameObject.AddComponent<BoxCollider>();
        dragCollider.size = new Vector3(radius * 2.2f, 0.05f, radius * 2.2f);
    }

    // Tetiðe Basýlýp Gizmo Tutulduðunda (SKP'de ilk týklama aný)
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        currentInteractor = args.interactorObject;

        centerPoint = transform.position;
        Vector3 interactorPos = currentInteractor.transform.position;

        // Elimizin gizmo hizasýndaki (Y ekseni düzlenmiþ) noktasýný bul
        startHitPoint = new Vector3(interactorPos.x, centerPoint.y, interactorPos.z);

        Vector3 dir = startHitPoint - centerPoint;
        initialHitAngle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;

        if (targetObject != null)
        {
            initialObjectRotationY = targetObject.eulerAngles.y;
        }

        // Referans çizgilerini görünür yap ve baþlangýç rotasýný çiz
        angleRenderer.enabled = true;
        angleRenderer.SetPosition(0, startHitPoint); // Sabit Baþlangýç Çizgisi
        angleRenderer.SetPosition(1, centerPoint);   // Merkez Noktasý
        angleRenderer.SetPosition(2, startHitPoint); // Hareketli Çizgi (Þu an baþlangýçta)
    }

    // Tetik Býrakýldýðýnda
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        currentInteractor = null;

        // Referans çizgilerini gizle
        angleRenderer.enabled = false;
    }

    // Basýlý Tutarak Hareket Ettirildiðinde (Döndürme Ýþlemi)
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase phase)
    {
        base.ProcessInteractable(phase);

        if (isSelected && currentInteractor != null && phase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (targetObject == null) return;

            Vector3 interactorPos = currentInteractor.transform.position;
            Vector3 currentHitPoint = new Vector3(interactorPos.x, centerPoint.y, interactorPos.z);
            Vector3 dir = currentHitPoint - centerPoint;

            float currentAngle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
            float deltaAngle = currentAngle - initialHitAngle;

            // 5'er Dereceye Snap (Yuvarlama)
            float desiredRotation = initialObjectRotationY - deltaAngle;
            float snappedRotation = Mathf.Round(desiredRotation / snapAngle) * snapAngle;

            targetObject.rotation = Quaternion.Euler(
                targetObject.eulerAngles.x,
                snappedRotation,
                targetObject.eulerAngles.z
            );

            // SKP Görselliði: Ýkinci çizgiyi elimizin mevcut açýsýna göre uzat
            // (Matematiksel olarak snap edilmiþ açýyý görselleþtiriyoruz)
            float visualDelta = initialObjectRotationY - snappedRotation;
            float visualRad = (initialHitAngle - visualDelta) * Mathf.Deg2Rad;
            Vector3 snappedHitPoint = centerPoint + new Vector3(Mathf.Cos(visualRad) * radius, 0, Mathf.Sin(visualRad) * radius);

            angleRenderer.SetPosition(2, snappedHitPoint);
        }
    }
}