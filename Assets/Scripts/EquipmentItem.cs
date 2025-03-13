using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EquipmentRarity
{
    Normal,
    Rare,
    Legendary
}

public enum EquipmentType
{
    Helmet,
    Chest,
    Leg,
    Shoes,
    Accessory,
    Weapon
}

public enum ItemStat
{
    // 主词条可能
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
    [Header("装备基础数据")]
    // 所有装备的生成均由 equipmentSeed 决定，如果为0则随机生成一个
    public int equipmentSeed = 0;
    public EquipmentType itemType;       // 装备类型（由种子随机生成）
    public EquipmentRarity rarity;       // 装备稀有度（由种子随机生成）
    public string itemName;              // 装备名称（由类型和稀有度生成）
    public Sprite icon;                  // 装备图标（备用）

    [Header("显示组件")]
    public Image iconImage;              // 用于显示装备图标的 Image
    public Image backgroundImage;        // 用于显示背景颜色的 Image

    [Header("类型对应贴图")]
    public Sprite helmetSprite;
    public Sprite chestSprite;
    public Sprite legSprite;
    public Sprite shoesSprite;
    public Sprite accessorySprite;
    public Sprite weaponSprite;

    [SerializeField]
    private List<StatEntry> mainStats = new List<StatEntry>(); // 主词条
    [SerializeField]
    private List<StatEntry> subStats = new List<StatEntry>();  // 次要词条

    public IReadOnlyList<StatEntry> MainStats => mainStats;
    public IReadOnlyList<StatEntry> SubStats => subStats;

    /// <summary>
    /// 生成装备所有特性：装备类型、稀有度、装备名称、主词条和次要词条及其数值
    /// 所有随机数均由 equipmentSeed 控制
    /// </summary>
    public void GenerateStats()
    {
        // 如果种子为0，则生成一个随机种子
        if (equipmentSeed == 0)
            equipmentSeed = Random.Range(1, int.MaxValue);
        // 初始化随机状态
        Random.InitState(equipmentSeed);

        // 根据种子随机决定装备类型
        float randType = Random.Range(0f, 1f);
        if (randType < 0.7f)
        {
            EquipmentType[] armors = new EquipmentType[] { EquipmentType.Helmet, EquipmentType.Chest, EquipmentType.Leg, EquipmentType.Shoes };
            itemType = armors[Random.Range(0, armors.Length)];
        }
        else if (randType < 0.7f + 0.2f)
        {
            itemType = EquipmentType.Weapon;
        }
        else
        {
            itemType = EquipmentType.Accessory;
        }

        // 根据种子随机决定稀有度
        float legendaryChance = 0.05f;
        float rareChance = 0.15f;
        float randRarity = Random.Range(0f, 1f);
        if (randRarity < legendaryChance)
        {
            rarity = EquipmentRarity.Legendary;
        }
        else if (randRarity < legendaryChance + rareChance)
        {
            rarity = EquipmentRarity.Rare;
        }
        else
        {
            rarity = EquipmentRarity.Normal;
        }

        // 生成装备名称（由装备类型和稀有度决定）
        itemName = $"{itemType} {rarity}";

        // 清空词条数据
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

        // 根据装备类型生成主词条
        switch (itemType)
        {
            case EquipmentType.Helmet:
            case EquipmentType.Chest:
            case EquipmentType.Leg:
            case EquipmentType.Shoes:
                mainStats.Add(new StatEntry(ItemStat.Defense, Random.Range(1, 6)));
                mainStats.Add(new StatEntry(ItemStat.HP, Random.Range(10, 101)));
                break;
            case EquipmentType.Weapon:
                mainStats.Add(new StatEntry(ItemStat.Attack, Random.Range(10, 31)));
                mainStats.Add(new StatEntry(ItemStat.AttackSpeed, Random.Range(2, 6)));
                break;
            case EquipmentType.Accessory:
                {
                    ItemStat accessoryStat = TakeRandomStat(substatsPool);
                    float accessoryValue = GenerateSubStatValue(accessoryStat);
                    mainStats.Add(new StatEntry(accessoryStat, accessoryValue * 2));
                }
                break;
        }

        // 如果装备稀有度为 Normal 且装备不是武器，则主词条（除攻速外）除以2并向下取整
        if (rarity == EquipmentRarity.Normal && itemType != EquipmentType.Weapon)
        {
            for (int i = 0; i < mainStats.Count; i++)
            {
                if (mainStats[i].stat != ItemStat.AttackSpeed)
                {
                    mainStats[i].value = Mathf.Floor(mainStats[i].value / 2f);
                }
            }
        }

        // 移除候选池中已出现在主词条中的属性，保证主词条和副词条不重复
        for (int i = substatsPool.Count - 1; i >= 0; i--)
        {
            if (mainStats.Exists(entry => entry.stat == substatsPool[i]))
            {
                substatsPool.RemoveAt(i);
            }
        }

        // 根据稀有度生成次要词条
        if (rarity == EquipmentRarity.Normal)
        {
            // Normal：最多2个附加词条，每个50%概率生成
            for (int i = 0; i < 2; i++)
            {
                if (substatsPool.Count == 0) break;
                if (Random.value < 0.5f)
                {
                    ItemStat chosen = TakeRandomStat(substatsPool);
                    float value = GenerateSubStatValue(chosen);
                    subStats.Add(new StatEntry(chosen, value));
                }
            }
        }
        else if (rarity == EquipmentRarity.Rare)
        {
            // Rare：最多3个附加词条，必定有1个，其后两个各50%概率生成
            if (substatsPool.Count > 0)
            {
                ItemStat chosen = TakeRandomStat(substatsPool);
                float value = GenerateSubStatValue(chosen);
                subStats.Add(new StatEntry(chosen, value));
            }
            for (int i = 0; i < 2; i++)
            {
                if (substatsPool.Count == 0) break;
                if (Random.value < 0.5f)
                {
                    ItemStat chosen = TakeRandomStat(substatsPool);
                    float value = GenerateSubStatValue(chosen);
                    subStats.Add(new StatEntry(chosen, value));
                }
            }
        }
        else if (rarity == EquipmentRarity.Legendary)
        {
            // Legendary：同 Rare 生成方式，但总词条数至少3
            for (int i = 0; i < 3; i++)
            {
                if (substatsPool.Count > 0)
                {
                    ItemStat chosen = TakeRandomStat(substatsPool);
                    float value = GenerateSubStatValue(chosen);
                    subStats.Add(new StatEntry(chosen, value));
                }
            }
            for (int i = 0; i < 2; i++)
            {
                if (substatsPool.Count == 0) break;
                if (Random.value < 0.5f)
                {
                    ItemStat chosen = TakeRandomStat(substatsPool);
                    float value = GenerateSubStatValue(chosen);
                    subStats.Add(new StatEntry(chosen, value));
                }
            }
            int totalStats = mainStats.Count + subStats.Count;
            while (totalStats < 3 && substatsPool.Count > 0)
            {
                ItemStat chosen = TakeRandomStat(substatsPool);
                float value = GenerateSubStatValue(chosen);
                subStats.Add(new StatEntry(chosen, value));
                totalStats = mainStats.Count + subStats.Count;
            }
        }

        // 生成词条后更新装备外观
        UpdateAppearance();
    }

