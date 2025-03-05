using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("背包配置")]
    public GameObject itemSlotPrefab;  // 物品槽预制体
    public Transform itemGridParent;   // 背包格子容器（需要挂载 Grid Layout Group 组件）
    public int inventorySize = 50;     // 背包总格子数
    public Sprite testSprite;

    [Header("装备预制体")]
    public GameObject equipmentPrefab;  // 装备预制体，预制体上应包含 EquipmentItem 和 DraggableItem 脚本

    // 记录背包中每个格子的装备，null 表示该槽为空
    private EquipmentItem[] inventoryItems;
    // 保存所有生成的物品槽引用，便于后续更新显示
    private List<GameObject> itemSlots = new List<GameObject>();

    void Start()
    {
        inventoryItems = new EquipmentItem[inventorySize];
        CreateItemSlots();
        AddTestItems();
    }

    // 动态生成所有背包格子
    void CreateItemSlots()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemGridParent);
            slot.name = "Slot " + i;
            itemSlots.Add(slot);
        }
    }

    public void AddNewItem(EquipmentType type, string name, Sprite icon)
    {
        // 实例化装备预制体
        GameObject newItem = Instantiate(equipmentPrefab);
        EquipmentItem equipmentItem = newItem.GetComponent<EquipmentItem>();
        if (equipmentItem != null)
        {
            equipmentItem.itemType = type;
            equipmentItem.itemName = name;
            equipmentItem.icon = icon;
            // 将装备加入背包
            AddItem(equipmentItem);
        }
        else
        {
            Debug.LogError("装备预制体缺少 EquipmentItem 组件！");
            Destroy(newItem);
        }
    }

    public void AddItem(EquipmentItem newItem)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventoryItems[i] == null)
            {
                inventoryItems[i] = newItem;
                // 将装备设置为对应槽的子物体，并定位到槽中心
                newItem.transform.SetParent(itemSlots[i].transform, false);
                newItem.transform.localPosition = Vector3.zero;
                Debug.Log("Added item " + newItem.itemName + " to slot " + i);
                return;
            }
        }
        Debug.Log("背包已满！");
        Destroy(newItem.gameObject);
    }

    void AddTestItems()
    {
        AddNewItem(EquipmentType.Helmet, "Test Helmet", testSprite);
        AddNewItem(EquipmentType.Chest, "Test Chest", testSprite);
        AddNewItem(EquipmentType.Leg, "Test Leg", testSprite);
        AddNewItem(EquipmentType.Shoes, "Test Shoes", testSprite);
        AddNewItem(EquipmentType.Weapon, "Test Weapon", testSprite);
        AddNewItem(EquipmentType.Accessory, "Test Accessory", testSprite);
    }
}