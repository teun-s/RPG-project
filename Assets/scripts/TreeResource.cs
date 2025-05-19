using System;
using System.Xml.Linq;
using UnityEngine;

public class TreeResource : MonoBehaviour
{
    public TreeData treeData;
    public float respawnDelay = 10f;
    public GameObject floatingEffectPrefab;

    private int woodAmount;
    private float chopTimer = 0f;
    private bool isBeingChopped = false;
    private float effectiveChopTime;
    public SpriteRenderer foliage;

    private GameObject player;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D collider;

    public delegate void TreeChopDeniedEvent(int requiredlevel);
    public event Action onTreeDepleted;
    public event TreeChopDeniedEvent onTreeChopDenied;
    [SerializeField] private Item fruitLogItem;
    private PlayerSkills playerSkills;
    private PlayerAnimationController playerController;
    private InventoryManager inventoryManager;
    private AudioSource audioSource;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<BoxCollider2D>();
        woodAmount = UnityEngine.Random.Range(treeData.minWoodAmount, treeData.maxWoodAmount + 1);

        playerSkills = player.GetComponent<PlayerSkills>();
        if (playerSkills == null)
        {
            Debug.LogWarning("PlayerSkills component not found on player!");
        }
        playerController = player.GetComponent<PlayerAnimationController>();
        if (playerController == null)
        {
            Debug.LogWarning("PlayerAnimationController component not found on player!");
        }
        // Initialize inventoryManager
        inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager not found in the scene!");
        }
        // Initialize AudioSource
        audioSource = GetComponent<AudioSource>();
        if (treeData.chopSound != null && audioSource != null)
        {
            audioSource.clip = treeData.chopSound;
        }

        onTreeChopDenied += HandleOnTreeChopDenied;
    }

    public void NotifyTreeChopDenied()
    {
        onTreeChopDenied?.Invoke(treeData.requiredLevel);
    }

    private void OnDisable()
    {
        onTreeChopDenied -= HandleOnTreeChopDenied;
    }

    private void HandleOnTreeChopDenied(int requirelevel)
    {
       var ChopDeniedEffect =  CreateFloatPrefab();
       ChopDeniedEffect.GetComponent<FloatingEffect>().SetText($"Tree requires Woodcutting lvl {requirelevel}", Color.red);
    }

    void Update()
    {
        if (isBeingChopped && player != null)
        {
            // Check if the inventory is full before chopping
            if (inventoryManager != null && inventoryManager.IsInventoryFull())
            {
                Debug.Log("Cannot chop tree: Inventory is full!");
                UIEventManager.Instance.NotifyStopAxeAnim();
                return; // Prevent chopping if inventory is full
            }

            chopTimer += Time.deltaTime;
            if (chopTimer >= effectiveChopTime)
            {
                chopTimer = 0f;
                // Play chop sound
                if (audioSource != null && audioSource.clip != null)
                {
                    audioSource.Play();
                }
                float successRate = treeData.baseSuccessRate;
                float chopTimeMultiplier = 1f; // Default value
                float successRateBonus = 0f;   // Default value

                if (playerSkills != null)
                {
                    int levelDifference = playerSkills.GetSkillLevel("Woodcutting") - treeData.treeLevel;
                    successRate += levelDifference * 0.05f;
                    Debug.Log($"Base Success Rate: {treeData.baseSuccessRate}, Level Difference: {levelDifference}, Level Bonus: {levelDifference * 0.05f}, Current Success Rate: {successRate}");

                    if (playerSkills.equippedTool != null)
                    {
                        ToolStats woodcuttingStats = playerSkills.equippedTool.toolStats.Find(stats => stats.skill == PlayerSkills.SkillType.Woodcutting);
                        if (woodcuttingStats.skill == PlayerSkills.SkillType.Woodcutting)
                        {
                            chopTimeMultiplier = woodcuttingStats.chopTimeMultiplier;
                            successRateBonus = woodcuttingStats.successRateBonus;
                            successRate += successRateBonus;
                            successRate += playerSkills.GetSkillLevel("Woodcutting") * successRateBonus * 0.1f;
                            Debug.Log($"Tool Stats - Chop Time Multiplier: {chopTimeMultiplier}, Success Rate Bonus: {successRateBonus}, Synergy Bonus: {playerSkills.GetSkillLevel("Woodcutting") * successRateBonus * 0.1f}, Final Success Rate: {successRate}");
                        }
                        else
                        {
                            Debug.Log("No Woodcutting stats found for equipped tool - using defaults.");
                        }
                    }
                }
                successRate = Mathf.Clamp(successRate, 0f, 1f);

                Debug.Log($"Clamped Success Rate: {successRate}, Critical Chance: {treeData.criticalChance}");

                if (UnityEngine.Random.value <= successRate)
                {
                    bool isCritical = UnityEngine.Random.value <= treeData.criticalChance;
                    int logsGained = isCritical ? 2 : 1;
                    Debug.Log($"Chop Attempt - Success: True, Critical: {isCritical}, Logs Gained: {logsGained}");
                    woodAmount--;

                    if (playerSkills != null)
                    {
                        playerSkills.AddWood(logsGained, fruitLogItem, treeData.expPerLog * logsGained);
                    }
                    else
                    {
                        Debug.LogWarning("PlayerSkills not found, cannot add logs or EXP!");
                    }

                    Debug.Log($"Chopped tree! {logsGained} log(s) gained. Wood remaining: {woodAmount}");

                    if (floatingEffectPrefab != null && foliage != null)
                    {
                        GameObject expEffect = CreateFloatPrefab();
                        string expText = "+" + (treeData.expPerLog * logsGained) + " EXP";
                        if (isCritical)
                        {
                            expText += " Crit! x2";
                            expEffect.GetComponent<FloatingEffect>().SetText(expText, Color.yellow);
                        }
                        else
                        {
                            expEffect.GetComponent<FloatingEffect>().SetText(expText, Color.green);
                        }//
                    }

                    if (woodAmount <= 0)
                    {
                        Debug.Log("Tree depleted!");
                        if (foliage != null)
                        {
                            foliage.enabled = false;
                        }
                        else
                        {
                            Debug.LogWarning("Foliage or spriteRenderer is not assigned!");
                        }
                        collider.enabled = false;
                        isBeingChopped = false;
                        UIEventManager.Instance.NotifyStopAxeAnim();
                        Invoke("RespawnTree", respawnDelay);
                    }
                }
                else
                {
                    chopTimer = 0f;
                    Debug.Log("Failed to chop tree!");
                }
            }
        }
    }

    private GameObject CreateFloatPrefab()
    {
        Vector3 foliageTop = foliage.transform.position;
        float foliageHeight = foliage.bounds.size.y;
        Vector3 effectPosition = foliageTop + new Vector3(0, foliageHeight * 0.5f + 0.5f, 0);
        GameObject floatingPrefabEffect = Instantiate(floatingEffectPrefab, effectPosition, Quaternion.identity);
        return floatingPrefabEffect;
    }

    public void StartChopping()
    {
        PlayerSkills playerSkills = FindObjectOfType<PlayerSkills>(); // Adjust based on how you access this
        if(playerSkills == null)
        {
            Debug.LogError("TreeResource: playerSkills object is null!");
            return;
        }
        if (playerSkills.equippedTool == null)
        {
            Debug.Log("TreeResource: equippedTool is null");
        }
        if (playerSkills.GetSkillLevel("Woodcutting") < treeData.requiredLevel)
        {
            Debug.Log("Player can't yet chop this level tree;");
            return;
        }

        ToolStats woodcuttingStats = playerSkills.equippedTool.toolStats.Find(stats => stats.skill == PlayerSkills.SkillType.Woodcutting);
        Debug.Log($"toolstats: successRateBonus {woodcuttingStats.successRateBonus} choptimemultiplier {woodcuttingStats.chopTimeMultiplier}");
        effectiveChopTime = treeData.chopTime * (woodcuttingStats.skill == PlayerSkills.SkillType.Woodcutting ? woodcuttingStats.chopTimeMultiplier : 1f);

        isBeingChopped = true;
    }

    public void StopChopping()
    {
        isBeingChopped = false;
        chopTimer = 0f;
        onTreeDepleted?.Invoke();
    }

    private void RespawnTree()
    {
        woodAmount = UnityEngine.Random.Range(treeData.minWoodAmount, treeData.maxWoodAmount + 1);
        foliage.enabled = true;
        collider.enabled = true;
        Debug.Log("Tree respawned");
    }

}