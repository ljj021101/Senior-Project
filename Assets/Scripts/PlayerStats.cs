using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 示例：读取“Equipment”下所有槽里的装备并计算最终玩家属性。
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("基础属性")]
    public float baseHP = 200f;              // 初始生命
    public float baseDefense = 0f;           // 初始防御
    public float baseAttack = 0f;            // 初始攻击力
    public float baseMoveSpeed = 2f;         // 初始移动速度
    public float baseMagicResist = 0f;       // 初始魔抗
    public float baseCritRate = 0f;          // 初始暴击率
    public float baseCritDamage = 150f;      // 初始暴击伤害(百分比)
    public float baseLuck = 0f;              // 初始运气
    public float baseLightRadius = 100f;     // 初始光照半径

    [Header("武器攻速(间隔)")]
    // 假设武器的 AttackSpeed 表示 x秒/次，基础为1秒攻击一次，也可自行调整
    public float baseAttackInterval = 1f;

    [Header("装备父物体")]
    // 在Inspector中将"Equipment"对象拖入此字段
    public Transform equipmentParent;

    [Header("最终计算结果(仅调试查看)")]
    public float finalHP;
    public float finalDefense;
    public float finalAttack;
    public float finalMoveSpeed;
    public float finalMagicResist;
    public float finalCritRate;
    public float finalCritDamage;
    public float finalLuck;
    public float finalLightRadius;
    public float finalAttackInterval;  // 攻击间隔(武器攻速)

    /// <summary>
    /// 重新计算玩家属性
    /// </summary>
    public void RecalculateStats()
    {
        // 每次计算前重置为基础值
        float sumHP = 0f;
        float sumDefense = 0f;
        float sumAttack = 0f;
        float sumMoveSpeed = 0f;
        float sumMagicResist = 0f;
        float sumCritRate = 0f;
        float sumCritDamage = 0f;
        float sumLuck = 0f;
        float sumLightRadius = 0f;

        // 攻速逻辑：武器决定基础攻击间隔 x秒/次，其他装备的AttackSpeed视为加成(1+sum)
        // 这里假设我们先找武器的攻速(weaponInterval)，其余攻速加成(attackSpeedBonus)
        float weaponInterval = baseAttackInterval;  // 初始设为 baseAttackInterval
        float attackSpeedBonus = 0f;                // 额外攻速加成(1 + attackSpeedBonus)

        // 遍历"Equipment"下所有槽
        if (equipmentParent != null)
        {
            foreach (Transform slot in equipmentParent)
            {
                if (slot.childCount > 0)
                {
                    // 假设每个槽最多一个装备
                    Transform equipChild = slot.GetChild(0);
                    EquipmentItem eqItem = equipChild.GetComponent<EquipmentItem>();
                    if (eqItem != null)
                    {
                        // 汇总主词条与次要词条
                        sumHP            += GetStatValue(eqItem, ItemStat.HP);
                        sumDefense       += GetStatValue(eqItem, ItemStat.Defense);
                        sumAttack        += GetStatValue(eqItem, ItemStat.Attack);
                        sumMoveSpeed     += GetStatValue(eqItem, ItemStat.MoveSpeed);
                        sumMagicResist   += GetStatValue(eqItem, ItemStat.MagicResist);
                        sumCritRate      += GetStatValue(eqItem, ItemStat.CritRate);
                        sumCritDamage    += GetStatValue(eqItem, ItemStat.CritDamage);
                        sumLuck          += GetStatValue(eqItem, ItemStat.Luck);
                        sumLightRadius   += GetStatValue(eqItem, ItemStat.LightRadius);

                        // 如果装备上有 AttackSpeed 词条
                        // - 如果是武器：我们认为它决定基础攻击间隔(weaponInterval)
                        // - 如果是非武器：则视为攻速加成(attackSpeedBonus)
                        // (具体逻辑看你如何区分武器与非武器，这里仅示例)
                        float eqAttackSpeed = GetStatValue(eqItem, ItemStat.AttackSpeed);
                        if (eqItem.itemType == EquipmentType.Weapon && eqAttackSpeed > 0f)
                        {
                            // 武器的 AttackSpeed = x秒/次
                            weaponInterval = eqAttackSpeed;
                        }
                        else if (eqAttackSpeed > 0f)
                        {
                            // 其他装备的AttackSpeed认为是加成
                            attackSpeedBonus += eqAttackSpeed;
                        }
                    }
                }
            }
        }

        // 根据汇总值计算最终属性
        finalHP          = baseHP + sumHP;
        // 防御 = (baseDefense + sumDefense)
        // 如果你想要 "(base + sum) * (1+ sum% )" 的话，需要区分平值与百分比
        finalDefense     = baseDefense + sumDefense;

        // 攻击力 = baseAttack + sumAttack
        finalAttack      = baseAttack + sumAttack;

        // 移动速度 = baseMoveSpeed + sumMoveSpeed
        finalMoveSpeed   = baseMoveSpeed + sumMoveSpeed;

        // 魔抗, 暴击率, 暴击伤害, 运气, 光照半径 都是直接相加
        finalMagicResist = baseMagicResist + sumMagicResist;
        finalCritRate    = baseCritRate + sumCritRate;
        finalCritDamage  = baseCritDamage + sumCritDamage;
        finalLuck        = baseLuck + sumLuck;
        finalLightRadius = baseLightRadius + sumLightRadius;

        // 攻击间隔 = weaponInterval / (1 + attackSpeedBonus)
        // (假设 weaponInterval 表示 x秒/次, 其余攻速都是加成)
        finalAttackInterval = weaponInterval / (1f + attackSpeedBonus);

        Debug.Log($"[PlayerStats] Recalculated: HP={finalHP}, DEF={finalDefense}, ATK={finalAttack}, MoveSpeed={finalMoveSpeed}, MagicRes={finalMagicResist}, CritRate={finalCritRate}, CritDamage={finalCritDamage}, Luck={finalLuck}, LightRadius={finalLightRadius}, AttackInterval={finalAttackInterval}");
    }

    /// <summary>
    /// 获取装备(主+次要)中某个 ItemStat 的总和
    /// </summary>
    private float GetStatValue(EquipmentItem eqItem, ItemStat statType)
    {
        float sum = 0f;
        // 主词条
        foreach (var statEntry in eqItem.MainStats)
        {
            if (statEntry.stat == statType)
                sum += statEntry.value;
        }
        // 次要词条
        foreach (var statEntry in eqItem.SubStats)
        {
            if (statEntry.stat == statType)
                sum += statEntry.value;
        }
        return sum;
    }
}
