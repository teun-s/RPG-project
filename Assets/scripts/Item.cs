using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;



public enum EquipmentType
{
    None,
    Head,
    Chest,
    Boot,
    Weapon
}

public enum ToolType
{
    None,
    Axe,
    Sword,
    Pickaxe
}

[CreateAssetMenu(fileName ="Item",menuName="ScriptableObjects/Item")]
public class Item : ScriptableObject
{
    public int itemID;
    public int itemCount;
    public Sprite itemIcon;
    public bool canStack = false;
    public ToolType toolType;
    public EquipmentType equipmentType;
    public List<ToolStats> toolStats;
}

public static class ScriptableObjectExtension
{
    /// <summary>
    /// Creates and returns a clone of any given scriptable object.
    /// </summary>
    public static T Clone<T>(this T scriptableObject) where T : ScriptableObject
    {
        if (scriptableObject == null)
        {
            Debug.LogError($"ScriptableObject was null. Returning default {typeof(T)} object.");
            return (T)ScriptableObject.CreateInstance(typeof(T));
        }

        T instance = UnityEngine.Object.Instantiate(scriptableObject);
        instance.name = scriptableObject.name; // remove (Clone) from name
        return instance;
    }
}