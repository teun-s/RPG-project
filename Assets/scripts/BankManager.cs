using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BankManager : MonoBehaviour
{
    [System.Serializable]
    public class BankSlot
    {
        public int itemID; // ID of the item in this slot
        public int itemCount; // Number of items in this slot
    }

    private List<BankSlot> bankSlots = new List<BankSlot>(); // List of items in the bank
    private List<Item> allItems = new List<Item>(); // Array of all possible items, indexed by itemID
    [SerializeField] private InventoryManager inventoryManager; // Reference to the InventoryManager

    void Start()
    {
        // Initialize inventoryManager
        inventoryManager = FindFirstObjectByType<InventoryManager>();
        Item[] loadedItems = Resources.LoadAll<Item>("Items");
        if (loadedItems.Count() <= 0)
        {
            Debug.Log("Falied to load items in resource folder");
        }
        allItems.AddRange(loadedItems);

        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager not found in the scene!");
        }

        // Load the bank state
        LoadBank();
    }

    void OnApplicationQuit()
    {
        // Save the bank state when the game exits
        SaveBank();
    }

    // Deposit an item from the inventory into the bank
    public void DepositItem(int itemID, int itemCount)
    {
        // Find an existing slot with the same itemID
        BankSlot existingSlot = bankSlots.Find(slot => slot.itemID == itemID);
        if (existingSlot != null)
        {
            // Stack the items in the bank
            existingSlot.itemCount += itemCount;
            Debug.Log($"Deposited {itemCount} items with ID {itemID} into existing bank slot. New count: {existingSlot.itemCount}");
        }
        else
        {
            // Create a new slot in the bank
            bankSlots.Add(new BankSlot { itemID = itemID, itemCount = itemCount });
            Debug.Log($"Deposited {itemCount} items with ID {itemID} into new bank slot.");
        }
    }

    // Withdraw an item from the bank into the inventory
    public bool WithdrawItem(int itemID, int amount)
    {
        BankSlot bankSlot = bankSlots.Find(slot => slot.itemID == itemID);
        if (bankSlot == null || bankSlot.itemCount < amount)
        {
            Debug.Log($"Cannot withdraw {amount} items with ID {itemID}: Not enough in bank!");
            return false;
        }

        Item itemTemplate = allItems.Find(item => item.itemID == itemID);
        if (itemTemplate == null)
        {
            Debug.LogError($"Item with ID {itemID} not found in allItems!");
            return false;
        }

        Item itemToWithdraw = itemTemplate.Clone();
        itemToWithdraw.itemCount = amount;

        if (inventoryManager != null && inventoryManager.AddItemToInventory(itemToWithdraw, allowStacking: true))
        {
            bankSlot.itemCount -= amount;
            if (bankSlot.itemCount <= 0)
            {
                bankSlots.Remove(bankSlot);
            }
            Debug.Log($"Withdrew {amount} items with ID {itemID} from bank. Remaining in bank: {bankSlot.itemCount}");
            return true;
        }

        Debug.Log($"Failed to withdraw {amount} items with ID {itemID}: Inventory is full!");
        return false;
    }

    // Get the list of bank slots (for UI display)
    public List<BankSlot> GetBankSlots()
    {
        return bankSlots;
    }

    // Get the item template by ID (for UI display)
    public Item GetItemByID(int itemID)
    {
        return allItems.Find(item => item.itemID == itemID);
    }

    private void SaveBank()
    {
        // Convert bankSlots to a JSON string and save to PlayerPrefs
        string json = JsonUtility.ToJson(new BankData { slots = bankSlots });
        PlayerPrefs.SetString("BankData", json);
        PlayerPrefs.Save();
        Debug.Log("Bank data saved.");
    }

    private void LoadBank()
    {
        // Load bank data from PlayerPrefs
        string json = PlayerPrefs.GetString("BankData", "");
        if (!string.IsNullOrEmpty(json))
        {
            BankData data = JsonUtility.FromJson<BankData>(json);
            bankSlots = data.slots ?? new List<BankSlot>();
            Debug.Log($"Bank data loaded with {bankSlots.Count} slots.");
        }
    }

    [System.Serializable]
    private class BankData
    {
        public List<BankSlot> slots;
    }
}