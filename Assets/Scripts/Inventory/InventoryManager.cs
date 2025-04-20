using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class EquipmentAndInventorySaveData
{
    public int[] allSeeds;
    public float currentHP;
}

public class InventoryManager : MonoBehaviour
{
    [Header("装备槽（10个）")]
    public Transform equipmentParent; // 这里放你“Equipment”节点，里面有 10 个子槽
    private const int EQUIPMENT_SLOTS_COUNT = 10; // 固定10个

    [Header("背包配置")]
    public GameObject itemSlotPrefab;  // 物品槽预制体
    public Transform itemGridParent;   // 背包格子容器（有 Grid Layout Group）
    public int inventorySize = 50;     // 背包总格子数

    [Header("装备预制体")]
    public GameObject equipmentPrefab;  // 装备预制体（需挂有 EquipmentItem 脚本）

    private List<GameObject> backpackSlots = new List<GameObject>(); // 背包槽引用
    public PlayerStats playerStats;
    private string saveFilePath;

    void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "player_inventory.json");
        CreateBackpackSlots();

        // 如果有存档，就加载；没有则测试添加一件随机物品
        if (File.Exists(saveFilePath))
        {
            LoadAll();
        }
        else
        {
            // 测试添加一件随机物品
            AddNewItemWithSeed(-1);
        }
    }

    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //AddNewItemWithSeed(-1);
        }
    }

    void OnApplicationQuit()
    {
        SaveAll();
    }

    /// <summary>
    /// 动态生成背包格子
    /// </summary>
    void CreateBackpackSlots()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemGridParent);
            slot.name = "BackpackSlot " + i;
            backpackSlots.Add(slot);
        }
    }

    /// <summary>
    /// 添加一件装备（种子为 seed，-1 表示随机种子），
    /// 由 EquipmentItem 根据种子决定全部特性
    /// 先尝试放入背包
    /// </summary>
    public EquipmentItem AddNewItemWithSeed(int seed)
    {
        GameObject newItem = Instantiate(equipmentPrefab);
        EquipmentItem eqItem = newItem.GetComponent<EquipmentItem>();
        if (eqItem != null)
        {
            if (seed == -1)
                seed = Random.Range(1, int.MaxValue);
            eqItem.equipmentSeed = seed;
            eqItem.GenerateStats();

            // 将装备放到背包的第一个空槽
            AddItemToBackpack(eqItem);

            return eqItem;  //返回新生成的装备
        }
        else
        {
            Debug.LogError("装备预制体缺少 EquipmentItem 组件！");
            Destroy(newItem);
            return null;
        }
    }

    /// <summary>
    /// 将装备添加到背包中第一个空槽
    /// </summary>
    public void AddItemToBackpack(EquipmentItem newItem)
    {
        foreach (GameObject slot in backpackSlots)
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

    /// <summary>
    /// 保存玩家装备栏（前10个）+ 背包（剩余）到一个整型数组 allSeeds，然后写入 JSON
    /// 没有装备的槽保存 -1
    /// </summary>
    public void SaveAll()
    {
        if (string.IsNullOrEmpty(saveFilePath))
        {
            return;
        }
        // 我们需要存储 10 个装备槽 + 背包槽数量
        int totalSlots = EQUIPMENT_SLOTS_COUNT + backpackSlots.Count;
        int[] allSeeds = new int[totalSlots];

        // 1) 保存装备槽（前10位）
        for (int i = 0; i < EQUIPMENT_SLOTS_COUNT; i++)
        {
            Transform slot = equipmentParent.GetChild(i); // 获取第 i 个子槽
            if (slot.childCount > 0)
            {
                EquipmentItem eq = slot.GetChild(0).GetComponent<EquipmentItem>();
                if (eq != null)
                    allSeeds[i] = eq.equipmentSeed;
                else
                    allSeeds[i] = -1;
            }
            else
            {
                allSeeds[i] = -1;
            }
        }

        // 2) 保存背包槽
        for (int i = 0; i < backpackSlots.Count; i++)
        {
            int index = EQUIPMENT_SLOTS_COUNT + i; // 对应 allSeeds 中的位置
            Transform slot = backpackSlots[i].transform;
            if (slot.childCount > 0)
            {
                EquipmentItem eq = slot.GetChild(0).GetComponent<EquipmentItem>();
                if (eq != null)
                    allSeeds[index] = eq.equipmentSeed;
                else
                    allSeeds[index] = -1;
            }
            else
            {
                allSeeds[index] = -1;
            }
        }

        // 打包到数据结构
        EquipmentAndInventorySaveData saveData = new EquipmentAndInventorySaveData();
        saveData.allSeeds = allSeeds;
        saveData.currentHP = playerStats.currentHP;
        Debug.Log("存档角色HP: " + playerStats.currentHP);

        // 序列化为 JSON
        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("存档完成: " + saveFilePath);
    }

    /// <summary>
    /// 从存档文件加载数据，根据种子重建装备，并放回对应槽位
    /// 前10个种子对应玩家装备栏，后面的是背包物品
    /// </summary>
    public void LoadAll()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("未找到存档文件，无法加载");
            return;
        }

        string json = File.ReadAllText(saveFilePath);
        EquipmentAndInventorySaveData loadData = JsonUtility.FromJson<EquipmentAndInventorySaveData>(json);
        if (loadData == null || loadData.allSeeds == null)
        {
            Debug.LogWarning("存档文件无效或空");
            return;
        }

        playerStats.currentHP = loadData.currentHP;

        int[] allSeeds = loadData.allSeeds;

        // 先清理装备栏和背包槽
        // 装备栏
        for (int i = 0; i < EQUIPMENT_SLOTS_COUNT; i++)
        {
            Transform slot = equipmentParent.GetChild(i);
            if (slot.childCount > 0)
            {
                Destroy(slot.GetChild(0).gameObject);
            }
        }
        // 背包
        foreach (GameObject slot in backpackSlots)
        {
            if (slot.transform.childCount > 0)
            {
                Destroy(slot.transform.GetChild(0).gameObject);
            }
        }

        // 重新生成装备栏的物品
        for (int i = 0; i < EQUIPMENT_SLOTS_COUNT; i++)
        {
            if (i < allSeeds.Length)
            {
                int seed = allSeeds[i];
                if (seed != -1)
                {
                    // 用 seed 生成装备
                    GameObject newItem = Instantiate(equipmentPrefab);
                    EquipmentItem eq = newItem.GetComponent<EquipmentItem>();
                    eq.equipmentSeed = seed;
                    eq.GenerateStats();
                    // 放进对应的装备槽
                    Transform slot = equipmentParent.GetChild(i);
                    eq.transform.SetParent(slot, false);
                    eq.transform.localPosition = Vector3.zero;
                    Debug.Log($"加载装备槽{i}, seed={seed}, 物品名={eq.itemName}");
                }
            }
        }

        // 重新生成背包物品
        int backpackCount = backpackSlots.Count;
        for (int i = 0; i < backpackCount; i++)
        {
            int index = EQUIPMENT_SLOTS_COUNT + i;
            if (index < allSeeds.Length)
            {
                int seed = allSeeds[index];
                if (seed != -1)
                {
                    // 用 seed 生成装备
                    GameObject newItem = Instantiate(equipmentPrefab);
                    EquipmentItem eq = newItem.GetComponent<EquipmentItem>();
                    eq.equipmentSeed = seed;
                    eq.GenerateStats();
                    // 放进背包槽
                    eq.transform.SetParent(backpackSlots[i].transform, false);
                    eq.transform.localPosition = Vector3.zero;
                    Debug.Log($"加载背包槽{i}, seed={seed}, 物品名={eq.itemName}");
                }
            }
        }

        Debug.Log("读取存档完成");
    }
}
