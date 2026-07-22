using UnityEngine;

[ExecuteAlways] // Oyun kapalýyken bile editörde canlý güncellenmesini saðlar
public class GLTFMaterialModifier : MonoBehaviour
{
    [Header("PBR Ayarlarý (Canlý Önizleme)")]
    [Range(0f, 1f)] public float glossiness = 0.5f;
    [Range(0f, 1f)] public float metallic = 0.0f;

    [Header("Doku Ayarý")]
    public Texture2D customTexture;

    // Editör modunda Inspector'dan bir slider veya deðer deðiþtirdiðin an tetiklenir
    private void OnValidate()
    {
        ApplyProperties();
    }

    private void Update()
    {
        // Oyun baþlamadýysa editör modunda sürekli çizimi canlý tutmak için
        if (!Application.isPlaying)
        {
            ApplyProperties();
        }
    }

    /// <summary>
    /// Runtime'da doku butonuna basýldýðýnda çaðrýlacak olan fonksiyon
    /// </summary>
    public void SetTextureRuntime(Texture2D newTex)
    {
        customTexture = newTex;
        ApplyProperties();
    }

    public void ApplyProperties()
    {
        // Model child (alt) objelerde olduðu için hepsini tarýyoruz
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

        if (renderers.Length == 0) return;

        foreach (var renderer in renderers)
        {
            // Editör modunda hafýza sýzýntýsý (Leak) olmamasý için sharedMaterials, 
            // Runtime'da ise orijinali bozmamak için kopyalanmýþ .materials kullanýlýr.
            Material[] mats = Application.isPlaying ? renderer.materials : renderer.sharedMaterials;

            foreach (var mat in mats)
            {
                if (mat == null) continue;

                // 1. DOKU (TEXTURE) ATAMA OLUÐU
                if (customTexture != null)
                {
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", customTexture); // URP varsayýlan
                    if (mat.HasProperty("_BaseColorTexture")) mat.SetTexture("_BaseColorTexture", customTexture); // glTFast özel
                    if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", customTexture); // Standart Pipeline
                }

                // 2. GLOSSINESS / SMOOTHNESS / ROUGHNESS AYARI
                // Unity URP Lit shader'ý varsa:
                if (mat.HasProperty("_Smoothness"))
                    mat.SetFloat("_Smoothness", glossiness);

                // Standart shader'lar için:
                if (mat.HasProperty("_Glossiness"))
                    mat.SetFloat("_Glossiness", glossiness);

                // EÐER glTFast PBR Shader'ý aktifse (Roughness = 1 - Glossiness mantýðý)
                if (mat.HasProperty("_RoughnessFactor"))
                    mat.SetFloat("_RoughnessFactor", 1f - glossiness);


                // 3. METALLIC AYARI
                if (mat.HasProperty("_Metallic"))
                    mat.SetFloat("_Metallic", metallic);

                if (mat.HasProperty("_MetallicFactor"))
                    mat.SetFloat("_MetallicFactor", metallic);
            }

            // Runtime'da deðiþikliklerin modele iþlenmesini zorunlu kýlýyoruz
            if (Application.isPlaying)
            {
                renderer.materials = mats;
            }
        }
    }
}