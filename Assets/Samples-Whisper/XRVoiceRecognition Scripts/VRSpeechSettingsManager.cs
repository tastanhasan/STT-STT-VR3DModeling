using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Whisper;
using Whisper.Utils;

public class VRSpeechSettingsManager : MonoBehaviour
{
    [Header("Sistem Baðlantýlarý")]
    public WhisperManager whisper;
    public MicrophoneRecord microphoneRecord;

    [Header("Arayüz (UI) Baðlantýlarý")]
    public GameObject settingsPanel;

    [Tooltip("Dilleri otomatik listeleyeceðimiz Dropdown")]
    public TMP_Dropdown languageDropdown;

    [Tooltip("Sistemdeki mikrofonlarý otomatik listeleyeceðimiz Dropdown")]
    public TMP_Dropdown micDropdown;

    public Toggle translateToggle;
    public Toggle vadToggle;

    private InputAction yButtonAction;

    // Whisper'ýn anlayabildiði dil kodlarý
    private readonly List<string> supportedLanguages = new List<string> { "auto", "tr", "en", "de", "fr", "es", "it", "ru" };

    private void Awake()
    {
        yButtonAction = new InputAction(name: "Y_Button", type: InputActionType.Button, binding: "<XRController>{LeftHand}/secondaryButton");
    }

    private void OnEnable()
    {
        yButtonAction.Enable();
        yButtonAction.performed += ToggleSettingsPanel;
    }

    private void OnDisable()
    {
        yButtonAction.Disable();
        yButtonAction.performed -= ToggleSettingsPanel;

        if (languageDropdown != null) languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
        if (translateToggle != null) translateToggle.onValueChanged.RemoveListener(OnTranslateChanged);
        if (vadToggle != null) vadToggle.onValueChanged.RemoveListener(OnVadChanged);
        if (micDropdown != null) micDropdown.onValueChanged.RemoveListener(OnMicChanged);
    }

    private void Start()
    {
        if (translateToggle != null) translateToggle.isOn = whisper.translateToEnglish;
        if (vadToggle != null) vadToggle.isOn = microphoneRecord.vadStop;

        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(supportedLanguages);

            int currentLangIndex = supportedLanguages.IndexOf(whisper.language);
            if (currentLangIndex != -1)
            {
                languageDropdown.value = currentLangIndex;
            }

            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        if (micDropdown != null)
        {
            micDropdown.ClearOptions();
            List<string> micDisplayNames = new List<string>();

            // 0. Ýndex her zaman en güvenilir VR seçeneðidir
            micDisplayNames.Add("Sistem Varsayýlaný (VR Ýçin Önerilen)");

            foreach (var device in Microphone.devices)
            {
                // Dropdown arayüzünün bozulmasýný engellemek için uzun isimleri kýsaltýyoruz
                string displayName = device.Length > 25 ? device.Substring(0, 22) + "..." : device;
                micDisplayNames.Add(displayName);
            }

            micDropdown.AddOptions(micDisplayNames);
            micDropdown.onValueChanged.AddListener(OnMicChanged);
        }

        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void ToggleSettingsPanel(InputAction.CallbackContext context)
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    private void OnMicChanged(int index)
    {
        if (index == 0)
        {
            // Null göndermek, Quest ve Android cihazlarýnda aktif kulaklýk mikrofonunu otomatik bulur.
            microphoneRecord.SelectedMicDevice = null;
            Debug.Log("[Mikrofon] Varsayýlan cihaza dönüldü (Oculus/Quest için en güvenli yöntem).");
        }
        else
        {
            // Gerçek cihaz ismini index - 1 ile çekiyoruz çünkü 0. index "Sistem Varsayýlaný"
            string actualDeviceName = Microphone.devices[index - 1];

            // Eðer cihaz ismi Oculus içeriyorsa geliþtiriciyi konsolda uyarýyoruz
            if (actualDeviceName.ToLower().Contains("oculus"))
            {
                Debug.LogWarning("[Mikrofon] DÝKKAT: Oculus mikrofonunu doðrudan ismen seçtiniz. Eðer ses algýlanmazsa 'Sistem Varsayýlaný' seçeneðine dönün.");
            }

            microphoneRecord.SelectedMicDevice = actualDeviceName;
            Debug.Log("[Mikrofon] Yeni cihaz seçildi: " + actualDeviceName);
        }
    }

    private void OnLanguageChanged(int index)
    {
        string selectedLang = supportedLanguages[index];
        whisper.language = selectedLang;
        Debug.Log("[Whisper] Dil ayarlandý: " + selectedLang);
    }

    private void OnTranslateChanged(bool isOn)
    {
        whisper.translateToEnglish = isOn;
    }

    private void OnVadChanged(bool isOn)
    {
        microphoneRecord.vadStop = isOn;
    }
}