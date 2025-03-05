using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ItemStat
{
    // 作为主词条和次要词条均可出现的属性
    Defense,
    HP,
    Attack,
    AttackSpeed,
    // 以下为次要属性候选
    MoveSpeed,
    MagicResist,
    CritRate,
    CritDamage,
    Luck,
    LightRadius
}

[System.Serializable]
public class StatEntry
{
    public ItemStat stat;
    public float value;

    public StatEntry(ItemStat stat, float value)
    {
        this.stat = stat;
        this.value = value;
    }

    public override string ToString()
    {
        return $"{stat}: {value}";
    }
}

public class EquipmentItem : MonoBehaviour
{
    public EquipmentType itemType;   // 装备类型
    public string itemName;          // 装备名称
    public Sprite icon;              // 装备图标

    [SerializeField]
    private List<StatEntry> mainStats = new List<StatEntry>(); // 主词条
    [SerializeField]
    private List<StatEntry> subStats = new List<StatEntry>();  // 次要词条

    public IReadOnlyList<StatEntry> MainStats => mainStats;
    public IReadOnlyList<StatEntry> SubStats => subStats;

    /// <summary>
    /// 生成装备的词条
    /// </summary>
    public void GenerateStats()
    {
        mainStats.Clear();
        subStats.Clear();

        // 定义可供次要词条随机的池
        List<ItemStat> substatsPool = new List<ItemStat>()
        {
            ItemStat.MoveSpeed,
            ItemStat.Defense,
            ItemStat.HP,
            ItemStat.MagicResist,
            ItemStat.Attack,
            ItemStat.CritRate,
            ItemStat.CritDamage,
            ItemStat.AttackSpeed,
            ItemStat.Luck,
            ItemStat.LightRadius
        };

        // 先生成装备的主词条，根据装备类型
        switch (itemType)
        {
            case EquipmentType.Helmet:
            case EquipmentType.Chest:
            case EquipmentType.Leg:
            case EquipmentType.Shoes:
                // 盔甲主词条：防御（1～5）和生命（10～100）
                mainStats.Add(new StatEntry(ItemStat.Defense, Random.Range(1, 6))); // 1到5
                mainStats.Add(new StatEntry(ItemStat.HP, Random.Range(10, 101)));    // 10到100
                break;
            case EquipmentType.Weapon:
                // 武器主词条：攻击（10～30）和攻速（2～5）
                mainStats.Add(new StatEntry(ItemStat.Attack, Random.Range(10, 31)));      // 10到30
                mainStats.Add(new StatEntry(ItemStat.AttackSpeed, Random.Range(2, 6)));   // 2到5
                break;
            case EquipmentType.Accessory:
                // 饰品主词条：从次要词条池中随机选1条，数值翻倍
                ItemStat accessoryStat = TakeRandomStat(substatsPool);
                float accessoryValue = GenerateSubStatValue(accessoryStat);
                // 翻倍
                mainStats.Add(new StatEntry(accessoryStat, accessoryValue * 2));
                break;
        }

        // 生成次要词条：所有装备均随机生成 1～5 条，判定成功率依次为 100%, 80%, 60%, 40%, 20%
        float[] chances = new float[] { 1.0f, 0.8f, 0.6f, 0.4f, 0.2f };
        for (int i = 0; i < chances.Length; i++)
        {
            if (substatsPool.Count == 0) break;
            if (Random.value < chances[i])
            {
                // 从池中随机选择一个词条，并生成其值
                ItemStat chosen = TakeRandomStat(substatsPool);
                float value = GenerateSubStatValue(chosen);
                subStats.Add(new StatEntry(chosen, value));
            }
            else
            {
                break; // 判定失败则停止
            }
        }
    }

    /// <summary>
    /// 根据次要词条类型生成随机数值
    /// </summary>
    private float GenerateSubStatValue(ItemStat stat)
    {
        switch (stat)
        {
            case ItemStat.MoveSpeed:
                // 随机 0.2 到 0.5，0.1 的整数倍
                int msSteps = Random.Range(0, 4); // 0,1,2,3
                return 0.2f + 0.1f * msSteps;
            case ItemStat.Defense:
                // 次要防御：随机整数2到5（百分比）
                return Random.Range(2, 6);
            case ItemStat.HP:
                // 次要生命：随机整数2到10（百分比）
                return Random.Range(2, 11);
            case ItemStat.MagicResist:
                return Random.Range(2, 6);
            case ItemStat.Attack:
                // 次要攻击：随机整数2到5（百分比）
                return Random.Range(2, 6);
            case ItemStat.CritRate:
                return Random.Range(2, 6);
            case ItemStat.CritDamage:
                return Random.Range(5, 21);
            case ItemStat.AttackSpeed:
                // 随机 0.1 到 0.5，0.1 的整数倍
                int asSteps = Random.Range(0, 5); // 0到4
                return 0.1f + 0.1f * asSteps;
            case ItemStat.Luck:
                return Random.Range(1, 6);
            case ItemStat.LightRadius:
                // 固定为50%
                return 50f;
            default:
                return 0f;
        }
    }

    /// <summary>
    /// 从列表中随机取一个ItemStat并移除（避免重复），如果需要允许重复，则不移除。
    /// </summary>
    private ItemStat TakeRandomStat(List<ItemStat> list)
    {
        int idx = Random.Range(0, list.Count);
        ItemStat stat = list[idx];
        list.RemoveAt(idx);
        return stat;
    }
}