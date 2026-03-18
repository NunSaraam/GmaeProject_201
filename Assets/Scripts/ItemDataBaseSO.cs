using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataBaseSO", menuName = "Inventory/DataBase")]
public class ItemDataBaseSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();             //ItemSO를 리스트로 관리

    //캐싱을 위한 Dictrionary
    private Dictionary<int, ItemSO> itemsById;                  //ID로 아이템 찾기
    private Dictionary<string, ItemSO> itemsByName;             //이름으로 아이템 찾기


    public void Init()
    {
        itemsById = new Dictionary<int, ItemSO>();
        itemsByName = new Dictionary<string, ItemSO>();

        foreach (var item in items)
        {
            itemsById[item.id] = item;
            itemsByName[item.itemName] = item;
        }
    }

    public ItemSO GetItemById(int id)
    {
        if (itemsById == null)
        {
            Init();
        }

        if (itemsById.TryGetValue(id, out ItemSO item))
            return item;

        return null;
    }

    //이름으로 아이템 찾기

    public ItemSO GetItemByName(string name)
    {
        if (itemsByName == null)
        {
            Init();

        }

        if (itemsByName.TryGetValue(name, out ItemSO item))
            return item;

        return null;
    }

    //타입으로 아이템 필터링
    public List<ItemSO> GetItemByType(ItemType type)
    {
        return items.FindAll(item => item.itemType == type);
    }
}
