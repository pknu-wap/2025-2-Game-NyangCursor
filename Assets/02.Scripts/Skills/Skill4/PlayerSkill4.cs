using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerSkill4 : MonoBehaviour, ISkill
{

    [SerializeField] private GameObject projectilePrefab;

    private int currentLevel = 0;
    private string skillName;

    [SerializeField] private LayerMask enemyMask;

    [Header("이 스킬이 사용하는 공용 스탯 키들")]
    [SerializeField] private List<SkillStatKey> usedStats = new List<SkillStatKey>();

    [Header("기본값")]
    [SerializeField] private float baseDamage = 2f;
    [SerializeField] private float baseCooldown = 5f;
    [SerializeField] private float baseRange = 3f;
    [SerializeField] private float baseProjectileSizeLevel = 1f;
    [SerializeField] private float baseProjectileCount = 1f;
    [SerializeField] private float baseEffectZoneDuration = 1f;


    // 각 STATKEY별 현재 값
    private Dictionary<SkillStatKey, float> statValues = new Dictionary<SkillStatKey, float>();

    // 현재값 변수
    public float currentDamage { get; private set; }
    public float currentCooldown { get; private set; }
    public float currentRange { get; private set; }
    public float currentProjectileSizeLevel { get; private set; }
    public float currentProjectileCount { get; private set; }
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
        statValues[SkillStatKey.ProjectileCount] = baseProjectileCount;
        statValues[SkillStatKey.EffectZoneDuration] = baseEffectZoneDuration;
        statValues[SkillStatKey.ProjectileSize] = baseProjectileSizeLevel;

        // 현재값 변수도 초기화
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
        currentRange = baseRange;
        currentProjectileCount = baseProjectileCount;
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
            FireProjectiles();
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
            case SkillStatKey.ProjectileCount:
                currentProjectileCount = statValues[key];
                break;
            case SkillStatKey.EffectZoneDuration:
                currentEffectZoneDuration = statValues[key];
                break;
            case SkillStatKey.ProjectileSize:
                currentProjectileSizeLevel = statValues[key];
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
                SkillStatKey.ProjectileCount => baseProjectileCount,
                SkillStatKey.Range => baseRange,
                SkillStatKey.EffectZoneDuration => baseEffectZoneDuration,
                SkillStatKey.ProjectileSize => baseProjectileSizeLevel,
                _ => 0f
            };
        }
        currentLevel = 1;
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
        currentProjectileCount = baseProjectileCount;
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

    private void FireProjectiles()
    {
        // 1) 범위 안 적 검색
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, currentRange, enemyMask);
        if (enemies.Length == 0)
            return; // 주변 적이 없으면 발사 안 함

        // 2) 발사할 투사체 수
        int projectileCount = Mathf.Max(1, Mathf.RoundToInt(currentProjectileCount));

        for (int i = 0; i < projectileCount; i++)
        {
            // PoolManager에서 투사체 가져오기
            GameObject projObj = PoolManager.instance.Spawn(projectilePrefab, transform.position);

            // IProjectile 세팅
            if (projObj.TryGetComponent<ElectricProjectile1>(out var projectile))
            {
                projectile.SetDamage(currentDamage);
                projectile.SetDuration(currentEffectZoneDuration); // 투사체 지속 시간
                projectile.SetSize(currentProjectileSizeLevel);
                projectile.effectZoneDuration = currentEffectZoneDuration;
            }

            // Rigidbody2D로 발사 방향 세팅: 현재 가장 가까운 적 방향
            Transform target = FindClosestEnemy(transform.position, enemies);
            if (projObj.TryGetComponent<Rigidbody2D>(out var rb))
            {
                Vector2 dir = target != null ? (target.position - transform.position).normalized : Vector2.right;
                rb.linearVelocity = dir * 4f; // 발사체 속도는 고정
            }
        }
    }

    // 가장 가까운 적 찾기
    private Transform FindClosestEnemy(Vector3 from, Collider2D[] enemies)
    {
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var e in enemies)
        {
            float dist = Vector2.Distance(from, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = e.transform;
            }
        }

        return closest;
    }

}