using UnityEngine;
using TMPro;
using System.Collections;

public class InventoryManager : MonoBehaviour
{
    private UISlotHandler[] inventorySlots;
    [SerializeField] private GameObject slotContainer;
    [SerializeField] private TextMeshProUGUI inventoryFullWarning;
    [SerializeField] private float warningDisplayDuration = 3f;
    [SerializeField] private float warningFadeDuration = 1f;
    [SerializeField] private BankManager bankManager;
    [SerializeField] private BankUI bankUI;
    public static InventoryManager Instance { get; private set; }
    private Coroutine warningFadeCoroutine;

    void Awake()
    {
        // If no instance exists, set this as the instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keeps the instance across scenes
        }
        else
        {
            // If another instance already exists, destroy this one
            Destroy(gameObject);
        }
    }


    void Start()
    {
        if (slotContainer != null)
        {
            inventorySlots = slotContainer.GetComponentsInChildren<UISlotHandler>();
            if (inventorySlots.Length == 0)
            {
                Debug.LogError("No UISlotHandler components found in children of slot container!");
            }
            else
            {
                Debug.Log($"Found {inventorySlots.Length} inventory slots.");
                foreach (UISlotHandler slot in inventorySlots)
                {
                    slot.inventoryManager = this;
                }
            }
        }
        else
        {
            Debug.LogError("Slot container not assigned in InventoryManager!");
        }

        if (inventoryFullWarning != null)
        {
            inventoryFullWarning.gameObject.SetActive(false);
        }
    }

    public bool IsInventoryFull()
    {
        //Debug.Log("Checking if inventory is full...");
        foreach (UISlotHandler slot in inventorySlots)
        {
            if (slot.item == null)
            {
                //Debug.Log("Found an empty slot, inventory is not full.");
                if (inventoryFullWarning != null && inventoryFullWarning.gameObject.activeSelf)
                {
                    //Debug.Log("Hiding inventory full warning.");
                    inventoryFullWarning.gameObject.SetActive(false);
                    if (warningFadeCoroutine != null)
                    {
                        StopCoroutine(warningFadeCoroutine);
                        warningFadeCoroutine = null;
                    }
                }
                else if (inventoryFullWarning == null)
                {
                    Debug.LogWarning("inventoryFullWarning is null in InventoryManager!");
                }
                return false;
            }
        }
        Debug.Log("No empty slots found, inventory is full.");
        if (inventoryFullWarning != null)
        {
            if (!inventoryFullWarning.gameObject.activeSelf)
            {
                Debug.Log("Showing inventory full warning.");
                inventoryFullWarning.gameObject.SetActive(true);
                Color color = inventoryFullWarning.color;
                color.a = 1f;
                inventoryFullWarning.color = color;
            }
            if (warningFadeCoroutine != null)
            {
                StopCoroutine(warningFadeCoroutine);
            }
            warningFadeCoroutine = StartCoroutine(FadeOutWarning());
        }
        else
        {
            Debug.LogWarning("inventoryFullWarning is null in InventoryManager!");
        }
        return true;
    }

    private IEnumerator FadeOutWarning()
    {
        yield return new WaitForSeconds(warningDisplayDuration);

        float elapsedTime = 0f;
        Color startColor = inventoryFullWarning.color;
        Color endColor = startColor;
        endColor.a = 0f;

        while (elapsedTime < warningFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / warningFadeDuration;
            inventoryFullWarning.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        inventoryFullWarning.gameObject.SetActive(false);
        warningFadeCoroutine = null;
    }

    public void EquipItem(UISlotHandler slot)
    {
        EquipmentManager.Instance.EquipItem(slot.item);
        ClearItemSlot(slot);
    }

    public void StackInInventory(UISlotHandler currentSlot, Item item)
    {
        if (!currentSlot.item.canStack || !item.canStack)
        {
            Debug.LogWarning("Cannot stack item: " + item.itemID + " (stacking disabled)");
            return;
        }
        currentSlot.item.itemCount += item.itemCount;
        currentSlot.itemCountText.text = currentSlot.item.itemCount.ToString();
    }

    public void PlaceInInventory(UISlotHandler currentSlot, Item item)
    {
        currentSlot.item = item;
        currentSlot.icon.sprite = item.itemIcon;
        currentSlot.itemCountText.text = item.itemCount.ToString();
        currentSlot.icon.gameObject.SetActive(true);
    }

    public void DepositItemToBank(UISlotHandler slot)
    {
        if (bankManager == null || bankUI == null)
        {
            Debug.LogError("Cannot deposit item: bankManager or bankUI is null.");
            return;
        }

        if (slot.item != null)
        {
            Debug.Log($"Depositing item ID {slot.item.itemID} with count {slot.item.itemCount} from inventory to bank.");
            bankManager.DepositItem(slot.item.itemID, slot.item.itemCount);
            ClearItemSlot(slot);
            bankUI.RefreshUI();
        }
    }


    public void ClearItemSlot(UISlotHandler currentSlot)
    {
        currentSlot.item = null;
        currentSlot.icon.sprite = null;
        currentSlot.itemCountText.text = string.Empty.ToString();
        currentSlot.icon.gameObject.SetActive(false);
        IsInventoryFull();
    }

    public bool AddItemToInventory(Item itemToAdd, bool allowStacking = false)
    {
        if (itemToAdd == null)
        {
            Debug.LogError("AddItemToInventory: itemToAdd is null!");
            return false;
        }

        Debug.Log($"Trying to add item with ID: {itemToAdd.itemID}");

        // Temporarily allow stacking if specified (e.g., for withdrawing from bank)
        bool originalCanStack = itemToAdd.canStack;
        if (allowStacking)
        {
            itemToAdd.canStack = true;
        }

        try
        {
            if (itemToAdd.canStack)
            {
                Debug.Log("Item can stack, checking for existing slots...");
                foreach (UISlotHandler slot in inventorySlots)
                {
                    if (slot.item != null && slot.item.itemID == itemToAdd.itemID)
                    {
                        Debug.Log($"Found stackable slot with item ID {slot.item.itemID}, stacking...");
                        StackInInventory(slot, itemToAdd);
                        return true;
                    }
                }
            }
            else
            {
                Debug.Log("Item cannot stack, looking for an empty slot...");
            }

            Debug.Log($"Checking {inventorySlots.Length} slots for an empty slot...");
            int slotIndex = 0;
            foreach (UISlotHandler slot in inventorySlots)
            {
                if (slot == null)
                {
                    Debug.LogWarning($"Slot at index {slotIndex} is null!");
                    slotIndex++;
                    continue;
                }
                if (slot.item == null)
                {
                    Debug.Log($"Found empty slot at index {slotIndex}, placing item...");
                    Item clonedItem = itemToAdd.Clone();
                    PlaceInInventory(slot, clonedItem);
                    return true;
                }
                slotIndex++;
            }

            Debug.Log("Inventory is full! Cannot add item: " + itemToAdd.itemID);
            return false;
        }
        finally
        {
            // Restore the original canStack value
            itemToAdd.canStack = originalCanStack;
        }
    }

    // Added method to expose the inventory slots
    public UISlotHandler[] GetInventorySlots()
    {
        return inventorySlots;
    }
}