using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Vector3 originalPosition;      // 拖拽开始时记录物品的世界坐标
    private Transform originalParent;      // 拖拽物品原来的父物体
    public bool dropSuccessful = false;    // 成功放置时由槽设置为 true
    private CanvasGroup canvasGroup;       // 用于控制 Raycast
    private Canvas overrideCanvas;         // 临时 Canvas，用于提升排序

    private EquipmentSlot highlightedSlot; // 用于保存匹配的装备槽

    public PlayerStats playerStats;
    public AudioPlayer audioPlayer;
    public EquipmentInfoPanel infoPanel;

    void Start()
    {
        // 获取或添加 CanvasGroup
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 在 Start 中查找场景中的引用
        if (infoPanel == null)
        {
            infoPanel = FindObjectOfType<EquipmentInfoPanel>();
            if (infoPanel == null)
            {
                Debug.LogError("未找到 EquipmentInfoPanel，请确保场景中存在该面板！");
            }
        }
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("未找到 PlayerStats，请确保场景中存在 PlayerStats 组件！");
            }
        }
        if (audioPlayer == null)
        {
            audioPlayer = FindObjectOfType<AudioPlayer>();
            if (audioPlayer == null)
            {
                Debug.LogError("未找到 AudioPlayer，请确保场景中存在 AudioPlayer 组件！");
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        EquipmentItem item = GetComponent<EquipmentItem>();
        if (item != null && infoPanel != null)
        {
            infoPanel.DisplayEquipmentInfo(item);
        }
        audioPlayer.PlayPickupSound();
        originalPosition = transform.position;
        originalParent = transform.parent;
        dropSuccessful = false;
        canvasGroup.blocksRaycasts = false;

        // 添加临时 Canvas 提升排序
        overrideCanvas = gameObject.AddComponent<Canvas>();
        overrideCanvas.overrideSorting = true;
        overrideCanvas.sortingOrder = 1000;

        // 查找与当前装备匹配的装备槽，并将其高亮为绿色
        EquipmentSlot[] slots = FindObjectsOfType<EquipmentSlot>();
        foreach (EquipmentSlot slot in slots)
        {
            if (slot.requireMatchingType && slot.slotType == item.itemType)
            {
                highlightedSlot = slot;
                highlightedSlot.Highlight(true);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        audioPlayer.PlayDropSound();
        canvasGroup.blocksRaycasts = true;

        // 移除临时 Canvas
        if (overrideCanvas != null)
        {
            Destroy(overrideCanvas);
        }

        // 拖拽结束时取消装备槽高亮
        EquipmentSlot[] slots = FindObjectsOfType<EquipmentSlot>();
        foreach (EquipmentSlot slot in slots)
        {
            slot.Highlight(false);
        }

        if (!dropSuccessful)
        {
            transform.position = originalPosition;
            transform.SetParent(originalParent, false);
        }
        // 重新计算玩家属性
        if (playerStats != null)
        {
            playerStats.RecalculateStats();
        }
    }

    // IPointerClickHandler 实现：点击装备时显示装备信息
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 右键丢进垃圾桶
            TrashSlot trashSlot = FindObjectOfType<TrashSlot>();
            if (trashSlot != null)
            {
                if (trashSlot.transform.childCount > 0)
                {
                    Transform oldItem = trashSlot.transform.GetChild(0);
                    Destroy(oldItem.gameObject);
                }

                transform.SetParent(trashSlot.transform, false);
                transform.localPosition = Vector3.zero;

                playerStats.RecalculateStats();

                Debug.Log("右键点击装备，已移入垃圾桶（并替换原有物品）");
            }

            return;
        }

        // 左键时才显示装备信息
        EquipmentInfoPanel infoPanel = FindObjectOfType<EquipmentInfoPanel>();
        if (infoPanel != null)
        {
            infoPanel.DisplayEquipmentInfo(GetComponent<EquipmentItem>());
        }
    }
}

