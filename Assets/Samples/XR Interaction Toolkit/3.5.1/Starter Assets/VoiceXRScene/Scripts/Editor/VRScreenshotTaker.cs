using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class VRScreenshotTaker : MonoBehaviour
{
    [Header("Ekran Görüntüsü Ayarlarý")]
    [Tooltip("Fotoðraflarýn kaydedileceði klasör yolu. Boþ býrakýrsan doðrudan Assets ana klasörüne kaydeder.")]
    public string savePath = "Assets/";

    // Sað el B tuþu için Input Action
    private InputAction bButtonAction;

    private void Awake()
    {
        // XR Controller için Sað El Ýkincil Tuþ (Oculus için B tuþu) atamasý
        bButtonAction = new InputAction(
            name: "B_Button",
            type: InputActionType.Button,
            binding: "<XRController>{RightHand}/secondaryButton"
        );
    }

    private void OnEnable()
    {
        bButtonAction.Enable();
        bButtonAction.performed += OnBButtonPressed;
    }

    private void OnDisable()
    {
        bButtonAction.Disable();
        bButtonAction.performed -= OnBButtonPressed;
    }

    private void OnBButtonPressed(InputAction.CallbackContext context)
    {
        TakeScreenshot();
    }

    private void TakeScreenshot()
    {
        // Klasör yolunun sonuna / eklendiðinden emin ol
        if (!savePath.EndsWith("/")) savePath += "/";

        // Tarih ve saate göre benzersiz bir dosya adý oluþtur
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"{savePath}VRScreenshot_{timestamp}.png";

        // Ekran görüntüsünü al ve kaydet
        ScreenCapture.CaptureScreenshot(fileName);
        Debug.Log($"[VRScreenshotTaker] Fotoðraf baþarýyla kaydedildi: {fileName}");

#if UNITY_EDITOR
        // Unity Editörünün yeni oluþturulan dosyayý anýnda projede (Project penceresinde) göstermesi için yenileme yap
        // Not: ScreenCapture asenkron çalýþabildiði için AssetDatabase.Refresh bir frame sonra çalýþsa daha iyi olur,
        // ancak geliþtirme ortamýnda bu haliyle de iþini görecektir.
        AssetDatabase.Refresh();
#endif
    }
}