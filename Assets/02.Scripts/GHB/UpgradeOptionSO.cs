using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeOption", menuName = "Game/Upgrade Option")]
public class UpgradeOptionSO : ScriptableObject
{
    [Header("스킬 / 비전서 확인용")]
    public bool isSkill;                   // true면 스킬, false면 비전서
    [Header("스킬명(isSkill이 true일 때만 유효)")]
    public string optionName;              // 예: "파이어볼", "쿨타임 마스터리"
    [Header("비전서 반영 스탯 키값(isSkill이 false일 때만 유효)")]
    public SkillStatKey affectedStat;      // 대상 스탯
    public Sprite icon; // 선택지에 표시할 아이콘
    [Range(0.01f, 1f)] public float minUpgradeRatio = 0.05f;
    [Range(0.01f, 1f)] public float maxUpgradeRatio = 0.15f;
    public string description;             // UI 표시용
}
