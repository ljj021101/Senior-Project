using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EquipmentRarity { Normal, Rare, Legendary }
public enum EquipmentType { Helmet, Chest, Leg, Shoes, Accessory, Weapon }
public enum WeaponClass { Light, Medium, Heavy }
public enum ItemStat
{
    Defense, HP, Attack, AttackSpeed,
    MoveSpeed, MagicResist, CritRate, CritDamage, Luck, LightRadius
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

    public override string ToString() => $"{stat}: {value}";
}

public class EquipmentItem : MonoBehaviour
{
    [Header("装备基础数据")]
    public int equipmentSeed = 0;
    public int equipmentLevel = 0;
    public EquipmentType itemType;
    public EquipmentRarity rarity;
    public string itemName;
    public Sprite icon;
    public WeaponClass weaponClass;

    [Header("显示组件")]
    public Image iconImage;
    public Image backgroundImage;

    [Header("类型对应贴图")]
    public Sprite helmetSprite;
    public Sprite chestSprite;
    public Sprite legSprite;
    public Sprite shoesSprite;
    public Sprite accessorySprite;
    public Sprite weaponSprite;

    [SerializeField] private List<StatEntry> mainStats = new List<StatEntry>();
    [SerializeField] private List<StatEntry> subStats = new List<StatEntry>();

    public IReadOnlyList<StatEntry> MainStats => mainStats;
    public IReadOnlyList<StatEntry> SubStats => subStats;

    public void GenerateStats(int specifiedLevel)
    {
        equipmentLevel = specifiedLevel;
        GenerateStats();
    }

    public void GenerateStats()
    {
        var originalState = Random.state;
        if (equipmentSeed == -1)
            equipmentSeed = Random.Range(1, int.MaxValue);
        Random.InitState(equipmentSeed);

        // 获取当前层数
        int level = (equipmentLevel > 0) 
                    ? equipmentLevel 
                    : (GameObject.FindObjectOfType<CaveGenerator>()?.currentLevel ?? 1);
        equipmentLevel = level;

        // 类型分配
        float randType = Random.Range(0f, 1f);
        if (randType < 0.7f)
        {
            EquipmentType[] armors = { EquipmentType.Helmet, EquipmentType.Chest, EquipmentType.Leg, EquipmentType.Shoes };
            itemType = armors[Random.Range(0, armors.Length)];
        }
        else if (randType < 0.9f)
        {
            itemType = EquipmentType.Weapon;
        }
        else
        {
            itemType = EquipmentType.Accessory;
        }

        // 稀有度分配
        float rarityRoll = Random.Range(0f, 1f);
        if (rarityRoll < 0.1f) rarity = EquipmentRarity.Legendary;
        else if (rarityRoll < 0.4f) rarity = EquipmentRarity.Rare;
        else rarity = EquipmentRarity.Normal;

        itemName = $"{itemType} {rarity} (LV.{level})";
        mainStats.Clear();
        subStats.Clear();

        List<ItemStat> subPool = new List<ItemStat>()
        {
            ItemStat.MoveSpeed, ItemStat.Defense, ItemStat.HP, ItemStat.MagicResist,
            ItemStat.Attack, ItemStat.CritRate, ItemStat.CritDamage,
            ItemStat.AttackSpeed, ItemStat.Luck, ItemStat.LightRadius
        };

        // 主属性计算
        switch (itemType)
        {
            case EquipmentType.Helmet:
            case EquipmentType.Chest:
            case EquipmentType.Leg:
            case EquipmentType.Shoes:
                {
                    int baseDef = Random.Range(0, 3) + level / 3;
                    float defBonus = (rarity == EquipmentRarity.Rare || rarity == EquipmentRarity.Legendary) ? level / 2f : 0;
                    float defense = baseDef + defBonus + Mathf.FloorToInt(level / 3f);
                    float hp = Random.Range(10, 26) + (rarity == EquipmentRarity.Rare || rarity == EquipmentRarity.Legendary ? 15 : 0) + level * 10;
                    mainStats.Add(new StatEntry(ItemStat.Defense, defense));
                    mainStats.Add(new StatEntry(ItemStat.HP, hp));
                    break;
                }

            case EquipmentType.Weapon:
                {
                    WeaponClass[] classes = { WeaponClass.Light, WeaponClass.Medium, WeaponClass.Heavy };
                    weaponClass = classes[Random.Range(0, classes.Length)];

                    float atk = Random.Range(2, 8) + level;
                    switch (weaponClass)
                    {
                        case WeaponClass.Medium: atk = (atk + 2) * 2; break;
                        case WeaponClass.Heavy: atk = (atk + 5) * 4; break;
                    }

                    if (rarity == EquipmentRarity.Rare) atk += 2 * level;
                    else if (rarity == EquipmentRarity.Legendary) atk += 3 * level;

                    int atkSpeed = weaponClass == WeaponClass.Light ? 1 : weaponClass == WeaponClass.Medium ? 2 : 3;

                    mainStats.Add(new StatEntry(ItemStat.Attack, atk));
                    mainStats.Add(new StatEntry(ItemStat.AttackSpeed, atkSpeed));
                    break;
                }

            case EquipmentType.Accessory:
                {
                    ItemStat chosen = TakeRandomStat(subPool);
                    float value = GenerateSubStatValue(chosen);
                    float multiplier = rarity == EquipmentRarity.Legendary ? 2f : (rarity == EquipmentRarity.Rare ? 1.5f : 1f);
                    mainStats.Add(new StatEntry(chosen, value * multiplier));
                    break;
                }
        }

        // 移除主属性中的副词条
        for (int i = subPool.Count - 1; i >= 0; i--)
        {
            if (mainStats.Exists(m => m.stat == subPool[i]))
                subPool.RemoveAt(i);
        }

        // 副词条逻辑
        int maxSub = (rarity == EquipmentRarity.Normal) ? 2 : (rarity == EquipmentRarity.Rare) ? 3 : 5;
        int guaranteed = (rarity == EquipmentRarity.Legendary) ? 3 : (rarity == EquipmentRarity.Rare ? 1 : 0);

        for (int i = 0; i < guaranteed && subPool.Count > 0; i++)
        {
            ItemStat chosen = TakeRandomStat(subPool);
            subStats.Add(new StatEntry(chosen, GenerateSubStatValue(chosen)));
        }

        for (int i = guaranteed; i < maxSub && subPool.Count > 0; i++)
        {
            if (Random.value < 0.5f)
            {
                ItemStat chosen = TakeRandomStat(subPool);
                subStats.Add(new StatEntry(chosen, GenerateSubStatValue(chosen)));
            }
        }

        UpdateAppearance();
        Random.state = originalState;
    }

