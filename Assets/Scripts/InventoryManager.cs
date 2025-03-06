using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("背包配置")]
    public GameObject itemSlotPrefab;  // 物品槽预制体
    public Transform itemGridParent;   // 背包格子容器（需要挂载 Grid Layout Group 组件）
    public int inventorySize = 50;     // 背包总格子数
    public Sprite testSprite;          // 用于测试的贴图

    [Header("装备预制体")]
    public GameObject equipmentPrefab;  // 装备预制体，需挂 EquipmentItem（包含 EquipmentRarity 字段）和 DraggableItem 脚本

    [Header("玩家运气（影响稀有度概率）")]
    public int playerLuck = 0;  // 运气值，建议范围为正负整数

    // 记录背包中每个格子的装备（null 表示槽为空）
    private EquipmentItem[] inventoryItems;
    // 保存所有生成的物品槽引用
    private List<GameObject> itemSlots = new List<GameObject>();

    void Start()
    {
        inventoryItems = new EquipmentItem[inventorySize];
        CreateItemSlots();
        // 添加一个随机装备作为奖励
        AddRandomItem();
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

    /// <summary>
    /// 添加装备到背包：传入装备类型、稀有度、名称、图标
    /// </summary>
    public void AddNewItem(EquipmentType type, EquipmentRarity rarity, string name, Sprite icon)
    {
        GameObject newItem = Instantiate(equipmentPrefab);
        EquipmentItem equipmentItem = newItem.GetComponent<EquipmentItem>();
        if (equipmentItem != null)
        {
            equipmentItem.itemType = type;
            equipmentItem.rarity = rarity;
            equipmentItem.itemName = name;
            equipmentItem.icon = icon;
            equipmentItem.GenerateStats();
            AddItem(equipmentItem);
        }
        else
        {
            Debug.LogError("装备预制体缺少 EquipmentItem 组件！");
            Destroy(newItem);
        }
    }

    /// <summary>
    /// 将装备加入背包（添加到第一个空槽）
    /// </summary>
    public void AddItem(EquipmentItem newItem)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventoryItems[i] == null)
            {
                inventoryItems[i] = newItem;
                // 将装备设为对应槽的子物体，并居中显示
                newItem.transform.SetParent(itemSlots[i].transform, false);
                newItem.transform.localPosition = Vector3.zero;
                Debug.Log("Added item " + newItem.itemName + " to slot " + i);
                return;
            }
        }
        Debug.Log("背包已满！");
        Destroy(newItem.gameObject);
    }

    /// <summary>
    /// 随机添加装备：
    /// 装备类型：盔甲70%（随机选取 Helmet/Chest/Leg/Shoes）、武器20%、饰品10%；
    /// 装备稀有度：Legendary基础5%，Rare基础15%，剩余为Normal；
    /// 稀有度的最终概率 = 基础概率 * (20 + playerLuck)/20；
    /// </summary>
    public void AddRandomItem()
    {
        // 随机确定装备类型
        float randType = Random.Range(0f, 1f);
        EquipmentType chosenType;
        if (randType < 0.7f)
        {
            // 盔甲：随机选择 Helmet、Chest、Leg、Shoes
            EquipmentType[] armors = new EquipmentType[] { EquipmentType.Helmet, EquipmentType.Chest, EquipmentType.Leg, EquipmentType.Shoes };
            chosenType = armors[Random.Range(0, armors.Length)];
        }
        else if (randType < 0.7f + 0.2f)
        {
            chosenType = EquipmentType.Weapon;
        }
        else
        {
            chosenType = EquipmentType.Accessory;
        }

        // 确定装备稀有度，先计算运气修正因子
        float luckFactor = (20 + playerLuck) / 20f;
        float legendaryChance = 0.05f * luckFactor;  // Legendary基础5%
        float rareChance = 0.15f * luckFactor;         // Rare基础15%
        float randRarity = Random.Range(0f, 1f);
        EquipmentRarity chosenRarity;
        if (randRarity < legendaryChance)
        {
            chosenRarity = EquipmentRarity.Legendary;
        }
        else if (randRarity < legendaryChance + rareChance)
        {
            chosenRarity = EquipmentRarity.Rare;
        }
        else
        {
            chosenRarity = EquipmentRarity.Normal;
        }

        // 生成装备名称（可根据需求自定义）
        string name = $"{chosenType} {chosenRarity}";
        // 添加装备到背包
        AddNewItem(chosenType, chosenRarity, name, testSprite);
    }
}
