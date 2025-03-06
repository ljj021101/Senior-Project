using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EquipmentRarity
{
    Normal,
    Rare,
    Legendary
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
    public EquipmentType itemType;       // 装备类型
    public EquipmentRarity rarity;       // 装备稀有度
    public string itemName;              // 装备名称
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
    /// 生成装备的词条，包含主词条和次要词条
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

        // 生成主词条，根据装备类型
        switch (itemType)
        {
            case EquipmentType.Helmet:
            case EquipmentType.Chest:
            case EquipmentType.Leg:
            case EquipmentType.Shoes:
                // 盔甲：主词条为 防御 (随机1~5) 和 生命 (随机10~100)
                mainStats.Add(new StatEntry(ItemStat.Defense, Random.Range(1, 6)));
                mainStats.Add(new StatEntry(ItemStat.HP, Random.Range(10, 101)));
                break;
            case EquipmentType.Weapon:
                // 武器：主词条为 攻击 (随机10~30) 和 攻速 (随机2~5)
                mainStats.Add(new StatEntry(ItemStat.Attack, Random.Range(10, 31)));
                mainStats.Add(new StatEntry(ItemStat.AttackSpeed, Random.Range(2, 6)));
                break;
            case EquipmentType.Accessory:
                // 饰品：主词条从次要词条池中随机选 1 条，数值翻倍
                ItemStat accessoryStat = TakeRandomStat(substatsPool);
                float accessoryValue = GenerateSubStatValue(accessoryStat);
                mainStats.Add(new StatEntry(accessoryStat, accessoryValue * 2));
                break;
        }

        // 如果装备稀有度为 Normal，且装备不是武器，则主词条（除武器攻速）需/2并取整
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

        // 生成次要词条，根据稀有度不同
        if (rarity == EquipmentRarity.Normal)
        {
            // Normal：最多2个附加词条，每个附加词条50%成功率
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
            // Rare：最多3个附加词条，必定有1个，后两个各50%成功率
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
        }

        // 生成词条后，更新装备外观
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
    /// 从列表中随机选一个词条并移除（避免重复），允许不重复时移除
    /// </summary>
    private ItemStat TakeRandomStat(List<ItemStat> list)
    {
        int idx = Random.Range(0, list.Count);
        ItemStat stat = list[idx];
        list.RemoveAt(idx);
        return stat;
    }

    /// <summary>
    /// 更新装备的显示外观：根据装备类型设置图标，根据稀有度设置背景颜色
    /// </summary>
    public void UpdateAppearance()
    {
        // 更新装备图标，根据 itemType 选择对应的 Sprite
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

        // 根据装备稀有度设置背景颜色
        switch (rarity)
        {
            case EquipmentRarity.Normal:
                backgroundImage.color = Color.white;
                break;
            case EquipmentRarity.Rare:
                backgroundImage.color = Color.blue;
                break;
            case EquipmentRarity.Legendary:
                backgroundImage.color = new Color(1f, 0.84f, 0f); // 金色
                break;
            default:
                backgroundImage.color = Color.white;
                break;
        }
    }
}
