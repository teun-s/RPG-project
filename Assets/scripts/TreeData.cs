using UnityEngine;

[CreateAssetMenu(fileName = "TreeData", menuName = "Game/TreeData", order = 1)]
public class TreeData : ScriptableObject
{
    public int treeLevel = 1;
    public float expPerLog = 25f;
    public float baseSuccessRate = 0.7f;
    public float criticalChance = 0.05f;
    public int minWoodAmount = 3;
    public int maxWoodAmount = 6;
    public float chopTime = 1f;
    public int requiredLevel;
    public AudioClip chopSound; // Sound to play when chopping

}
