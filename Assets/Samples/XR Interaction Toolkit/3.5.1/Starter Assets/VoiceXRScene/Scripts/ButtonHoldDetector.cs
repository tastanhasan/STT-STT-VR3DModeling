using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonHoldDetector : MonoBehaviour
{
    // Inspector'dan referans arama derdini bitiriyoruz.
    // Tuþu doðrudan kodun içinde, donaným yoluyla tanýmlýyoruz.
    private InputAction xButtonAction;

    private bool isHolding = false;

    private void Awake()
    {
        // Sol kontrolcü (LeftHand) üzerindeki Primary Button (X Tuþu) için aksiyon oluþturuluyor.
        xButtonAction = new InputAction(
            name: "X_Button",
            type: InputActionType.Button,
            binding: "<XRController>{LeftHand}/primaryButton"
        );
    }

    private void OnEnable()
    {
        // Kodla oluþturulan Input Action'larý manuel olarak aktifleþtirmek gerekir
        xButtonAction.Enable();
        xButtonAction.started += OnButtonDown;
        xButtonAction.canceled += OnButtonUp;
    }

    private void OnDisable()
    {
        xButtonAction.Disable();
        xButtonAction.started -= OnButtonDown;
        xButtonAction.canceled -= OnButtonUp;
    }

    private void OnButtonDown(InputAction.CallbackContext context)
    {
        isHolding = true;
        Debug.Log("<color=green>[X Tuþu] Basýldý ve tutuluyor...</color>");

        // Fynout veya diðer projelerinde basýlý tutma iþlemi baþladýðýnda çaðrýlacak metot buraya
    }

    private void OnButtonUp(InputAction.CallbackContext context)
    {
        if (isHolding)
        {
            isHolding = false;
            Debug.Log("<color=red>[X Tuþu] Býrakýldý!</color>");

            // Tuþ býrakýldýðýnda iþlemi sonlandýracak metot buraya
        }
    }
}