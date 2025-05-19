using UnityEngine;
using UnityEngine.InputSystem; // For the new Input System

public class InventoryUIManager : MonoBehaviour
{
    private Canvas inventoryCanvas; // Reference to the InventoryCanvas

    void Start()
    {
        // Get the Canvas component
        inventoryCanvas = GetComponent<Canvas>();
        if (inventoryCanvas != null)
        {
            // Hide the inventory by default
            inventoryCanvas.enabled = false;
        }
        else
        {
            Debug.LogWarning("InventoryCanvas component not found on this GameObject!");
        }
        UIEventManager.Instance.OnBankOpened += HandleBankOpened;
        UIEventManager.Instance.OnBankClosed += HandleBankClosed;
    }


    private void OnDestroy()
    {
        UIEventManager.Instance.OnBankOpened -= HandleBankOpened;
        UIEventManager.Instance.OnBankClosed -= HandleBankClosed;
    }

    void Update()
    {
        // Toggle inventory visibility with 'I' key using the new Input System
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (inventoryCanvas != null)
            {
                inventoryCanvas.enabled = !inventoryCanvas.enabled;
                UIEventManager.Instance.NotifyUIClosed();
                Debug.Log("InventoryCanvas toggled to: " + inventoryCanvas.enabled);
            }
        }
    }

    private void HandleBankOpened()
    {
        if (inventoryCanvas != null)
        {
            inventoryCanvas.enabled = true;
        }
    }

    private void HandleBankClosed()
    {
        if (inventoryCanvas != null)
        {
            if (inventoryCanvas.isActiveAndEnabled)
            {
                inventoryCanvas.enabled = false;
                UIEventManager.Instance.NotifyUIClosed();
            }
        }
    }
}