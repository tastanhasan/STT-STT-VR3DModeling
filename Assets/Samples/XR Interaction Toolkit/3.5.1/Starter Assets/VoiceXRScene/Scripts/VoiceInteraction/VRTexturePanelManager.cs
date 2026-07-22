using UnityEngine;

public class VRTexturePanelManager : MonoBehaviour
{
    public static VRTexturePanelManager Instance { get; private set; }

    [Header("Arayüz Paneli")]
    [Tooltip("Sol el kontrolcüsünün altýndaki Doku Seçim Canvas'ý")]
    public GameObject textureUIPanel;

    [Header("Doku (Texture) Kütüphanesi")]
    [Tooltip("Arayüzdeki butonlarýn sýrasýna göre atayacaðýnýz 2D görseller")]
    public Texture2D[] availableTextures;

    private GameObject currentTargetObject;

    private void Awake()
    {
        Debug.Log("[VRTexturePanelManager - AWAKE] Awake tetiklendi.");
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[VRTexturePanelManager - AWAKE] Instance baþarýyla atandý.");
        }
        else
        {
            Debug.LogError("[VRTexturePanelManager - AWAKE] Sahnede birden fazla Instance var, bu obje yok ediliyor!");
            Destroy(gameObject);
        }

        if (textureUIPanel != null)
        {
            textureUIPanel.SetActive(false);
            Debug.Log("[VRTexturePanelManager - AWAKE] Panel baþlangýçta gizlendi (SetActive = false).");
        }
        else
        {
            Debug.LogError("[VRTexturePanelManager - AWAKE] textureUIPanel atanmamýþ! Inspector'ý kontrol et.");
        }
    }

    public void OpenPanel(GameObject target)
    {
        Debug.Log($"[VRTexturePanelManager - OPENPANEL] Fonksiyon çaðrýldý! Gelen hedef obje: {(target != null ? target.name : "NULL")}");
        currentTargetObject = target;

        if (textureUIPanel != null)
        {
            textureUIPanel.SetActive(true);
            Debug.Log($"[VRTexturePanelManager - OPENPANEL] Panel AÇILDI (SetActive = true). Hedef: {target.name}");
        }
        else
        {
            Debug.LogError("[VRTexturePanelManager - OPENPANEL] Hata: textureUIPanel referansý yok, panel açýlamadý!");
        }
    }

    public void ApplyTextureToTarget(int textureIndex)
    {
        Debug.Log($"[VRTexturePanelManager - APPLY] ApplyTextureToTarget çaðrýldý. Gelen Ýndeks: {textureIndex}");

        if (currentTargetObject == null)
        {
            Debug.LogWarning("[VRTexturePanelManager - APPLY] Ýptal: currentTargetObject NULL! Malzeme uygulanacak obje yok.");
            return;
        }

        if (textureIndex < 0 || textureIndex >= availableTextures.Length)
        {
            Debug.LogError($"[VRTexturePanelManager - APPLY] Ýptal: Geçersiz doku indeksi! Ýstlenen: {textureIndex}, Toplam Doku: {availableTextures.Length}");
            return;
        }

        Texture2D selectedTexture = availableTextures[textureIndex];

        // --- YENÝ EKLENEN KISIM: Dokunun oranýný hesapla ---
        // Örneðin 2048x1024 bir doku ise oran 2.0 olacaktýr.
        float aspectRatio = (float)selectedTexture.width / selectedTexture.height;

        // Orana göre Tiling (Scale) vektörü oluþturuyoruz.
        // Geniþlik yükseklikten büyükse X ekseninde, deðilse Y ekseninde ölçekleme yapar.
        Vector2 textureScale = new Vector2(
            aspectRatio >= 1f ? aspectRatio : 1f,
            aspectRatio < 1f ? 1f / aspectRatio : 1f
        );
        // ----------------------------------------------------

        Debug.Log($"[VRTexturePanelManager - APPLY] Seçilen doku: {selectedTexture.name}, Hesaplanýlan Oran: {textureScale}. MeshRenderer aranýyor...");

        MeshRenderer[] targetRenderers;
        MeshRenderer singleRenderer = currentTargetObject.GetComponent<MeshRenderer>();

        if (singleRenderer != null)
        {
            targetRenderers = new MeshRenderer[] { singleRenderer };
        }
        else
        {
            targetRenderers = currentTargetObject.GetComponentsInChildren<MeshRenderer>();
        }

        if (targetRenderers.Length > 0)
        {
            foreach (var renderer in targetRenderers)
            {
                // --- YENÝ EKLENEN KORUMA KALKANI ---
                // Eðer bu MeshRenderer bir 3D Yazýya (TextMesh) aitse, dokunma ve sýradaki objeye geç!
                if (renderer.GetComponent<TextMesh>() != null)
                {
                    continue;
                }
                // -----------------------------------
                Material[] objectMaterials = renderer.materials;

                foreach (var mat in objectMaterials)
                {
                    if (mat == null) continue;

                    // Doku Atamalarý
                    mat.SetTexture("_BaseMap", selectedTexture);
                    mat.SetTexture("_MainTex", selectedTexture);

                    // --- YENÝ EKLENEN KISIM: Tiling (Ölçek) Atamalarý ---
                    if (mat.HasProperty("_BaseMap")) mat.SetTextureScale("_BaseMap", textureScale);
                    if (mat.HasProperty("_MainTex")) mat.SetTextureScale("_MainTex", textureScale);
                    // ----------------------------------------------------

                    // GLTF Specular/Glossiness ShaderGraph Özel Atamasý
                    if (mat.HasProperty("diffuseTexture"))
                    {
                        mat.SetTexture("diffuseTexture", selectedTexture);
                        mat.SetTextureScale("diffuseTexture", textureScale);
                    }
                    if (mat.HasProperty("baseColorTexture"))
                    {
                        mat.SetTexture("baseColorTexture", selectedTexture);
                        mat.SetTextureScale("baseColorTexture", textureScale);
                    }

                    // GLTF Renk (Tint) Sýfýrlama
                    if (mat.HasProperty("diffuseFactor")) mat.SetColor("diffuseFactor", Color.white);
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
                    if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
                }
                renderer.materials = objectMaterials;
            }

            ClosePanel();
        }
        else
        {
            Debug.LogError($"[VRTexturePanelManager - APPLY] HATA: {currentTargetObject.name} veya alt objelerinde hiçbir MeshRenderer bulunamadý!");
        }
    }

    public void ClosePanel()
    {
        Debug.Log("[VRTexturePanelManager - CLOSEPANEL] Fonksiyon çaðrýldý!");
        if (textureUIPanel != null)
        {
            textureUIPanel.SetActive(false);
            Debug.Log("[VRTexturePanelManager - CLOSEPANEL] Panel KAPATILDI (SetActive = false).");
        }
        currentTargetObject = null;
        Debug.Log("[VRTexturePanelManager - CLOSEPANEL] currentTargetObject NULL olarak sýfýrlandý.");
    }
}