using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill1 : MonoBehaviour, ISkill
{
    private int currentLevel = 0;
    private string skillName;

    [SerializeField] private Rigidbody2D playerRb;

    [Header("이 스킬이 사용하는 공용 스탯 키들")]
    [SerializeField] private List<SkillStatKey> usedStats = new List<SkillStatKey>();

    [Header("기본값")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseCooldown = 5f;
    [SerializeField] private float baseDuration = 3f;
    [SerializeField] private int baseProjectileCount = 2;
    [SerializeField] private int baseProjectileSizeLevel = 1;
    [SerializeField] private float baseProjectileSpeed = 3f;


    // 각 STATKEY별 현재 값
    private Dictionary<SkillStatKey, float> statValues = new Dictionary<SkillStatKey, float>();

    // 현재값 변수
    public float currentDamage { get; private set; }
    public float currentCooldown { get; private set; }
    public float currentDuration { get; private set; }
    public float currentProjectileCount { get; private set; }
    public float currentProjectileSizeLevel { get; private set; }
    public float currentProjectileSpeed { get; private set; }

    private Coroutine passiveRoutine;

    [Header("개별 스킬 설정")]
    [Header("레벨별 스킬 프리팹")]
    [SerializeField] private List<GameObject> projectilePrefabs = new List<GameObject>();


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
        statValues[SkillStatKey.Duration] = baseDuration;
        statValues[SkillStatKey.ProjectileCount] = baseProjectileCount;
        statValues[SkillStatKey.ProjectileSize] = baseProjectileSizeLevel;

        // 현재값 변수도 초기화
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
        currentDuration = baseDuration;
        currentProjectileSpeed = baseProjectileSpeed;
        currentProjectileCount = baseProjectileCount;
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
            FireProjectiles(); // 스킬 작동 함수가 들어가면 됩니다
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
                // 레벨은 항상 1씩 증가
                currentLevel++;

                // 2레벨당 한 번씩(2, 4, 6, ...) 업그레이드
                if (currentLevel % 2 == 0)
                {
                    currentProjectileSpeed += 1f;
                    currentProjectileCount += 1;
                    statValues[SkillStatKey.ProjectileCount] += 1;
                }
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
            case SkillStatKey.Duration:
                currentDuration = statValues[key];
                break;
            case SkillStatKey.ProjectileCount:
                currentProjectileCount = statValues[key];
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
                SkillStatKey.Duration => baseDuration,
                SkillStatKey.ProjectileCount => baseProjectileCount,
                SkillStatKey.ProjectileSize => baseProjectileSizeLevel,
                _ => 0f
            };
        }
        currentLevel = 1;
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
        currentDuration = baseDuration;
        currentProjectileSpeed = baseProjectileSpeed;
        currentProjectileCount = baseProjectileCount;
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
        if (projectilePrefabs.Count == 0)
        {
            Debug.LogWarning($"{skillName}: 발사할 프리팹이 없습니다!");
            return;
        }

        int projectileCount = Mathf.Max(1, Mathf.RoundToInt(currentProjectileCount));
        float angleStep = 360f / projectileCount;

        Vector2 moveDir = playerRb.linearVelocity.normalized;

        // 🔹 이동 방향의 수직 벡터 (왼쪽 방향)
        Vector2 perpendicular = new Vector2(-moveDir.y, moveDir.x);
        float baseAngle = Mathf.Atan2(perpendicular.y, perpendicular.x) * Mathf.Rad2Deg;

        for (int i = 0; i < projectileCount; i++)
        {
            GameObject prefabToUse = GetProjectilePrefabForLevel();

            // PoolManager에서 스폰
            GameObject proj = PoolManager.instance.Spawn(prefabToUse, transform.position);

            // 레벨별 사이즈 조정
            int level = Mathf.Clamp(currentLevel, 1, 8);
            if (level % 2 == 0)
            {
                // 기본 스케일 대비 배율 적용
                proj.transform.localScale *= 1.2f;
            }

            float angle = baseAngle + (i * angleStep);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            // IProjectile 세팅
            if (proj.TryGetComponent<IProjectile>(out var projectile))
            {
                projectile.SetDamage(currentDamage);
                projectile.SetDuration(currentDuration);
            }

            // Rigidbody 세팅
            if (proj.TryGetComponent<Rigidbody2D>(out var rb2d))
            {
                rb2d.linearVelocity = dir.normalized * currentProjectileSpeed;
            }
        }
    }





    // ================== 투사체 프리팹/크기 관리 ==================
    private GameObject GetProjectilePrefabForLevel()
    {
        int level = Mathf.Clamp(currentLevel, 1, 8);

        int prefabIndex = (level - 1) / 2;
        prefabIndex = Mathf.Clamp(prefabIndex, 0, projectilePrefabs.Count - 1);

        return projectilePrefabs[prefabIndex]; // 여기선 절대 Instantiate 하지 말기
    }


}
