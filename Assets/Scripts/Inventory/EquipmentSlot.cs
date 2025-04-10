using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class EquipmentSlot : MonoBehaviour, IDropHandler
{
    public bool requireMatchingType = true;
    public EquipmentType slotType;

    private Outline outlineEffect;  // 用来控制描边
    private Image slotImage;        // 槽位背景（如果需要）
    private Color originalColor;    // 如果你还想用原来的颜色

    void Awake()
    {
        slotImage = GetComponent<Image>();
        if (slotImage != null)
        {
            originalColor = slotImage.color;
        }

        // 获取或添加 Outline 组件
        outlineEffect = GetComponent<Outline>();
        if (outlineEffect == null)
        {
            outlineEffect = gameObject.AddComponent<Outline>();
        }

        // 设置 Outline 的默认属性
        outlineEffect.effectColor = Color.green;
        // 数值越大，边框越“厚”，可以自行调整
        outlineEffect.effectDistance = new Vector2(5f, -5f);
        // 默认不启用
        outlineEffect.enabled = false;
    }

    public void Highlight(bool flag)
    {
        // 只需打开或关闭 Outline 即可
        outlineEffect.enabled = flag;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 你的原有逻辑不变
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            EquipmentItem item = draggable.GetComponent<EquipmentItem>();
            if (transform.childCount > 0)
            {
                Debug.Log("槽中已有装备！");
                return;
            }
            if (requireMatchingType && (item == null || item.itemType != slotType))
            {
                Debug.Log("装备类型不匹配！");
                return;
            }
            draggable.transform.SetParent(transform, false);
            draggable.transform.position = transform.position;
            draggable.dropSuccessful = true;
        }
    }
}

