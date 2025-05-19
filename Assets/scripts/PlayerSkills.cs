using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    [SerializeField]
    private class SkillProgress
    {
        public int level = 1;
        public float exp = 0f;
    }

    public enum SkillType
    {
        Woodcutting,
        Mining,
        Fishing
    // Add more skills later as needed
    }

    static readonly float[] expToNextLevel = new float[99];
    static float baseExp = 100f;
    static float exponent = 1.2f;
    public Item equippedTool;

    static PlayerSkills()
    {
        for (int i = 0; i < expToNextLevel.Length; i++)
        {
            expToNextLevel[i] = baseExp * Mathf.Pow(i + 1, exponent);
        }
    }

    private int woodCount = 0;
    [SerializeField] private InventoryManager inventoryManager;
    private Dictionary<string, SkillProgress> skillProgress = new Dictionary<string, SkillProgress>();
   

    //Add an event to trigger on skill exp changed
    public event System.Action<string> OnSkillProgressChanged;
    void Start()
    {

        // Load skill progress from PlayerPrefs
        LoadSkillProgress();
        inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager == null)
        {
            Debug.LogWarning("InventoryManager not found in the scene!");
        }
        equippedTool = Resources.Load<Item>("Items/Stone_Axe"); // Adjust path to your axe
    }

    public int GetSkillLevel(string skillName)
    {
        if (!skillProgress.ContainsKey(skillName))
        {
            skillProgress[skillName] = new SkillProgress();
        }
        return skillProgress[skillName].level;
    }

    public float GetSkillExp(string skillName)
    {
        if (!skillProgress.ContainsKey(skillName))
        {
            skillProgress[skillName] = new SkillProgress();
        }
        return skillProgress[skillName].exp;
    }

    public float GetRequiredExpForNextLevel(string skillName)
    {
        if (!skillProgress.ContainsKey(skillName))
        {
            skillProgress[skillName] = new SkillProgress();
        }
        int level = skillProgress[skillName].level;
        if (level >= 99) return 0f;
        return expToNextLevel[level - 1] - skillProgress[skillName].exp;
    }

    private void LoadSkillProgress()
    {
        // Load all skills from PlayerPrefs
        string[] skillNames = new string[] { "Woodcutting", "Mining" }; // Add more skills as needed
        foreach (string skillName in skillNames)
        {
            if (PlayerPrefs.HasKey(skillName + "_Level"))
            {
                SkillProgress progress = new SkillProgress
                {
                    level = PlayerPrefs.GetInt(skillName + "_Level", 1),
                    exp = PlayerPrefs.GetFloat(skillName + "_Exp", 0f)
                };
                skillProgress[skillName] = progress;
            }

            OnSkillProgressChanged?.Invoke(skillName);
        }
    }

    private void SaveSkillProgress(string skillName)
    {
        if (skillProgress.ContainsKey(skillName))
        {
            SkillProgress progress = skillProgress[skillName];
            PlayerPrefs.SetInt(skillName + "_Level", progress.level);
            PlayerPrefs.SetFloat(skillName + "_Exp", progress.exp);
            PlayerPrefs.Save();
        }
    }

    public void AddWood(int amount, Item logItem, float exp)
    {
        woodCount += amount;
        Debug.Log($"Collected {amount} wood! Total wood: {woodCount}");

        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager is null in PlayerSkills!");
        }
        if (logItem == null)
        {
            Debug.LogError("LogItem is null in PlayerSkills.AddWood!");
        }
        if (inventoryManager != null && logItem != null)
        {
            for (int i = 0; i < amount; i++)
            {
                Item log = logItem.Clone();
                log.itemCount = 1;
                Debug.Log($"Attempting to add 1 {log.itemID} to inventory...");
                if (!inventoryManager.AddItemToInventory(log))
                {
                    Debug.Log($"Inventory is full! Dropped {amount - i} Log(s) of type {log.itemID}.");
                    break;
                }
                Debug.Log($"1 {log.itemID} added to inventory!");
            }
        }
        AddSkillExp("Woodcutting", exp);
    }

    // Generic method to add EXP to any skill
    public void AddSkillExp(string skillName, float expGained)
    {
        if (!skillProgress.ContainsKey(skillName))
        {
            skillProgress[skillName] = new SkillProgress();
        }

        SkillProgress progress = skillProgress[skillName];
        progress.exp += expGained;
        Debug.Log($"Added {expGained} EXP to {skillName}. Total: {progress.exp}");

        // Check for level up
        int maxLevel = 99; // Could be fetched from SkillData if needed
        while (progress.level < maxLevel && progress.exp >= expToNextLevel[progress.level - 1])
        {
            progress.exp -= expToNextLevel[progress.level - 1];
            progress.level++;
            Debug.Log($"{skillName} leveled up to {progress.level}!");
        }

        // Save progress
        SaveSkillProgress(skillName);
        OnSkillProgressChanged?.Invoke(skillName);
    }

    //private void AddWoodcuttingExp(float exp)
    //{
    //    woodcuttingExp += exp;
    //    Debug.Log($"Gained {exp} Woodcutting EXP! Total EXP: {woodcuttingExp}");

    //    int nextLevel = woodcuttingLevel + 1;
    //    if (nextLevel <= expToLevel.Length && woodcuttingExp >= expToLevel[nextLevel - 1])
    //    {
    //        woodcuttingLevel = nextLevel;
    //        Debug.Log($"Leveled up to Woodcutting Level {woodcuttingLevel}!");
    //    }
    //}

    //public float GetWoodcuttingEXP()
    //{
    //    return woodcuttingExp;
    //}

    //public float GetRemainingExp()
    //{
    //    var levelexp = expToLevel[woodcuttingLevel - 1];
    //    var remainingexp = levelexp - woodcuttingExp;
    //    return remainingexp;
    //}

    //public int GetRequiredExpForNextLevel()
    //{
    //    return expToLevel[woodcuttingLevel - 1];
    //}

    //public int GetWoodcuttingLevel()
    //{
    //    return woodcuttingLevel;
    //}
}