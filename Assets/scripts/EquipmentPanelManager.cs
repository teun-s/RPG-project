using System.Collections.Generic;
using UnityEngine;

public class EquipmentPanelManager : MonoBehaviour
{
    private Dictionary<EquipmentType, Item> slots = new Dictionary<EquipmentType, Item>();

    private InventoryManager inventory;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slots.Add(EquipmentType.Head, null);
        slots.Add(EquipmentType.Chest, null);
        slots.Add(EquipmentType.Boot, null);
        slots.Add(EquipmentType.Weapon, null);
        inventory = FindFirstObjectByType<InventoryManager>();
    }


    public bool EquipItem(Item item)
    {
        if (item == null) return false;
        EquipmentType slotType = item.equipmentType;

        // Check if the slot exists
        if (slots.ContainsKey(slotType))
        {
            // If slot is occupied and inventory is full, prevent equipping
            if (slots[slotType] != null && inventory.IsInventoryFull())
            {
                Debug.Log("Cannot equip: Inventory is full.");
                return false;
            }

            // Unequip current item back to inventory
            if (slots[slotType] != null)
            {
                inventory.AddItemToInventory(slots[slotType]);
            }

            // Equip the new item
            slots[slotType] = item;
            Debug.Log($"Equipped {item.itemID} to {slotType} slot.");
            return true;
        }
        else
        {
            Debug.Log("No matching slot for this item.");
            return false;
        }
    }

    // Get the item in a specific slot
    public Item GetEquippedItem(EquipmentType slotType)
    {
        return slots.ContainsKey(slotType) ? slots[slotType] : null;
    }

    // Optional: Unequip an item
    public void UnequipItem(EquipmentType slotType)
    {
        if (slots.ContainsKey(slotType) && slots[slotType] != null)
        {
            inventory.AddItemToInventory(slots[slotType]);
            slots[slotType] = null;
            Debug.Log($"Unequipped item from {slotType} slot.");
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
