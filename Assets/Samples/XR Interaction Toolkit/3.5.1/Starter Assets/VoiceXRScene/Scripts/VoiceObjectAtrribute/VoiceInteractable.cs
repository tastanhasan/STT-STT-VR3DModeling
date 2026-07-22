using UnityEngine;

[RequireComponent(typeof(BoxSelectVisualizerParent))]
public class VoiceInteractable : MonoBehaviour
{
    [Header("Command Permissions")]
    [Tooltip("Allows this specific object to be rotated via voice command.")]
    public bool canRotate = true;

    [Tooltip("Allows opening the VR Texture Panel to change materials.")]
    public bool canChangeMaterial = true;

    [Tooltip("Allows toggling the assigned target objects on and off.")]
    public bool canToggleObjects = false;

    [Space(10)]
    [Header("Tool & Target References")]
    [Tooltip("The protractor UI tool used for rotation. Leave empty if not required.")]
    public GameObject myProtractor;

    [Tooltip("Assign the GameObjects (lights, meshes, groups) to turn on/off via voice commands.")]
    public GameObject[] targetObjectsToToggle;

    private BoxSelectVisualizerParent selectionData;

    private void Awake()
    {
        selectionData = GetComponent<BoxSelectVisualizerParent>();
        if (selectionData == null)
        {
            Debug.LogError($"[VoiceInteractable] Missing BoxSelectVisualizerParent on {gameObject.name}!");
        }

        // Hide the protractor on start if it exists
        if (myProtractor != null)
        {
            myProtractor.SetActive(false);
        }
    }

    private void Update()
    {
        // Disable the active protractor if the selection is canceled
        if (myProtractor != null && myProtractor.activeSelf && !selectionData.isSelected)
        {
            myProtractor.SetActive(false);
        }
    }

    public void ProcessVoiceCommand(string command)
    {
        if (string.IsNullOrEmpty(command)) return;

        command = command.ToLowerInvariant().Trim();

        if (!selectionData.isSelected)
        {
            Debug.LogWarning($"[VoiceInteractable] Command '{command}' rejected. {gameObject.name} is NOT selected.");
            return;
        }

        switch (command)
        {
            case "rotate":
                HandleRotateCommand();
                break;

            case "material":
                HandleMaterialCommand();
                break;

            case "on":
                HandleToggleCommand(true);
                break;

            case "off":
                HandleToggleCommand(false);
                break;

            default:
                Debug.LogWarning($"[VoiceInteractable] Unrecognized command: '{command}' on {gameObject.name}");
                break;
        }
    }

    private void HandleRotateCommand()
    {
        if (!canRotate || myProtractor == null) return;

        bool newState = !myProtractor.activeSelf;
        myProtractor.SetActive(newState);
        Debug.Log($"[VoiceInteractable] Protractor toggled to: {newState}");
    }

    private void HandleMaterialCommand()
    {
        if (!canChangeMaterial) return;

        if (VRTexturePanelManager.Instance != null)
        {
            VRTexturePanelManager.Instance.OpenPanel(this.gameObject);
        }
        else
        {
            Debug.LogError("[VoiceInteractable] VRTexturePanelManager.Instance is NULL!");
        }
    }

    private void HandleToggleCommand(bool turnOn)
    {
        if (!canToggleObjects || targetObjectsToToggle == null || targetObjectsToToggle.Length == 0)
        {
            Debug.LogWarning($"[VoiceInteractable] Toggle command ignored on {gameObject.name}. Permission is false or no targets assigned.");
            return;
        }

        // Apply state change to all assigned target objects
        foreach (var targetObj in targetObjectsToToggle)
        {
            if (targetObj != null)
            {
                targetObj.SetActive(turnOn);
            }
        }

        Debug.Log($"[VoiceInteractable] Target objects set to: {turnOn} on {gameObject.name}");
    }

    // Displays the available English commands to the UI overlay
    public string GetAvailableCommandsText()
    {
        string commands = "";

        if (canRotate) commands += "• Rotate\n";
        if (canChangeMaterial) commands += "• Material\n";
        if (canToggleObjects) commands += "• Turn On / Off\n";

        return commands;
    }
}