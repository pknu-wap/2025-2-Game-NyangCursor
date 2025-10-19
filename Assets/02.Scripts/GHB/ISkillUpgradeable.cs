using System.Collections.Generic;

public interface ISkillUpgradable
{
    List<SkillStatKey> UsedStats { get; }
    void ApplyUpgrade(UpgradeEventData data);
    void SetSkillName(string skillName);
}
