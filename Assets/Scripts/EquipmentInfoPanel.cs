using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;
using System.Collections.Generic;

public class EquipmentInfoPanel : MonoBehaviour
{
    [Header("UI 显示组件")]
    public TMP_Text equipmentNameText;  // 显示装备名称
    public TMP_Text equipmentTypeText;  // 显示装备类型
    public TMP_Text mainStatsText;      // 显示主词条列表
    public TMP_Text subStatsText;       // 显示副词条列表
    public TMP_Text descriptionText;    // 显示装备描述

    /// <summary>
    /// 根据装备信息，更新面板显示
    /// </summary>
    /// <param name="item">要显示的装备</param>
    public void DisplayEquipmentInfo(EquipmentItem item)
    {
        if (item == null)
        {
            ClearInfo();
            return;
        }

        // 装备名称
        if (equipmentNameText != null)
            equipmentNameText.text = item.itemName;

        // 装备类型
        if (equipmentTypeText != null)
            equipmentTypeText.text = item.itemType.ToString();

        // 主词条
        if (mainStatsText != null)
            mainStatsText.text = GetStatsString(item.MainStats);

        // 副词条
        if (subStatsText != null)
            subStatsText.text = GetStatsString(item.SubStats);

        // 描述
        if (descriptionText != null)
            descriptionText.text = GetDescription(item);
    }

    /// <summary>
    /// 将词条列表转换为可读字符串
    /// </summary>
    private string GetStatsString(IReadOnlyList<StatEntry> stats)
    {
        if (stats == null || stats.Count == 0)
            return "None";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var entry in stats)
        {
            sb.AppendLine(entry.ToString());
        }
        return sb.ToString();
    }

    /// <summary>
    /// 根据装备类型生成一个简单描述，或根据需求自行实现
    /// </summary>
    private string GetDescription(EquipmentItem item)
    {
        switch (item.itemType)
        {
            case EquipmentType.Helmet:
                return "a sliver helmet";
            case EquipmentType.Chest:
                return "a sliver chest armor";
            case EquipmentType.Leg:
                return "a silver leg armor";
            case EquipmentType.Shoes:
                return "a stable shoe";
            case EquipmentType.Weapon:
                return "a sharp knife";
            case EquipmentType.Accessory:
                return "mysterious rings";
            default:
                return "No description";
        }
    }

    /// <summary>
    /// 清空显示
    /// </summary>
    public void ClearInfo()
    {
        if (equipmentNameText) equipmentNameText.text = "";
        if (equipmentTypeText) equipmentTypeText.text = "";
        if (mainStatsText) mainStatsText.text = "";
        if (subStatsText) subStatsText.text = "";
        if (descriptionText) descriptionText.text = "";
    }
}
