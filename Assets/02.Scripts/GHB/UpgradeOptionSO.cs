using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeOption", menuName = "Game/Upgrade Option")]
public class UpgradeOptionSO : ScriptableObject
{
    public bool isSkill;                   // true면 스킬, false면 비전서
    public string optionName;              // 예: "파이어볼", "쿨타임 마스터리"
    public SkillStatKey affectedStat;      // 대상 스탯
    [Range(0.01f, 1f)] public float minUpgradeRatio = 0.05f;
    [Range(0.01f, 1f)] public float maxUpgradeRatio = 0.15f;
    public string description;             // UI 표시용
}
