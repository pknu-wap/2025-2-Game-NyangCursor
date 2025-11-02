using System;
using UnityEngine;

[Serializable]
public class SkillSlot
{
    [Header("연결된 업그레이드 데이터 (자동 이름 동기화)")]
    public UpgradeOptionSO linkedUpgradeSO;

    // 스킬 이름. SO의 optionName에서 자동 설정
    [HideInInspector] public string skillName;

    public GameObject skillManagerObject;

    [HideInInspector] public bool isUnlocked = false;
    [HideInInspector] public Sprite icon;
}
