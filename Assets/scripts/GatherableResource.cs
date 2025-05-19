using UnityEngine;

public class GatherableResource : MonoBehaviour
{
    [SerializeField] private Item resourceItem;
    [SerializeField] private int resourceCount = 1;
    [SerializeField] private float respawnTime = 10f;
    [SerializeField] private Sprite baseSprite; // Original sprite
    [SerializeField] private Sprite outlineSprite; // Sprite with white outline

    private SpriteRenderer spriteRenderer;
    private InventoryManager inventoryManager;
    private BoxCollider2D clickCollider;
    private CircleCollider2D triggerCollider;
    private bool isDepleted = false;
    public bool isPlayerNearby = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        clickCollider = GetComponent<BoxCollider2D>();
        triggerCollider = GetComponent<CircleCollider2D>();
        inventoryManager = Object.FindFirstObjectByType<InventoryManager>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on " + gameObject.name);
        }
        if (clickCollider == null)
        {
            Debug.LogError("BoxCollider2D not found on " + gameObject.name);
        }
        if (triggerCollider == null)
        {
            Debug.LogError("CircleCollider2D not found on " + gameObject.name);
        }
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager not found in the scene!");
        }
        if (resourceItem == null)
        {
            Debug.LogError("ResourceItem not assigned on " + gameObject.name);
        }
        if (baseSprite == null || outlineSprite == null)
        {
            Debug.LogError("BaseSprite or OutlineSprite not assigned on " + gameObject.name);
        }

        // Ensure the base sprite is set initially
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = baseSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger Entered by: " + other.name + " with tag: " + other.tag);
        if (other.CompareTag("Player") && !isDepleted)
        {
            isPlayerNearby = true;
            ShowOutline();
            Debug.Log("Player entered trigger area of " + gameObject.name + ". isPlayerNearby = " + isPlayerNearby);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Trigger Exited by: " + other.name + " with tag: " + other.tag);
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            HideOutline();
            Debug.Log("Player exited trigger area of " + gameObject.name + ". isPlayerNearby = " + isPlayerNearby);
        }
    }

    public void Gather()
    {
        Debug.Log("Gather called on " + gameObject.name + ". isPlayerNearby = " + isPlayerNearby);
        if (isDepleted || inventoryManager == null || resourceItem == null || !isPlayerNearby)
        {
            Debug.Log("Cannot gather: isDepleted = " + isDepleted + ", inventoryManager = " + (inventoryManager == null) + ", resourceItem = " + (resourceItem == null) + ", isPlayerNearby = " + isPlayerNearby);
            return;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = outlineSprite; // Flash the outline
            Invoke("HideOutline", 0.2f);
        }

        if (inventoryManager.IsInventoryFull())
        {
            Debug.Log("Cannot gather " + gameObject.name + ": Inventory is full!");
            return;
        }

        Item clonedItem = resourceItem.Clone();
        clonedItem.itemCount = resourceCount;
        if (inventoryManager.AddItemToInventory(clonedItem, true))
        {
            Debug.Log($"Gathered {resourceCount} {resourceItem.itemID} from " + gameObject.name);
            Deplete();
        }
        else
        {
            Debug.Log("Failed to add " + resourceItem.itemID + " to inventory! Check AddItemToInventory logic.");
        }
    }

    private void Deplete()
    {
        isDepleted = true;
        spriteRenderer.enabled = false;
        clickCollider.enabled = false;
        triggerCollider.enabled = false;
        HideOutline();
        Invoke("Respawn", respawnTime);
    }

    private void Respawn()
    {
        isDepleted = false;
        spriteRenderer.enabled = true;
        clickCollider.enabled = true;
        triggerCollider.enabled = true;
        if (isPlayerNearby)
        {
            ShowOutline();
        }
        else
        {
            HideOutline();
        }
        Debug.Log(gameObject.name + " respawned");
    }

    private void ShowOutline()
    {
        if (spriteRenderer != null && outlineSprite != null)
        {
            spriteRenderer.sprite = outlineSprite;
            Debug.Log("Showing outline on " + gameObject.name);
        }
    }

    private void HideOutline()
    {
        if (spriteRenderer != null && baseSprite != null)
        {
            spriteRenderer.sprite = baseSprite;
            Debug.Log("Hiding outline on " + gameObject.name);
        }
    }
}