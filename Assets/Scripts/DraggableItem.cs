using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Vector3 originalPosition;      // 拖拽开始时记录物品的世界坐标
    private Transform originalParent;      // 拖拽物品原来的父物体
    public bool dropSuccessful = false;    // 成功放置时由槽设置为 true
    private CanvasGroup canvasGroup;       // 用于控制 Raycast
    private Canvas overrideCanvas;         // 临时Canvas，用于提升排序

    // 引用装备信息面板，运行时通过代码自动查找或在Inspector中手动指定
    public EquipmentInfoPanel infoPanel;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        if (infoPanel == null)
        {
            infoPanel = FindObjectOfType<EquipmentInfoPanel>();
            if (infoPanel == null)
            {
                Debug.LogError("未找到 EquipmentInfoPanel，请确保场景中存在该面板！");
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = transform.position;
        originalParent = transform.parent;
        dropSuccessful = false;
        canvasGroup.blocksRaycasts = false;

        // 添加临时Canvas组件提升排序
        overrideCanvas = gameObject.AddComponent<Canvas>();
        overrideCanvas.overrideSorting = true;
        overrideCanvas.sortingOrder = 1000;  // 设置较高排序值
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

    // 实现点击事件，点击装备时显示装备信息
    public void OnPointerClick(PointerEventData eventData)
    {
        // 点击时显示装备信息
        EquipmentItem item = GetComponent<EquipmentItem>();
        if (item != null && infoPanel != null)
        {
            infoPanel.DisplayEquipmentInfo(item);
            Debug.Log("装备被点击：" + item.itemName);
        }
    }
}