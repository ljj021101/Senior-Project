using UnityEngine;
using UnityEngine.EventSystems;

public class TrashSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // 获取拖拽过来的物品
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggable != null && draggable.transform.parent != transform)
        {
            // 如果垃圾桶内已有其他装备，则销毁它们
            foreach (Transform child in transform)
            {
                if (child != draggable.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            // 将当前拖拽的装备设为垃圾桶的子物体（如果尚未设置）
            if (draggable.transform.parent != transform)
            {
                draggable.transform.SetParent(transform, false);
                draggable.transform.position = transform.position;
            }
            draggable.dropSuccessful = true;
        }
    }
}
