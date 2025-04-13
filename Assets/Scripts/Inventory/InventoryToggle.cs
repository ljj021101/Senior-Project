using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryPanel;

    void Start()
    {
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleInventory();
        }
    }

    // 公共方法，可直接通过按钮调用
    public void ToggleInventory()
    {
        CaveGenerator cave = FindObjectOfType<CaveGenerator>();
        if (cave != null && !cave.canOpenInventory)
        {
            Debug.Log("当前无法打开背包！");
            return;
        }

        bool willBeOpen = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(willBeOpen);

        if (cave != null)
        {
            cave.canMove = !willBeOpen; // 打开背包时禁止移动，关闭时恢复
        }
    }
}