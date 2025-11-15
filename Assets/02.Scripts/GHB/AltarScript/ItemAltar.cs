using System;
using UnityEngine;

public class ItemAltar : AltarBase
{
    [Header("제단 UI")]
    [SerializeField] private GameObject ItemAltarPanel;

    [Header("아이템 SO")]
    [SerializeField] private ItemSO item;

    public override void Execute(Transform player)
    {
        StageFlowManager.instance.SetStateToAltar();
        // UI 활성화
        ItemAltarPanel.SetActive(true);

        var altarUI = ItemAltarPanel.GetComponent<ItemAltarUIManager>();
        if (altarUI != null && item != null)
        {
            altarUI.Setup(item, player);
        }
    }

}
