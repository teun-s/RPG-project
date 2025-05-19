using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlot : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button slotButton; // For clicking to show details

    private SkillsData skillData;
    private SkillsPanelManager skillsPanelManager; // Reference to parent manager

    public void Initialize(SkillsData skill, SkillsPanelManager manager)
    {
        skillData = skill;
        skillsPanelManager = manager;

        if (skillIcon != null)
        {
            skillIcon.sprite = skillData.Icon;
        }
        if (levelText != null)
        {
            levelText.text = "1/99"; // Will be updated dynamically
        }
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotClicked);
        }
    }

    public void UpdateDisplay(int currentLevel)
    {
        if (levelText != null && skillData != null)
        {
            levelText.text = $"{currentLevel}/{skillData.MaxLevel}";
        }
    }

    public string GetSkillName()
    {
        return skillData != null ? skillData.SkillName : "";
    }

    private void OnSlotClicked()
    {
        if (skillsPanelManager != null)
        {
            if (skillsPanelManager.skillDetailsPanel.activeSelf && skillsPanelManager.currentlyDisplayedSkill == skillData.SkillName)
            {
                skillsPanelManager.HideSkillDetails();
            }
            else
            {
                skillsPanelManager.ShowSkillDetails(skillData.SkillName, transform);
            }
        }
    }
}