    private float GenerateSubStatValue(ItemStat stat)
    {
        switch (stat)
        {
            case ItemStat.MoveSpeed: return 0.2f + 0.1f * Random.Range(0, 4);
            case ItemStat.Defense: return Random.Range(2, 6);
            case ItemStat.HP: return Random.Range(2, 11);
            case ItemStat.MagicResist: return Random.Range(2, 6);
            case ItemStat.Attack: return Random.Range(2, 6);
            case ItemStat.CritRate: return Random.Range(2, 6);
            case ItemStat.CritDamage: return Random.Range(5, 21);
            case ItemStat.AttackSpeed: return 0.1f + 0.1f * Random.Range(0, 5);
            case ItemStat.Luck: return Random.Range(1, 6);
            case ItemStat.LightRadius: return 20f;
            default: return 0f;
        }
    }

    private ItemStat TakeRandomStat(List<ItemStat> list)
    {
        int index = Random.Range(0, list.Count);
        ItemStat stat = list[index];
        list.RemoveAt(index);
        return stat;
    }

    public void UpdateAppearance()
    {
        switch (itemType)
        {
            case EquipmentType.Helmet: if (helmetSprite) iconImage.sprite = helmetSprite; break;
            case EquipmentType.Chest: if (chestSprite) iconImage.sprite = chestSprite; break;
            case EquipmentType.Leg: if (legSprite) iconImage.sprite = legSprite; break;
            case EquipmentType.Shoes: if (shoesSprite) iconImage.sprite = shoesSprite; break;
            case EquipmentType.Weapon: if (weaponSprite) iconImage.sprite = weaponSprite; break;
            case EquipmentType.Accessory: if (accessorySprite) iconImage.sprite = accessorySprite; break;
        }

        switch (rarity)
        {
            case EquipmentRarity.Normal: backgroundImage.color = Color.white; break;
            case EquipmentRarity.Rare: backgroundImage.color = Color.blue; break;
            case EquipmentRarity.Legendary: backgroundImage.color = new Color(1f, 0.84f, 0f); break;
        }
    }
}
