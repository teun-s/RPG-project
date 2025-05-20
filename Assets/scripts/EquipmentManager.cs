using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }
    private Dictionary<EquipmentType, Item> equippedItems = new Dictionary<EquipmentType, Item>();
    private InventoryManager inventory;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventory = FindFirstObjectByType<InventoryManager>();
    }

    public void EquipItem(Item item)
    {
        if (item == null || item.equipmentType == EquipmentType.None) return;

        EquipmentType slot = item.equipmentType;
        if (equippedItems.ContainsKey(slot))
        {
            // Unequip existing item back to inventory (optional)
            InventoryManager.Instance.AddItemToInventory(equippedItems[slot]);
        }
        equippedItems[slot] = item;
        OnEquipmentChanged?.Invoke(slot, item);
    }

    public Item GetEquippedItem(EquipmentType slot)
    {
        equippedItems.TryGetValue(slot, out Item item);
        return item;
    }

    public delegate void EquipmentChanged(EquipmentType slot, Item item);
    public event EquipmentChanged OnEquipmentChanged;
}
