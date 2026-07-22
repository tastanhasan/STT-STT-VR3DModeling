using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Whisper;
using Whisper.Utils;

public class XRVoiceController : MonoBehaviour
{
    [Header("Whisper & Mikrofon Baðlantýlarý")]
    public WhisperManager whisper;
    public MicrophoneRecord microphoneRecord;
    public SmartCommandRouter commandManager;

    [Header("Görsel Arayüz (UI)")]
    public GameObject voiceUIPanel;
    public TextMeshProUGUI feedbackText;

    [Tooltip("Panelin yavaþça silinmesi için CanvasGroup bileþeni gereklidir")]
    public CanvasGroup uiCanvasGroup;

    private InputAction xButtonAction;
    private Coroutine closePanelCoroutine; // Kapanma iþlemini yönetecek güvenli kanal

    private void Awake()
    {
        xButtonAction = new InputAction(name: "X_Button", type: InputActionType.Button, binding: "<XRController>{LeftHand}/primaryButton");

        // Güvenlik: Eðer CanvasGroup atanmamýþsa otomatik ekle
        if (uiCanvasGroup == null && voiceUIPanel != null)
        {
            uiCanvasGroup = voiceUIPanel.GetComponent<CanvasGroup>();
            if (uiCanvasGroup == null)
                uiCanvasGroup = voiceUIPanel.AddComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        xButtonAction.Enable();
        xButtonAction.started += OnButtonDown;
        xButtonAction.canceled += OnButtonUp;
        microphoneRecord.OnRecordStop += OnRecordStop;
    }

    private void OnDisable()
    {
        xButtonAction.Disable();
        xButtonAction.started -= OnButtonDown;
        xButtonAction.canceled -= OnButtonUp;
        microphoneRecord.OnRecordStop -= OnRecordStop;
    }

    private void OnButtonDown(InputAction.CallbackContext context)
    {
        // 1. Yeni butona basýldýðýnda devam eden kapanma ve silinme efektlerini DURDUR
        if (closePanelCoroutine != null)
        {
            StopCoroutine(closePanelCoroutine);
            closePanelCoroutine = null;
        }

        // 2. Paneli görünür ve %100 opak (net) yap
        voiceUIPanel.SetActive(true);
        uiCanvasGroup.alpha = 1f;

        feedbackText.text = "<color=red>Dinleniyor...</color>";

        if (!microphoneRecord.IsRecording)
        {
            microphoneRecord.StartRecord();
        }
    }

    private void OnButtonUp(InputAction.CallbackContext context)
    {
        feedbackText.text = "<color=yellow>Ýþleniyor...</color>";

        if (microphoneRecord.IsRecording)
        {
            microphoneRecord.StopRecord();
        }
    }

    private async void OnRecordStop(AudioChunk recordedAudio)
    {
        if (recordedAudio.Data == null || recordedAudio.Data.Length == 0)
        {
            feedbackText.text = "Çok kýsa basýldý!";
            closePanelCoroutine = StartCoroutine(WaitAndFadeOut(2f)); // Kýsa hata için 2 sn
            return;
        }

        var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);

        if (res == null || string.IsNullOrWhiteSpace(res.Result))
        {
            feedbackText.text = "Sesi anlayamadým.";
            closePanelCoroutine = StartCoroutine(WaitAndFadeOut(2.5f));
            return;
        }

        feedbackText.text = res.Result;

        if (commandManager != null)
        {
            commandManager.ProcessTranscript(res.Result);
        }

        // DÝNAMÝK SÜRE HESAPLAMA: Her bir harf için 0.08 saniye ekle.
        // En az 2.5 saniye, en fazla 8 saniye açýk kalsýn.
        float dynamicWaitTime = Mathf.Clamp(res.Result.Length * 0.08f, 2.5f, 8f);

        // Ýþlemi Coroutine'e devret
        closePanelCoroutine = StartCoroutine(WaitAndFadeOut(dynamicWaitTime));
    }

    // Yumuþak geçiþi ve beklemeyi saðlayan Coroutine motoru
    private IEnumerator WaitAndFadeOut(float waitTime)
    {
        // Belirlenen dinamik süre kadar bekle
        yield return new WaitForSeconds(waitTime);

        // Alpha (Saydamlýk) deðerini 1'den 0'a doðru yavaþça düþür
        float fadeSpeed = 2f;
        while (uiCanvasGroup.alpha > 0)
        {
            uiCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null; // Bir sonraki frame'i bekle
        }

        // Tamamen görünmez olunca paneli kapat
        voiceUIPanel.SetActive(false);
        uiCanvasGroup.alpha = 1f; // Bir sonraki açýlýþ için sýfýrla
    }
}