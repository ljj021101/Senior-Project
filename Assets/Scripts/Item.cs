using UnityEngine;

public class EquipmentItem : MonoBehaviour
{
    // 装备类型，例如 Helmet, Chest, Leg, Shoes, Accessory 等
    public EquipmentType itemType;

    // 可选属性：物品名称、属性加成等
    public string itemName;
    public Sprite icon; // 图标，可用于背包UI显示

    // 你可以添加更多属性和方法以满足游戏需求
}