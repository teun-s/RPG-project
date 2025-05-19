using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class SkillsPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject skillSlotPrefab; // Prefab for skill slots
    [SerializeField] private Transform skillSlotContainer; // GridLayoutGroup transform
    [SerializeField] private TextMeshProUGUI currentExpText; // Current EXP text
    [SerializeField] private TextMeshProUGUI nextExpText; // Next level EXP text

    private PlayerSkills playerSkills;
    private List<SkillSlot> skillSlots = new List<SkillSlot>();
    private RectTransform skillDetailsPanelRect;
    private List<SkillsData> skills = new List<SkillsData>();

    public GameObject skillDetailsPanel;
    public string currentlyDisplayedSkill = "";
    void Start()
    {
        playerSkills = FindFirstObjectByType<PlayerSkills>();
        if (playerSkills == null)
        {
            Debug.LogError("PlayerSkills not found in the scene!");
        }
        if (skillSlotPrefab == null || skillSlotContainer == null || skillDetailsPanel == null || currentExpText == null || nextExpText == null)
        {
            Debug.LogError("One or more UI elements not assigned in SkillsPanelManager!");
        }

        SkillsData[] loadedSkills = Resources.LoadAll<SkillsData>("Skills");
        if (loadedSkills.Count() <= 0)
        {
            Debug.Log("No skills loaded");
        }
        skills.AddRange(loadedSkills);
        skills.Sort((a, b) => a.Id.CompareTo(b.Id));
        if (skills.Count <= 0)
        {
            Debug.Log("Skills did not load from Assets/Resources in skillspanelmanager!!");
        }

        // Ensure the details panel is hidden initially
        if (skillDetailsPanel != null)
        {
            skillDetailsPanel.SetActive(false);
        }

        skillDetailsPanelRect = skillDetailsPanel.GetComponent<RectTransform>();
        if (skillDetailsPanelRect == null)
        {
            Debug.LogError("SkillDetailsPanel requires a RectTransform component!");
        }
        // Dynamically create skill slots
        if (playerSkills != null)
        {
            playerSkills.OnSkillProgressChanged += OnSkillProgressChangedHandler;
        }
        CreateSkillSlots();
        UpdateSkillsDisplay();
    }

    private void CreateSkillSlots()
    {
        if (skills == null) return;

        foreach (SkillsData skill in skills)
        {
            GameObject slotObject = Instantiate(skillSlotPrefab, skillSlotContainer);
            SkillSlot slot = slotObject.GetComponent<SkillSlot>();
            if (slot != null)
            {
                slot.Initialize(skill, this);
                skillSlots.Add(slot);
            }
        }
    }

    public void UpdateSkillsDisplay()
    {
        foreach (SkillSlot slot in skillSlots)
        {
            string skillName = slot.GetSkillName();
            int level = playerSkills != null ? playerSkills.GetSkillLevel(skillName) : 1;
            slot.UpdateDisplay(level);
        }

        // Update details panel if visible
        if (skillDetailsPanel.activeSelf && !string.IsNullOrEmpty(currentlyDisplayedSkill))
        {
            ShowSkillDetails(currentlyDisplayedSkill,null);
        }
    }

    public void ShowSkillDetails(string skillName, Transform slotTransform)
    {
        currentlyDisplayedSkill = skillName;
        if (playerSkills != null)
        {
            float currentExp = playerSkills.GetSkillExp(skillName);
            float nextExp = playerSkills.GetRequiredExpForNextLevel(skillName);
            currentExpText.text = $"Current EXP: {currentExp}";
            nextExpText.text = $"Remaining EXP: {nextExp}";
            skillDetailsPanel.SetActive(true);
            Debug.Log($"Showing details for {skillName}: Current EXP = {currentExp}, Next Level EXP = {nextExp}");

            if (slotTransform != null && skillDetailsPanelRect != null)
            {
                RectTransform slotRect = slotTransform.GetComponent<RectTransform>();
                if (slotRect != null)
                {
                    Vector2 slotPos = slotRect.anchoredPosition;
                    float offsetY = -slotRect.rect.height - 10; // Below the slot, with 10 units spacing
                    skillDetailsPanelRect.anchoredPosition = new Vector2(slotPos.x, slotPos.y + offsetY);
                }
            }
        }
    }

    public bool SkillDetailPanelState()
    {
        return skillDetailsPanel.activeSelf;
    }

    public void HideSkillDetails()
    {
        skillDetailsPanel.SetActive(false);
        currentlyDisplayedSkill = "";
        Debug.Log("Hid skill details panel");
    }

    void OnDestroy()
    {
        if (playerSkills != null)
        {
            playerSkills.OnSkillProgressChanged -= OnSkillProgressChangedHandler;
        }
    }

    private void OnSkillProgressChangedHandler(string skillName)
    {
        UpdateSkillsDisplay();
    }

    private void OnEnable()
    {
        UIEventManager.Instance.OnUIViewSwitched += HandleViewSwitched;
        UIEventManager.Instance.OnUIClosed += HandleViewClosed;
    }

    private void OnDisable()
    {
        UIEventManager.Instance.OnUIViewSwitched -= HandleViewSwitched;
        UIEventManager.Instance.OnUIClosed -= HandleViewClosed;
    }

    private void HandleViewSwitched()
    {
        HideSkillDetails();
    }

    private void HandleViewClosed()
    {
        HideSkillDetails();
    }
}