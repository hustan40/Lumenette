using UnityEngine;
[CreateAssetMenu(fileName = "Item", menuName = "Objects/Imem")]
public class Item : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public int MaxStack = 1;
    public ItemType Type;

    public enum ItemType
    {
        Weapon,
        Consumable,
        Resource,
        Misc,
        Throw
    }
}
