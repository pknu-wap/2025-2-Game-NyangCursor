using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class SkillRemoveAltar : AltarBase
{
    [Header("제단 UI")]
    [SerializeField] private GameObject removeAltarPanel;

    public override void Execute(Transform player)
    {
        StageFlowManager.instance.SetStateToAltar();
        // UI 활성화
        removeAltarPanel.SetActive(true);
    }
}
