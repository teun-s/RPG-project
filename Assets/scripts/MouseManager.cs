using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public static MouseManager instance;
    public Item currentlyHeldItem;
    private BankUI bankUI;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        bankUI = FindAnyObjectByType<BankUI>();
        if (bankUI == null)
        {
            Debug.LogError("BankUI Not found in the scene");
        }
    }

    public void UpdateHeldItem(UISlotHandler currentSlot)
    {
        if (bankUI == null && currentSlot.inventoryManager == null)
        {
            Debug.LogError("BankUI or inventoryManager is null in MouseManager!");
            return;
        }

        Item currentActiveItem = currentSlot.item;

        if (bankUI.IsOpen() && currentActiveItem != null)
        {
            Debug.Log($"Depositing item ID {currentActiveItem.itemID} with count {currentActiveItem.itemCount} to bank.");
            currentSlot.inventoryManager.DepositItemToBank(currentSlot);
            return;
        }

        if (currentlyHeldItem != null && currentActiveItem != null && currentlyHeldItem.itemID == currentActiveItem.itemID && currentlyHeldItem.canStack && currentActiveItem.canStack)
        {
            currentSlot.inventoryManager.StackInInventory(currentSlot, currentlyHeldItem);
            currentlyHeldItem = null;
            return;
        }

        if (currentSlot.item != null)
        {
            currentSlot.inventoryManager.ClearItemSlot(currentSlot);
        }

        if (currentlyHeldItem != null)
        {
            currentSlot.inventoryManager.PlaceInInventory(currentSlot, currentlyHeldItem);
        }
        currentlyHeldItem = currentActiveItem;
    }

    public void PickupFromStack(UISlotHandler currentSlot)
    {
        if (bankUI.IsOpen())
        {
            Debug.Log("Bank UI is open, skipping stack pickup.");
            return;
        }
        if (currentlyHeldItem != null && currentlyHeldItem.itemID != currentSlot.item.itemID)
        {
            return;
        }
        if (!currentSlot.item.canStack)
        {
            UpdateHeldItem(currentSlot); // Treat as a regular pickup since stacking is disabled
            return;
        }
        if (currentlyHeldItem == null)
        {
            currentlyHeldItem = currentSlot.item.Clone();
            currentlyHeldItem.itemCount = 0;
        }

        currentlyHeldItem.itemCount++;
        currentSlot.item.itemCount--;
        currentSlot.itemCountText.text = currentSlot.item.itemCount.ToString();

        if (currentSlot.item.itemCount <= 0)
        {
            currentSlot.inventoryManager.ClearItemSlot(currentSlot);
        }
    }
}