using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour, IDropHandler
{
    public bool requireMatchingType = true; // 是否要求装备类型匹配
    public EquipmentType slotType;          // 该槽支持的装备类型

    private Image slotImage;
    private Color originalColor;

    void Awake()
    {
        slotImage = GetComponent<Image>();
        if (slotImage != null)
        {
            originalColor = slotImage.color;
        }
    }

    // 高亮方法：flag 为 true 时设为绿色，否则恢复原始颜色
    public void Highlight(bool flag)
    {
        if (slotImage != null)
        {
            slotImage.color = flag ? Color.green : originalColor;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            EquipmentItem item = draggable.GetComponent<EquipmentItem>();

            // 如果槽中已有装备，则拒绝放置
            if (transform.childCount > 0)
            {
                Debug.Log("槽中已有装备！");
                return;
            }

            // 如果要求类型匹配且装备类型不匹配，拒绝放置
            if (requireMatchingType && (item == null || item.itemType != slotType))
            {
                Debug.Log("装备类型不匹配！");
                return;
            }

            // 如果条件都满足，则放置装备到槽中
            draggable.transform.SetParent(transform, false);
            draggable.transform.position = transform.position;
            draggable.dropSuccessful = true;
        }
    }
}

