using System.Collections.Generic;

public enum SkillType
{
    Active, // 액티브형
    Passive // 패시브형
}

public interface ISkill
{
    // 스킬 타입 읽기 전용 속성
    SkillType SkillType { get; }
    List<SkillStatKey> UsedStats { get; }
    void ApplyUpgrade(UpgradeEventData data);
    void SetSkillName(string skillName);
    void ResetSkill();
}
