using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlayerStats : MonoBehaviour
{
    [Header("基础属性")]
    public float baseHP = 200f;              // 初始生命
    public float baseDefense = 0f;           // 初始防御
    public float baseAttack = 10f;            // 初始攻击
    public float baseMoveSpeed = 2f;         // 初始移动速度
    public float baseMagicResist = 0f;       // 初始魔抗
    public float baseCritRate = 0f;          // 初始暴击率
    public float baseCritDamage = 150f;      // 初始暴击伤害（百分比）
    public float baseLuck = 0f;              // 初始运气
    public float baseLightRadius = 100f;     // 初始光照范围（百分比）

    [Header("等级与经验")]
    public int playerLevel = 1;
    public int currentExp = 1;
    public int expToNextLevel = 100;

    [Header("光照控制")]
    public UnityEngine.Rendering.Universal.Light2D playerLight;

    [Header("当前状态")]
    public float currentHP;  // 当前血量

    [Header("武器攻速(间隔)")]
    public float baseAttackInterval = 1f;    // 如果没有装备武器时，默认攻击间隔

    [Header("装备父物体")]
    public Transform equipmentParent;        // 背包/装备栏容器（包含所有装备槽，每个槽下可能有装备）

    [Header("最终计算结果 (仅调试查看)")]
    public float finalHP;
    public float finalDefense;       // 按公式：盔甲主防御 * (1 + 总次防御%/100)
    public float finalAttack;        // 按公式：武器主攻击 * (1 + 总次攻击%/100)
    public float finalMoveSpeed;
    public float finalMagicResist;
    public float finalCritRate;
    public float finalCritDamage;
    public float finalLuck;
    public float finalLightRadius;
    public float finalAttackInterval; // 攻击间隔 = 武器主攻速 / (1 + 非武器攻速加成)

    [Header("消耗品数量")]
    public Dictionary<string, int> consumableInventory = new Dictionary<string, int>();

    public GameObject BattlePanel;

    /// <summary>
    /// 重新计算玩家属性
    /// </summary>
    public void RecalculateStats()
    {
        // 需要分别统计：
        // 1. 盔甲主防御：只统计装备类型为 Helmet, Chest, Leg, Shoes 中的主词条 Defense
        // 2. 武器主攻击：只统计装备类型为 Weapon 中的主词条 Attack
        // 3. 次要 Defense 加成：所有装备中次要词条 Defense 的总和（百分比）
        // 4. 次要 Attack 加成：所有装备中次要词条 Attack 的总和（百分比）
        // 5. 非武器攻速加成：所有非武器装备中次要词条 AttackSpeed 的总和
        // 6. 武器主攻速：装备类型为 Weapon 中主词条 AttackSpeed（表示 x秒/次）
        // 其他属性直接累加（所有装备中主+次词条之和）

        float armorMainDefense = 0f;
        float totalDefenseBonusPercent = 0f;
        float baseHpFromArmor = 0f;
        float totalHPBonusPercent = 0f;

        float weaponMainAttack = 0f;
        float totalAttackBonusPercent = 0f;

        float totalMoveSpeed = 0f;
        float totalMagicResist = 0f;
        float totalCritRate = 0f;
        float totalCritDamage = 0f;
        float totalLuck = 0f;
        float totalLightRadius = 0f;

        float weaponAttackSpeed = baseAttackInterval;  // 默认
        float nonWeaponAttackSpeedBonus = 0f;

        // 遍历装备父物体下所有槽
        if (equipmentParent != null)
        {
            foreach (Transform slot in equipmentParent)
            {
                if (slot.childCount > 0)
                {
                    EquipmentItem eqItem = slot.GetChild(0).GetComponent<EquipmentItem>();
                    if (eqItem != null)
                    {
                        // 盔甲主防御只从 Armor 类型装备的 主词条 Defense 中获取
                        if (eqItem.itemType == EquipmentType.Helmet ||
                            eqItem.itemType == EquipmentType.Chest ||
                            eqItem.itemType == EquipmentType.Leg ||
                            eqItem.itemType == EquipmentType.Shoes)
                        {
                            armorMainDefense += GetMainStatValue(eqItem, ItemStat.Defense);
                            baseHpFromArmor += GetMainStatValue(eqItem, ItemStat.HP);
                        }
                        // 武器主攻击只从 Weapon 类型装备的 主词条 Attack 中获取
                        if (eqItem.itemType == EquipmentType.Weapon)
                        {
                            weaponMainAttack = GetMainStatValue(eqItem, ItemStat.Attack);
                            // 武器的主攻速（AttackSpeed）作为基础攻击间隔
                            float temp = GetMainStatValue(eqItem, ItemStat.AttackSpeed);
                            if (temp > 0f)
                                weaponAttackSpeed = temp;
                        }
                        // 累加所有装备的次要 Defense 加成
                        totalDefenseBonusPercent += GetSubStatValue(eqItem, ItemStat.Defense);
                        totalHPBonusPercent += GetSubStatValue(eqItem, ItemStat.HP);
                        // 累加所有装备的次要 Attack 加成
                        totalAttackBonusPercent += GetSubStatValue(eqItem, ItemStat.Attack);
                        // 累加其他属性，均为主+次
                        totalMoveSpeed += GetStatValue(eqItem, ItemStat.MoveSpeed);
                        totalMagicResist += GetStatValue(eqItem, ItemStat.MagicResist);
                        totalCritRate += GetStatValue(eqItem, ItemStat.CritRate);
                        totalCritDamage += GetStatValue(eqItem, ItemStat.CritDamage);
                        totalLuck += GetStatValue(eqItem, ItemStat.Luck);
                        totalLightRadius += GetStatValue(eqItem, ItemStat.LightRadius);

                        // 对于攻速加成，非武器装备的次要 AttackSpeed视为加成
                        if (eqItem.itemType != EquipmentType.Weapon)
                        {
                            nonWeaponAttackSpeedBonus += GetSubStatValue(eqItem, ItemStat.AttackSpeed);
                        }
                    }
                }
            }
        }

        // 计算最终属性
        finalHP = (baseHP + baseHpFromArmor + playerLevel * 10f) * (1f + totalHPBonusPercent / 100f);
        // 防御：以所有盔甲主防御为基数，加上次要 Defense 百分比加成
        finalDefense = (armorMainDefense + 0.5f * playerLevel)* (1f + totalDefenseBonusPercent / 100f);
        // 攻击：以武器主攻击为基数，加上次要 Attack 百分比加成
        finalAttack = (baseAttack + weaponMainAttack + playerLevel) * (1f + totalAttackBonusPercent / 100f);
        finalMoveSpeed = baseMoveSpeed + totalMoveSpeed;
        finalMagicResist = baseMagicResist + totalMagicResist;
        finalCritRate = baseCritRate + totalCritRate;
        finalCritDamage = baseCritDamage + totalCritDamage;
        finalLuck = baseLuck + totalLuck;
        finalLightRadius = baseLightRadius + totalLightRadius;

        float normalLightRadius = finalLightRadius / 100f;
        if (!IsInBattle())
        {
            playerLight.pointLightOuterRadius = normalLightRadius;
        }

        // 攻击间隔 = 武器主攻速 / (1 + 非武器攻速加成)
        finalAttackInterval = weaponAttackSpeed / (1f + nonWeaponAttackSpeedBonus);

        if (currentHP > finalHP)
        {
            currentHP = finalHP;
        }

        Debug.Log($"[PlayerStats] Recalculated: HP={finalHP}, DEF={finalDefense}, ATK={finalAttack}, MoveSpeed={finalMoveSpeed}, MagicRes={finalMagicResist}, CritRate={finalCritRate}, CritDamage={finalCritDamage}, Luck={finalLuck}, LightRadius={finalLightRadius}, AttackInterval={finalAttackInterval}");
    }

    /// <summary>
    /// 获取装备中某个词条（主+次）的总和
    /// </summary>
    private float GetStatValue(EquipmentItem eqItem, ItemStat statType)
    {
        float sum = 0f;
        foreach (var statEntry in eqItem.MainStats)
        {
            if (statEntry.stat == statType)
                sum += statEntry.value;
        }
        foreach (var statEntry in eqItem.SubStats)
        {
            if (statEntry.stat == statType)
                sum += statEntry.value;
        }
        return sum;
    }

    /// <summary>
    /// 获取装备中仅主词条的某个属性
    /// </summary>
    private float GetMainStatValue(EquipmentItem eqItem, ItemStat statType)
    {
        float sum = 0f;

        if (eqItem.itemType == EquipmentType.Accessory)
            return 0f;

        foreach (var statEntry in eqItem.MainStats)
        {
            if (statEntry.stat == statType)
                sum += statEntry.value;
        }
        return sum;
    }

    private float GetSubStatValue(EquipmentItem eqItem, ItemStat statType)
    {
        float sum = 0f;

        // 添加饰品的主词条作为“副词条”
        if (eqItem.itemType == EquipmentType.Accessory)
        {
            foreach (var statEntry in eqItem.MainStats)
            {
                if (statEntry.stat == statType)
                    sum += statEntry.value;
            }
        }

        // 正常的副词条统计
        foreach (var statEntry in eqItem.SubStats)
        {
            if (statEntry.stat == statType)
                sum += statEntry.value;
        }

        return sum;
    }

    void Start()
    {
        RecalculateStats();           // 先计算 finalHP 等属性
    }

    public void GainExp(int amount)
    {
        currentExp += amount;
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            playerLevel++;
            expToNextLevel = Mathf.FloorToInt(expToNextLevel * 1.5f); // 每次升级需求更高
            currentHP = finalHP;
            RecalculateStats();
        }

        InventoryManager manager = FindObjectOfType<InventoryManager>();
        if (manager != null)
            manager.SaveAll(); // 升级后立刻保存
    }

    public void AddConsumable(string name, int amount)
    {
        if (!consumableInventory.ContainsKey(name))
            consumableInventory[name] = 0;
        consumableInventory[name] += amount;
        Debug.Log($"获得 {name} ×{amount}，当前数量：{consumableInventory[name]}");

        FindObjectOfType<PlayerStatsUI>()?.UpdateConsumableUI(name, consumableInventory[name]);
    }

    public bool UseConsumable(string name)
    {
        if (consumableInventory.ContainsKey(name) && consumableInventory[name] > 0)
        {
            consumableInventory[name]--;
            currentHP = Mathf.Min(finalHP, currentHP + finalHP * 0.1f);
            Debug.Log($"使用{name}，恢复生命，目前血量：{currentHP}");
            FindObjectOfType<PlayerStatsUI>()?.UpdateConsumableUI(name, consumableInventory[name]);
            if(IsInBattle())
            {
                var battle = FindObjectOfType<BattleManager>();
                battle.playerHPBar.value = currentHP;
            }
            return true;
        }
        return false;
    }

    private bool IsInBattle()
    {
        return BattlePanel != null && BattlePanel.gameObject.activeInHierarchy;
    }
}
