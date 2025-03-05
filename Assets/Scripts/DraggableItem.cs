using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 originalPosition;      // 拖拽开始时记录物品的世界坐标
    private Transform originalParent;      // 拖拽物品原来的父物体
    public bool dropSuccessful = false;    // 成功放置时由槽设置为 true
    private CanvasGroup canvasGroup;       // 用于控制 Raycast
    private Canvas overrideCanvas;         // 临时Canvas，用于提升排序

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = transform.position;
        originalParent = transform.parent;
        dropSuccessful = false;
        canvasGroup.blocksRaycasts = false;

        // 添加一个临时的Canvas组件，并设置其排序
        overrideCanvas = gameObject.AddComponent<Canvas>();
        overrideCanvas.overrideSorting = true;
        overrideCanvas.sortingOrder = 1000;  // 设置一个很高的排序值，确保显示在最前面
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 移除临时Canvas组件
        if (overrideCanvas != null)
        {
            Destroy(overrideCanvas);
        }

        if (!dropSuccessful)
        {
            transform.position = originalPosition;
            transform.SetParent(originalParent, false);
        }
    }
}
