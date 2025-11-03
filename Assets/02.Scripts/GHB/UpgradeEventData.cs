using System;

[Serializable]
public class UpgradeEventData
{
    public bool isSkillUpgrade;
    public string skillName;
    public SkillStatKey statKey;
    public float upgradeRatio;
}
