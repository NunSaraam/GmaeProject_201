using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public int id;
    public string itemName;
    public string nameENG;
    public string description;

    public ItemType itemType;
    public int price;
    public int power;
    public int level;
    public bool isStackable;
    public Sprite icon;

    public override string ToString()
    {
        return $"[{id}] {itemName} ({itemType}) - 가격 : {price}골드, 속성 : {power}";
    }

    public string DisplayName
    {
        get { return string.IsNullOrEmpty(nameENG) ? itemName : nameENG; }
    }
}
