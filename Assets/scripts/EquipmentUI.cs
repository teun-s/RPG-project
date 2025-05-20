using UnityEngine;
using UnityEngine.UI;

public class EquipmentUI : MonoBehaviour
{
    // Assign these in the Unity Inspector
    public Image headSlotIcon;
    public Image chestSlotIcon;
    public Image weaponSlotIcon;

    private void OnEnable()
    {
        EquipmentManager.Instance.OnEquipmentChanged += UpdateSlot;
        RefreshUI();
    }

    private void OnDisable()
    {
        EquipmentManager.Instance.OnEquipmentChanged -= UpdateSlot;
    }

    private void RefreshUI()
    {
        UpdateSlot(EquipmentType.Head, EquipmentManager.Instance.GetEquippedItem(EquipmentType.Head));
        UpdateSlot(EquipmentType.Chest, EquipmentManager.Instance.GetEquippedItem(EquipmentType.Chest));
        UpdateSlot(EquipmentType.Weapon, EquipmentManager.Instance.GetEquippedItem(EquipmentType.Weapon));
    }

    private void UpdateSlot(EquipmentType slot, Item item)
    {
        Image slotIcon = GetSlotIcon(slot);
        if (slotIcon != null)
        {
            slotIcon.sprite = item?.itemIcon ?? null;
            slotIcon.enabled = item != null; // Hide if no item
        }
    }

    private Image GetSlotIcon(EquipmentType slot)
    {
        switch (slot)
        {
            case EquipmentType.Head: return headSlotIcon;
            case EquipmentType.Chest: return chestSlotIcon;
            case EquipmentType.Weapon: return weaponSlotIcon;
            default: return null;
        }
    }
}