using System;
using System.Collections.Generic;
using UnityEngine;

public class TempSkillManager : MonoBehaviour, ISkillUpgradable
{
    // PlayerSkillManager가 초기화해주는 스킬 이름, 추후 업그레이드 시 비교값으로 사용됨
    private string skillName;
    [Header("이 스킬이 사용하는 공용 스탯 키들")]
    [SerializeField] private List<SkillStatKey> usedStats = new List<SkillStatKey>();

    [Header("기본값 (예시)")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseCooldown = 5f;

    // 각 STATKEY별 현재 값
    private Dictionary<SkillStatKey, float> statValues = new Dictionary<SkillStatKey, float> ();

    // 현재값 변수
    public float currentDamage { get; private set; }
    public float currentCooldown { get; private set; }

    private void Awake()
    {
        // 모든 SkillStatKey 항목 초기화
        foreach (SkillStatKey key in Enum.GetValues(typeof(SkillStatKey)))
        {
            statValues[key] = 0f;
        }

        // 각 스킬 스크립트에서 쓰이는 변수는 따로 초기화
        statValues[SkillStatKey.Damage] = baseDamage;
        statValues[SkillStatKey.Cooldown] = baseCooldown;

        // 현재값 변수도 초기화
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
    }


    private void OnEnable()
    {
        UpgradeManager.OnUpgradeSelected += ApplyUpgrade;
    }

    private void OnDisable()
    {
        UpgradeManager.OnUpgradeSelected -= ApplyUpgrade;
    }

    public void ApplyUpgrade(UpgradeEventData data)
    {
        SkillStatKey key = data.statKey;

        // 1. 스킬 이벤트인지 확인
        if (data.isSkillUpgrade)
        {
            if (!string.Equals(data.skillName, skillName, StringComparison.OrdinalIgnoreCase))
                return;
        }

        // 2. usedStats에 있는 키만 처리
        if (!usedStats.Contains(key))
            return;

        // 3. statValues 갱신
        statValues[key] *= 1f + data.upgradeRatio;

        // 4. current 변수 갱신
        if (key == SkillStatKey.Damage)
            currentDamage = statValues[key];
        else if (key == SkillStatKey.Cooldown)
            currentCooldown = statValues[key];

        // 5. 디버그 로그
        Debug.Log($"[UpgradeEvent] {gameObject.name}: {key} +{data.upgradeRatio:P1} -> {statValues[key]:F2}");
    }

    public void SetSkillName(string skillName)
    {
        this.skillName = skillName;
    }

    // UsedStats 공개
    public List<SkillStatKey> UsedStats => usedStats;
}
