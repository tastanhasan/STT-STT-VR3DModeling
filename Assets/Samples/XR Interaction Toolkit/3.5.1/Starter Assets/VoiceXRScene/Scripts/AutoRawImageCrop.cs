using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways] // OYUN BAÞLAMADAN (EDÝTÖRDE) ÇALIÞMASINI SAÐLAYAN KOD
[RequireComponent(typeof(RawImage))]
public class AutoRawImageCrop : MonoBehaviour
{
    private RawImage rawImage;

    private void Awake()
    {
        ApplyCropToFit();
    }

    private void OnEnable()
    {
        ApplyCropToFit();
    }

    // Arayüzdeki (Canvas) buton boyutlarý deðiþtiðinde anýnda düzeltir
    private void OnRectTransformDimensionsChange()
    {
        ApplyCropToFit();
    }

#if UNITY_EDITOR
    // Inspector paneli üzerinden dokuyu (texture) deðiþtirdiðinizde anýnda günceller
    private void OnValidate()
    {
        // OnValidate içinde gecikmeli çaðýrmak, Unity editör hatalarýný önler
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null) ApplyCropToFit();
        };
    }
#endif

    public void ApplyCropToFit()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        if (rawImage == null || rawImage.texture == null)
            return;

        Texture tex = rawImage.texture;

        // Sýfýra bölünme hatasýný engellemek için güvenlik önlemi
        if (tex.height == 0 || rawImage.rectTransform.rect.height == 0) return;

        float textureAspect = (float)tex.width / tex.height;
        float uiAspect = rawImage.rectTransform.rect.width / rawImage.rectTransform.rect.height;

        Rect uvRect = new Rect(0, 0, 1, 1);

        if (textureAspect > uiAspect)
        {
            float cropWidth = uiAspect / textureAspect;
            uvRect.width = cropWidth;
            uvRect.x = (1f - cropWidth) / 2f;
        }
        else if (textureAspect < uiAspect)
        {
            float cropHeight = textureAspect / uiAspect;
            uvRect.height = cropHeight;
            uvRect.y = (1f - cropHeight) / 2f;
        }

        rawImage.uvRect = uvRect;
    }
}