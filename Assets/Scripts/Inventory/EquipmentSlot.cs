// EquipmentSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class EquipmentSlot : MonoBehaviour, IDropHandler
{
    public bool requireMatchingType = true;
    public EquipmentType slotType;

    private Outline outlineEffect;
    private Image slotImage;
    private Color originalColor;

    void Awake()
    {
        slotImage = GetComponent<Image>();
        if (slotImage != null) originalColor = slotImage.color;

        outlineEffect = GetComponent<Outline>();
        if (outlineEffect == null)
            outlineEffect = gameObject.AddComponent<Outline>();

        outlineEffect.effectColor    = Color.green;
        outlineEffect.effectDistance = new Vector2(5f, -5f);
        outlineEffect.enabled        = false;
    }

    public void Highlight(bool flag)
    {
        outlineEffect.enabled = flag;
    }

    public void OnDrop(PointerEventData eventData)
{
    var draggable = eventData.pointerDrag?.GetComponent<DraggableItem>();
    if (draggable == null) return;

    var newItem = draggable.GetComponent<EquipmentItem>();
    if (requireMatchingType && (newItem == null || newItem.itemType != slotType))
        return;

    // 1) 若槽里有旧装备，就退回原父格，并归零本地坐标
    if (transform.childCount > 0)
    {
        var old = transform.GetChild(0);
        old.SetParent(draggable.originalParent, false);
        old.localPosition = Vector3.zero;
    }

    // 2) 把新装备放进本槽，并归零本地坐标
    draggable.transform.SetParent(transform, false);
    draggable.transform.localPosition = Vector3.zero;

    // 3) 标记放置成功
    draggable.dropSuccessful = true;
}

}

