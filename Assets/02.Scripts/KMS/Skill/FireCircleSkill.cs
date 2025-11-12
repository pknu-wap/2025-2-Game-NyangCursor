using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireCircleSkill : MonoBehaviour, ISkill
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
    private bool isOnCooldown = false;
    private Coroutine cooldownRoutine;

    private Dictionary<SkillStatKey, float> statValues = new Dictionary<SkillStatKey, float>();

    public float currentDamage { get; private set; }
    public float currentCooldown { get; private set; }
    public float currentDuration { get; private set; }
    public float currentSpeed { get; private set; }
    public float currentRange { get; private set; }

    public SkillType SkillType => SkillType.Active;
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
    }

    private void OnDisable()
    {
        UpgradeManager.OnUpgradeSelected1 -= ApplyUpgrade;
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

        if (isOnCooldown)
        {
            Debug.Log($"[FireCircleSkill] 쿨타임 중 ({currentCooldown:F1}s)");
            return;
        }

        Debug.Log($"[FireCircleSkill] 발동! Damage:{currentDamage}, Range:{currentRange}");
        FireCircleActive();
        cooldownRoutine = StartCoroutine(CooldownRoutine(currentCooldown));
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

    private IEnumerator CooldownRoutine(float cooldown)
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
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
        isOnCooldown = false;
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
        // 필요 시 상태별 동작 추가
    }

    public bool IsSkillAllowed()
    {
        var state = PlayerStateLogic.Instance.CurrentState;
        return state == PlayerStateLogic.PlayerState.OverDrive ||
               state == PlayerStateLogic.PlayerState.Berserk;
    }
}
