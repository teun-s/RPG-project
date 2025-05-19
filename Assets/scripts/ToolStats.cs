using static PlayerSkills;

[System.Serializable]
public struct ToolStats
{
    public SkillType skill;             // The skill this applies to
    public float chopTimeMultiplier;    // E.g., 0.8f = 20% faster
    public float successRateBonus;      // E.g., 0.1f = +10% success
    // Add more fields (e.g., miningEfficiency, catchRate) as needed
}