using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("背包配置")]
    public GameObject itemSlotPrefab;  // 物品槽预制体
    public Transform itemGridParent;   // 背包格子容器（挂有 Grid Layout Group 组件）
    public int inventorySize = 50;     // 背包总格子数

    [Header("装备预制体")]
    public GameObject equipmentPrefab;  // 装备预制体（需挂有 EquipmentItem 脚本）

    // 保存所有生成的物品槽引用
    private List<GameObject> itemSlots = new List<GameObject>();

    void Start()
    {
        CreateItemSlots();
        // 测试：添加一个随机装备
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
    /// 随机添加装备
    /// 装备所有特性均由传入的随机种子决定
    /// </summary>
    public void AddRandomItem()
    {
        // 实例化装备预制体
        GameObject newItem = Instantiate(equipmentPrefab);
        EquipmentItem equipmentItem = newItem.GetComponent<EquipmentItem>();
        if (equipmentItem != null)
        {
            // 生成一个随机种子
            int seed = Random.Range(1, int.MaxValue);
            equipmentItem.equipmentSeed = seed;
            // 调用装备生成方法，内部会用该种子控制所有随机数生成
            equipmentItem.GenerateStats();

            // 将装备添加到第一个空槽中
            AddItem(equipmentItem);
        }
        else
        {
            Debug.LogError("装备预制体缺少 EquipmentItem 组件！");
            Destroy(newItem);
        }
    }

    /// <summary>
    /// 将装备添加到背包中第一个没有子物体的槽里
    /// </summary>
    public void AddItem(EquipmentItem newItem)
    {
        foreach (GameObject slot in itemSlots)
        {
            if (slot.transform.childCount == 0)
            {
                newItem.transform.SetParent(slot.transform, false);
                newItem.transform.localPosition = Vector3.zero;
                Debug.Log("Added item " + newItem.itemName + " to " + slot.name);
                return;
            }
        }
        Debug.Log("背包已满！");
        Destroy(newItem.gameObject);
    }
}
