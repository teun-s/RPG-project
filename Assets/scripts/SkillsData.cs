using UnityEngine;

[CreateAssetMenu(fileName = "SkillsData", menuName = "Skills/SkillsData")]
public class SkillsData : ScriptableObject
{
    [SerializeField] private int id;
    [SerializeField] private string skillName;
    [SerializeField] private Sprite icon;
    [SerializeField] private int maxLevel;

    public int Id => id;
    public string SkillName => skillName;
    public int MaxLevel => maxLevel;
    public Sprite Icon => icon;
}
