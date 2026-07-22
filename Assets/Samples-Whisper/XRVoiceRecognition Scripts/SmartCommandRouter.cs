using UnityEngine;
using System.Text.RegularExpressions;

public class SmartCommandRouter : MonoBehaviour
{
    [Header("Router Settings")]
    [Tooltip("Enable to see performance and routing details in the console.")]
    public bool logDebugMessages = true;

    /// <summary>
    /// Analyzes the raw transcript from Whisper using Regex boundaries.
    /// </summary>
    public void ProcessTranscript(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript)) return;

        // 1. Gelen metni küçült, boþluklarý al ve NOKTALAMA ÝÞARETLERÝNÝ SÝL
        string cleanedTranscript = transcript.ToLowerInvariant().Trim();
        cleanedTranscript = Regex.Replace(cleanedTranscript, @"[.,?!;:]", "");

        if (logDebugMessages)
            Debug.Log($"[Smart Router] Temizlenmiþ Metin (Analyzed): '{cleanedTranscript}'");

        string detectedCommand = "";

        // 2. REGEX GÜNCELLEMESÝ: Sondaki '\b' sýnýrlarýný kaldýrdýk. 
        // Böylece "döndür", "döndürsün", "döndürürmüsün" kelimelerinin hepsi "rotate" komutunu tetikler.
        if (Regex.IsMatch(cleanedTranscript, @"\b(rotate|döndür|çevir)"))
        {
            detectedCommand = "rotate";
        }
        else if (Regex.IsMatch(cleanedTranscript, @"\b(material|materyal|malzeme|renk|doku|texture)"))
        {
            detectedCommand = "material";
        }
        else if (Regex.IsMatch(cleanedTranscript, @"\b(light on|lights on|ýþýðý aç|ýþýklarý aç|on|aç|aydýnlat)"))
        {
            detectedCommand = "on";
        }
        else if (Regex.IsMatch(cleanedTranscript, @"\b(light off|lights off|ýþýðý kapat|ýþýklarý kapat|off|kapat|söndür)"))
        {
            detectedCommand = "off";
        }

        if (string.IsNullOrEmpty(detectedCommand))
        {
            if (logDebugMessages)
                Debug.Log("[Smart Router] Metinde geçerli bir komut bulunamadý.");
            return;
        }

        if (logDebugMessages)
            Debug.Log($"[Smart Router] Yakalanan Ana Komut: {detectedCommand}");

        // Tüm etkileþimli objelere komutu gönder[cite: 7]
        VoiceInteractable[] allInteractables = FindObjectsOfType<VoiceInteractable>();
        int affectedObjectCount = 0;

        foreach (var interactable in allInteractables)
        {
            interactable.ProcessVoiceCommand(detectedCommand);
            affectedObjectCount++;
        }

        if (logDebugMessages)
            Debug.Log($"[Smart Router] Komut {affectedObjectCount} adet objeye yönlendirildi.");
    }
}