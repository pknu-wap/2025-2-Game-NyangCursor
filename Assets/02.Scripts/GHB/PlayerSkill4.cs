using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerSkill4 : MonoBehaviour, ISkill
{
    [SerializeField] private GameObject stormPrefab;

    private int currentLevel = 0;
    private string skillName;

    [Header("이 스킬이 사용하는 공용 스탯 키들")]
    [SerializeField] private List<SkillStatKey> usedStats = new List<SkillStatKey>();

    [Header("기본값")]
    [SerializeField] private float baseDamage = 2f;
    [SerializeField] private float baseCooldown = 5f;
    [SerializeField] private float baseRange = 3f;
    [SerializeField] private float baseProjectileSizeLevel = 1f;

    // 발사체 수가 아니라 소환체 수 같은걸로 따로 변수를 파야할듯
    [SerializeField] private float baseEffectZoneDuration = 1f;


    // 각 STATKEY별 현재 값
    private Dictionary<SkillStatKey, float> statValues = new Dictionary<SkillStatKey, float>();

    // 현재값 변수
    public float currentDamage { get; private set; }
    public float currentCooldown { get; private set; }
    public float currentRange { get; private set; }
    public float currentProjectileSizeLevel { get; private set; }
    public float currentEffectZoneDuration { get; private set; }
    private Coroutine passiveRoutine;


    #region 스킬 스크립트 기본 구조

    private void Awake()
    {
        foreach (SkillStatKey key in Enum.GetValues(typeof(SkillStatKey)))
        {
            statValues[key] = 0f;
        }

        // 각 스킬 스크립트에서 쓰이는 변수는 따로 초기화
        statValues[SkillStatKey.Damage] = baseDamage;
        statValues[SkillStatKey.Cooldown] = baseCooldown;
        statValues[SkillStatKey.Range] = baseRange;
        statValues[SkillStatKey.EffectZoneDuration] = baseEffectZoneDuration;
        statValues[SkillStatKey.ProjectileSize] = baseProjectileSizeLevel;

        // 현재값 변수도 초기화
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
        currentRange = baseRange;
        currentEffectZoneDuration = baseEffectZoneDuration;
        currentProjectileSizeLevel = baseProjectileSizeLevel;
    }


    private void OnEnable()
    {
        PlayerStateLogic.Instance.OnStateChanged += HandleStateChanged;
        UpgradeManager.OnUpgradeSelected1 += ApplyUpgrade;
        HandleStateChanged(PlayerStateLogic.Instance.CurrentState);
    }

    private void OnDisable()
    {
        UpgradeManager.OnUpgradeSelected1 -= ApplyUpgrade;
        PlayerStateLogic.Instance.OnStateChanged -= HandleStateChanged;
    }

    public void Activate()
    {
        if (!IsSkillAllowed())
        {
            Debug.Log($"{skillName} 현재 상태에서 사용 불가");
            return;
        }

        SkillCooldownUI cdUI = null;
        SkillIconUIManager.Instance.TryGetCooldownUI(skillName, out cdUI);

        bool hasCooldownStat = usedStats.Contains(SkillStatKey.Cooldown);

        if (passiveRoutine == null)
            passiveRoutine = StartCoroutine(PassiveLoop(currentCooldown, cdUI, hasCooldownStat));

    }

    private IEnumerator PassiveLoop(float cd, SkillCooldownUI cdUI, bool showUI)
    {
        while (true)
        {
            SpawnStormAtRandomPosition();
            Debug.Log($"{skillName} (패시브 효과 발동 중...)");
            if (showUI)
                cdUI?.StartCooldown(cd);

            yield return new WaitForSeconds(cd);
        }
    }

    public void ApplyUpgrade(UpgradeEventData data)
    {
        SkillStatKey key = data.statKey;

        if (data.isSkillUpgrade)
        {
            if (!string.Equals(data.skillName, skillName, StringComparison.OrdinalIgnoreCase))
                return;

            if (data.applyLevelUp)
            {
                currentLevel++;
            }

        }

        if (!usedStats.Contains(key))
            return;

        // 스킬에서 사용하는 스탯에 따라 커스터마이징 하면 됩니다.
        if (key == SkillStatKey.Cooldown)
        {
            statValues[key] -= data.upgradeRatio;
        }
        else
        {
            statValues[key] += data.upgradeRatio;
        }
        // 스킬에서 사용하는 스탯에 따라 커스터마이징 하면 됩니다.
        switch (key)
        {
            case SkillStatKey.Damage:
                currentDamage = statValues[key];
                break;
            case SkillStatKey.Cooldown:
                currentCooldown = statValues[key];
                break;
            case SkillStatKey.Range:
                currentRange = statValues[key];
                break;
            case SkillStatKey.EffectZoneDuration:
                baseEffectZoneDuration = statValues[key];
                break;
            case SkillStatKey.ProjectileSize:
                baseProjectileSizeLevel = statValues[key];
                break;
        }
    }


    public void ResetSkill()
    {
        foreach (SkillStatKey key in Enum.GetValues(typeof(SkillStatKey)))
        {
            statValues[key] = key switch
            {
                // 스킬에서 사용하는 스탯에 따라 커스터마이징 하면 됩니다.
                SkillStatKey.Damage => baseDamage,
                SkillStatKey.Cooldown => baseCooldown,
                SkillStatKey.Range => baseRange,
                SkillStatKey.EffectZoneDuration => baseEffectZoneDuration,
                SkillStatKey.ProjectileSize => baseProjectileSizeLevel,
                _ => 0f
            };
        }
        currentLevel = 1;
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
        currentRange = baseRange;
        currentEffectZoneDuration = baseEffectZoneDuration;
        currentProjectileSizeLevel = baseProjectileSizeLevel;


        StopAllCoroutines();
        passiveRoutine = null;
    }

    public void SetSkill(string skillName)
    {
        this.skillName = skillName;
    }


    // UsedStats 공개
    public List<SkillStatKey> UsedStats => usedStats;

    public int CurrentLevel
    {
        get => currentLevel;
        set => currentLevel = value;
    }

    // ======================= 상태 제어 로직 =======================
    public void HandleStateChanged(PlayerStateLogic.PlayerState newState)
    {
        bool allowed = IsSkillAllowed();

        SkillCooldownUI cdUI = null;
        bool found = SkillIconUIManager.Instance?.TryGetCooldownUI(skillName, out cdUI) ?? false;

        Debug.Log($"[Skill] {skillName} | State: {newState} | Allowed: {allowed} | Found UI: {found} | cdUI: {cdUI}");

        bool hasCooldown = usedStats.Contains(SkillStatKey.Cooldown);


        if (!allowed)
        {
            // 상태 불허용
            if (passiveRoutine != null)
            {
                StopCoroutine(passiveRoutine);
                passiveRoutine = null;
                Debug.Log($"[Skill] {skillName} | PassiveRoutine stopped due to disallowed state");
            }
            if (cdUI != null)
                cdUI.ForceFill();
            return;
        }
        if (hasCooldown)
        {
            // 쿨다운 있는 패시브 루프
            if (passiveRoutine == null)
            {
                passiveRoutine = StartCoroutine(PassiveLoop(currentCooldown, cdUI, hasCooldown));
                Debug.Log($"[Skill] {skillName} | Started PassiveLoop coroutine");

                if (cdUI != null)
                {
                    cdUI.StartCooldown(currentCooldown);
                    Debug.Log($"[Skill] {skillName} | Started cdUI cooldown for {currentCooldown}s");
                }
                else
                {
                    Debug.LogWarning($"[Skill] {skillName} | cdUI null, cannot start cooldown UI");
                }
            }
        }
        else
        {
            if (cdUI != null)
            {
                cdUI.ForceReset();
                Debug.Log($"[Skill] {skillName} | ForceReset (no cooldown)");
            }
            else
            {
                Debug.LogWarning($"[Skill] {skillName} | cdUI null, cannot ForceReset");
            }
        }
    }


    public bool IsSkillAllowed()
    {
        var state = PlayerStateLogic.Instance.CurrentState;
        return state == PlayerStateLogic.PlayerState.OverDrive ||
               state == PlayerStateLogic.PlayerState.Berserk;
    }

    #endregion


    private void SpawnStormAtRandomPosition()
    {
        Vector2 spawnPos = GetRandomPositionOutsideInnerRadius(transform.position, currentRange, 1f);

        GameObject stormObj = PoolManager.instance.Spawn(stormPrefab, spawnPos);

        Storm storm = stormObj.GetComponent<Storm>();
        if (storm != null)
        {
            storm.Init(currentDamage, currentEffectZoneDuration, currentProjectileSizeLevel);
        }
    }

    private Vector2 GetRandomPositionOutsideInnerRadius(Vector2 center, float baseRange, float innerRadius)
    {
        Vector2 pos;
        do
        {
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = UnityEngine.Random.Range(innerRadius, baseRange);
            pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        } while (Vector2.Distance(center, pos) < innerRadius);
        return pos;
    }
}