    /// <summary>
    /// 根据次要词条类型生成随机数值
    /// </summary>
    private float GenerateSubStatValue(ItemStat stat)
    {
        switch (stat)
        {
            case ItemStat.MoveSpeed:
                int msSteps = Random.Range(0, 4);
                return 0.2f + 0.1f * msSteps;
            case ItemStat.Defense:
                return Random.Range(2, 6);
            case ItemStat.HP:
                return Random.Range(2, 11);
            case ItemStat.MagicResist:
                return Random.Range(2, 6);
            case ItemStat.Attack:
                return Random.Range(2, 6);
            case ItemStat.CritRate:
                return Random.Range(2, 6);
            case ItemStat.CritDamage:
                return Random.Range(5, 21);
            case ItemStat.AttackSpeed:
                int asSteps = Random.Range(0, 5);
                return 0.1f + 0.1f * asSteps;
            case ItemStat.Luck:
                return Random.Range(1, 6);
            case ItemStat.LightRadius:
                return 50f;
            default:
                return 0f;
        }
    }

    /// <summary>
    /// 从列表中随机选取一个词条并移除，避免重复
    /// </summary>
    private ItemStat TakeRandomStat(List<ItemStat> list)
    {
        int idx = Random.Range(0, list.Count);
        ItemStat stat = list[idx];
        list.RemoveAt(idx);
        return stat;
    }

    /// <summary>
    /// 更新装备外观：根据装备类型设置图标，根据稀有度设置背景颜色
    /// </summary>
    public void UpdateAppearance()
    {
        // 设置装备图标
        switch (itemType)
        {
            case EquipmentType.Helmet:
                if (helmetSprite != null)
                    iconImage.sprite = helmetSprite;
                break;
            case EquipmentType.Chest:
                if (chestSprite != null)
                    iconImage.sprite = chestSprite;
                break;
            case EquipmentType.Leg:
                if (legSprite != null)
                    iconImage.sprite = legSprite;
                break;
            case EquipmentType.Shoes:
                if (shoesSprite != null)
                    iconImage.sprite = shoesSprite;
                break;
            case EquipmentType.Weapon:
                if (weaponSprite != null)
                    iconImage.sprite = weaponSprite;
                break;
            case EquipmentType.Accessory:
                if (accessorySprite != null)
                    iconImage.sprite = accessorySprite;
                break;
        }

        // 设置背景颜色：Normal = white, Rare = blue, Legendary = gold
        switch (rarity)
        {
            case EquipmentRarity.Normal:
                backgroundImage.color = Color.white;
                break;
            case EquipmentRarity.Rare:
                backgroundImage.color = Color.blue;
                break;
            case EquipmentRarity.Legendary:
                backgroundImage.color = new Color(1f, 0.84f, 0f);
                break;
            default:
                backgroundImage.color = Color.white;
                break;
        }
    }
}
