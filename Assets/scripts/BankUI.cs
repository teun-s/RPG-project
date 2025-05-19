using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BankUI : MonoBehaviour
{
    [SerializeField] private Transform content; // Reference to "Content" object with Grid Layout Group
    [SerializeField] private Button depositAllButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private BankManager bankManager;
    [SerializeField] private InventoryManager inventoryManager;
    private bool isOpen = false;

    private UISlotHandler[] bankSlots; // Store the pre-populated slots

    void Start()
    {
        if (bankManager == null)
        {
            Debug.LogError("BankManager not assigned in BankUI!");
        }

        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager not assigned in BankUI!");
        }

        if (content != null)
        {
            bankSlots = content.GetComponentsInChildren<UISlotHandler>();
            Debug.Log($"Found {bankSlots.Length} bank slots.");
        }
        else
        {
            Debug.LogError("Content not assigned in BankUI!");
        }

        if (depositAllButton != null)
        {
            Debug.Log("DepositAllButton assigned, adding listener.");
            depositAllButton.onClick.AddListener(DepositAllItems);
        }
        else
        {
            Debug.LogError("DepositAllButton not assigned in BankUI!");
        }

        if (closeButton != null)
        {
            Debug.Log("CloseButton assigned, adding listener.");
            closeButton.onClick.AddListener(CloseUI);
        }
        else
        {
            Debug.LogError("CloseButton not assigned in BankUI!");
        }

        gameObject.SetActive(false);
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public void OpenUI()
    {
        Debug.Log("Opening Bank UI.");
        gameObject.SetActive(true);
        isOpen = true;
        RefreshUI();
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
        isOpen = false;
    }

    private void DepositAllItems()
    {
        Debug.Log("DepositAllItems called.");
        if (inventoryManager == null || bankManager == null)
        {
            Debug.LogError($"Deposit failed: inventoryManager = {(inventoryManager == null ? "null" : "not null")}, bankManager = {(bankManager == null ? "null" : "not null")}");
            return;
        }

        var slots = inventoryManager.GetInventorySlots();
        Debug.Log($"Found {slots.Length} inventory slots to deposit.");

        foreach (UISlotHandler slot in slots)
        {
            if (slot.item != null)
            {
                Debug.Log($"Depositing item ID {slot.item.itemID} with count {slot.item.itemCount}.");
                bankManager.DepositItem(slot.item.itemID, slot.item.itemCount);
                inventoryManager.ClearItemSlot(slot);
            }
        }
        RefreshUI();
    }

    public void RefreshUI()
    {
        Debug.Log("Refreshing Bank UI.");
        if (bankManager == null)
        {
            Debug.LogError("bankManager is null in RefreshUI!");
            return;
        }

        var bankSlotsData = bankManager.GetBankSlots();
        Debug.Log($"Bank contains {bankSlotsData.Count} slots.");

        // Reset all slots to empty
        foreach (UISlotHandler slot in bankSlots)
        {
            slot.item = null;
            slot.icon.sprite = null;
            slot.icon.color = new Color(1f, 1f, 1f, 0f); // Transparent
            slot.itemCountText.text = "";
            slot.icon.gameObject.SetActive(false);
        }

        // Update slots with bank contents
        for (int i = 0; i < bankSlotsData.Count && i < bankSlots.Length; i++)
        {
            BankManager.BankSlot bankSlot = bankSlotsData[i];
            Item item = bankManager.GetItemByID(bankSlot.itemID);
            if (item == null)
            {
                Debug.LogWarning($"Item with ID {bankSlot.itemID} not found in allItems!");
                continue;
            }

            Debug.Log($"Updating bank slot {i} with item ID {bankSlot.itemID} and count {bankSlot.itemCount}.");
            UISlotHandler slotHandler = bankSlots[i];
            Item clonedItem = item.Clone();
            clonedItem.itemCount = bankSlot.itemCount;
            slotHandler.item = clonedItem;
            slotHandler.icon.sprite = clonedItem.itemIcon;
            slotHandler.icon.color = Color.white;
            slotHandler.itemCountText.text = clonedItem.itemCount.ToString();
            slotHandler.icon.gameObject.SetActive(true);

            // Add withdraw button listener
            Button button = slotHandler.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => WithdrawItem(bankSlot.itemID, bankSlot.itemCount));
            }
        }
    }

    private void WithdrawItem(int itemID, int itemCount)
    {
        if (bankManager != null)
        {
            bankManager.WithdrawItem(itemID, itemCount);
            RefreshUI();
        }
    }
}