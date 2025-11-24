using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill2 : MonoBehaviour, ISkill
{
    [Header("기본 스탯")]
    [SerializeField] private float baseDamage = 2f;
    [SerializeField] private float baseCooldown = 5f;
    [SerializeField] private float baseDuration = 3f;
    [SerializeField] private float baseSpeed = 1f;
    [SerializeField] private float baseRange = 1f;

    [Header("이 스킬이 사용하는 공용 스탯 키들")]
    [SerializeField] private List<SkillStatKey> usedStats = new List<SkillStatKey>();

    [Header("레벨별 프리팹 (4개)")]
    [SerializeField] private List<GameObject> fireCirclePrefabs = new List<GameObject>();

    [Header("플레이어 참조")]
    [SerializeField] private Transform playerTransform;

    private int currentLevel = 1;
    private string skillName;
    private Coroutine passiveRoutine;

    private Dictionary<SkillStatKey, float> statValues = new Dictionary<SkillStatKey, float>();

    public float currentDamage { get; private set; }
    public float currentCooldown { get; private set; }
    public float currentDuration { get; private set; }
    public float currentSpeed { get; private set; }
    public float currentRange { get; private set; }
    public List<SkillStatKey> UsedStats => usedStats;
    public int CurrentLevel { get => currentLevel; set => currentLevel = value; }

    private void Awake()
    {
        // 기본 스탯 초기화
        statValues[SkillStatKey.Damage] = baseDamage;
        statValues[SkillStatKey.Cooldown] = baseCooldown;
        statValues[SkillStatKey.Duration] = baseDuration;
        statValues[SkillStatKey.Speed] = baseSpeed;
        statValues[SkillStatKey.Range] = baseRange;

        SyncCurrentValues();
    }

    private void OnEnable()
    {
        UpgradeManager.OnUpgradeSelected1 += ApplyUpgrade;
        PlayerStateLogic.Instance.OnStateChanged += HandleStateChanged;

        // 스킬이 활성화될 때, 현재 플레이어 상태 확인
        HandleStateChanged(PlayerStateLogic.Instance.CurrentState);
    }

    private void OnDisable()
    {
        UpgradeManager.OnUpgradeSelected1 -= ApplyUpgrade;
        PlayerStateLogic.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void SyncCurrentValues()
    {
        currentDamage = statValues[SkillStatKey.Damage];
        currentCooldown = statValues[SkillStatKey.Cooldown];
        currentDuration = statValues[SkillStatKey.Duration];
        currentSpeed = statValues[SkillStatKey.Speed];
        currentRange = statValues[SkillStatKey.Range];
    }

    public void Activate()
    {
        if (!IsSkillAllowed())
        {
            Debug.Log("[FireCircleSkill] 현재 상태에서는 사용 불가");
            return;
        }
        SkillCooldownUI cdUI = null;
        SkillIconUIManager.Instance.TryGetCooldownUI(skillName, out cdUI);

        bool hasCooldownStat = usedStats.Contains(SkillStatKey.Cooldown);

        Debug.Log($"[FireCircleSkill] 발동! Damage:{currentDamage}, Range:{currentRange}");
        passiveRoutine = StartCoroutine(PassiveLoop(currentCooldown, cdUI, hasCooldownStat));
    }

    private IEnumerator PassiveLoop(float cd, SkillCooldownUI cdUI, bool showUI)
    {
        while (true)
        {
            FireCircleActive();
            Debug.Log($"{skillName} (패시브 효과 발동 중...)");
            // 쿨다운 StatKey가 있는 경우만 UI 표시
            if (showUI)
                cdUI?.StartCooldown(cd);

            yield return new WaitForSeconds(cd);
        }
    }

    private void FireCircleActive()
    {
        if (fireCirclePrefabs.Count == 0)
        {
            Debug.LogWarning("[FireCircleSkill] 프리팹이 비어 있습니다.");
            return;
        }

        int prefabIndex = Mathf.Clamp((currentLevel - 1) / 2, 0, fireCirclePrefabs.Count - 1);
        GameObject prefab = fireCirclePrefabs[prefabIndex];

        GameObject circle = Instantiate(prefab, transform.position, Quaternion.identity);

        if (circle.TryGetComponent<CircleSkillLogic>(out var logic))
        {
            // 플레이어의 Transform을 넘겨서 따라가게
            logic.Initialize(currentDamage, currentSpeed, currentRange, transform);
        }

        Destroy(circle, currentDuration);
    }

    public void ApplyUpgrade(UpgradeEventData data)
    {
        SkillStatKey key = data.statKey;

        if (data.isSkillUpgrade)
        {
            if (!string.Equals(data.skillName, skillName, StringComparison.OrdinalIgnoreCase))
                return;

            if (data.applyLevelUp) // applyLevelUp가 true일 때만 레벨 증가
                currentLevel++;
        }

        if (!usedStats.Contains(key))
            return;

        float ratio = data.upgradeRatio;

        if (key == SkillStatKey.Cooldown)
            statValues[key] *= (1f - ratio);
        else
            statValues[key] *= (1f + ratio);

        SyncCurrentValues();
        Debug.Log($"[FireCircleSkill] 강화 적용: {key} {(key == SkillStatKey.Cooldown ? "-" : "+")}{ratio:P1} → {statValues[key]:F2}");
    }

    public void SetSkill(string skillName)
    {
        this.skillName = skillName;
    }

    public void ResetSkill()
    {
        StopAllCoroutines();
        currentLevel = 1;

        statValues[SkillStatKey.Damage] = baseDamage;
        statValues[SkillStatKey.Cooldown] = baseCooldown;
        statValues[SkillStatKey.Duration] = baseDuration;
        statValues[SkillStatKey.Speed] = baseSpeed;
        statValues[SkillStatKey.Range] = baseRange;

        SyncCurrentValues();
    }

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
}
