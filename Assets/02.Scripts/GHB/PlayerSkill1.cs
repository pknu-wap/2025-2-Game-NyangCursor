using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill1 : MonoBehaviour, ISkill
{
    private int currentLevel = 0;
    private string skillName;

    public SkillType skillType;

    [SerializeField] private Rigidbody2D playerRb;

    [Header("이 스킬이 사용하는 공용 스탯 키들")]
    [SerializeField] private List<SkillStatKey> usedStats = new List<SkillStatKey>();

    [Header("기본값")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseCooldown = 5f;
    [SerializeField] private float baseDuration = 3f;
    [SerializeField] private float baseSpeed = 3f;
    [SerializeField] private int baseProjectileCount = 2;
    [SerializeField] private int baseProjectileSizeLevel = 0;


    // 각 STATKEY별 현재 값
    private Dictionary<SkillStatKey, float> statValues = new Dictionary<SkillStatKey, float>();

    // 현재값 변수
    public float currentDamage { get; private set; }
    public float currentCooldown { get; private set; }
    public float currentDuration { get; private set; }
    public float currentSpeed { get; private set; }
    public float currentProjectileCount { get; private set; }
    public float currentProjectileSizeLevel { get; private set; }

    private bool isOnCooldown = false;
    private Coroutine cooldownRoutine;
    private Coroutine passiveRoutine;

    [Header("개별 스킬 설정")]
    [Header("레벨별 스킬 프리팹")]
    [SerializeField] private List<GameObject> projectilePrefabs = new List<GameObject>();

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
        statValues[SkillStatKey.Duration] = baseDuration;
        statValues[SkillStatKey.Speed] = baseSpeed;
        statValues[SkillStatKey.ProjectileCount] = baseProjectileCount;
        statValues[SkillStatKey.ProjectileSize] = baseProjectileSizeLevel;

        // 현재값 변수도 초기화
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
        currentDuration = baseDuration;
        currentSpeed = baseSpeed;
        currentProjectileCount = baseProjectileCount;
        currentProjectileSizeLevel = baseProjectileSizeLevel;
    }


    private void OnEnable()
    {
        //UpgradeManager.OnUpgradeSelected += ApplyUpgrade;
        PlayerStateLogic.Instance.OnStateChanged += HandleStateChanged;
        UpgradeManager.OnUpgradeSelected1 += ApplyUpgrade;
        // 시작 시 현재 상태 확인
        HandleStateChanged(PlayerStateLogic.Instance.CurrentState);
    }

    private void OnDisable()
    {
        UpgradeManager.OnUpgradeSelected1 += ApplyUpgrade;
        //UpgradeManager.OnUpgradeSelected -= ApplyUpgrade;
        PlayerStateLogic.Instance.OnStateChanged -= HandleStateChanged;
    }

    public void Activate()
    {
        // 현재 상태에서 사용 가능한지 체크
        if (!IsSkillAllowed())
        {
            Debug.Log($"{skillName} 현재 상태에서 사용 불가");
            return;
        }

        SkillCooldownUI cdUI = null;
        SkillIconUIManager.Instance.TryGetCooldownUI(skillName, out cdUI);

        bool hasCooldownStat = usedStats.Contains(SkillStatKey.Cooldown);

        if (skillType == SkillType.Active)
        {
            if (isOnCooldown)
            {
                Debug.Log($"{skillName} 쿨타임 중입니다!");
                return;
            }

            Debug.Log($"{skillName} 발동! (Damage {currentDamage})");

            // 실제 발사 함수 호출
            FireProjectiles();

            // 수정했어요: 쿨다운 StatKey가 있는 경우만 UI 실행
            if (hasCooldownStat)
                cdUI?.StartCooldown(currentCooldown);

            cooldownRoutine = StartCoroutine(CooldownRoutine(currentCooldown));
        }
        else // 패시브형
        {
            // 수정했어요: 패시브 루프 UI 연동, StatKey 없으면 UI 생략
            if (cooldownRoutine == null)
                cooldownRoutine = StartCoroutine(PassiveLoop(currentCooldown, cdUI, hasCooldownStat));
        }
    }

    // 수정했어요: PassiveLoop에 StatKey 체크 추가
    private IEnumerator PassiveLoop(float cd, SkillCooldownUI cdUI, bool showUI)
    {
        while (true)
        {
            Debug.Log($"{skillName} (패시브 효과 발동 중...)");

            // 쿨다운 StatKey가 있는 경우만 UI 표시
            if (showUI)
                cdUI?.StartCooldown(cd);

            yield return new WaitForSeconds(cd);
        }
    }


    private IEnumerator CooldownRoutine(float cooldown)
    {
        isOnCooldown = true;
        Debug.Log($"{skillName} 쿨다운 시작 ({cooldown:F1}s)");

        yield return new WaitForSeconds(cooldown);

        isOnCooldown = false;
        Debug.Log($"{skillName} 준비 완료!");
    }

    public void ApplyUpgrade(UpgradeEventData data)
    {
        SkillStatKey key = data.statKey;

        // 1. 스킬 이벤트인지 확인
        if (data.isSkillUpgrade)
        {
            if (!string.Equals(data.skillName, skillName, StringComparison.OrdinalIgnoreCase))
                return;

            if (data.applyLevelUp) // applyLevelUp가 true일 때만 레벨 증가
                currentLevel++;
        }

        // 2. usedStats에 있는 키만 처리
        if (!usedStats.Contains(key))
            return;

        // 3. statValues 갱신
        if (key == SkillStatKey.Cooldown)
        {
            // 쿨다운은 감소 처리
            statValues[key] -= data.upgradeRatio;
        }
        else
        {
            statValues[key] += data.upgradeRatio;
        }

        // 4. current 변수 갱신
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
            case SkillStatKey.Speed:
                currentSpeed = statValues[key];
                break;
            case SkillStatKey.ProjectileCount:
                currentProjectileCount = statValues[key];
                break;
            case SkillStatKey.ProjectileSize:
                currentProjectileSizeLevel = statValues[key];
                break;
        }

        Debug.Log($"[UpgradeEvent] {gameObject.name}: {key} +{data.upgradeRatio:P1} -> {statValues[key]:F2}");
    }


    public void ResetSkill()
    {
        // statValues 초기화
        foreach (SkillStatKey key in Enum.GetValues(typeof(SkillStatKey)))
        {
            statValues[key] = key switch
            {
                SkillStatKey.Damage => baseDamage,
                SkillStatKey.Cooldown => baseCooldown,
                SkillStatKey.Duration => baseDuration,
                SkillStatKey.Speed => baseSpeed,
                SkillStatKey.ProjectileCount => baseProjectileCount,
                SkillStatKey.ProjectileSize => baseProjectileSizeLevel,
                _ => 0f
            };
        }
        currentLevel = 1;
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
        currentDuration = baseDuration;
        currentSpeed = baseSpeed;
        currentProjectileCount = baseProjectileCount;
        currentProjectileSizeLevel = baseProjectileSizeLevel;

        StopAllCoroutines();
        isOnCooldown = false;
        passiveRoutine = null;

        Debug.Log($"{gameObject.name} 스킬 초기화 완료");
    }

    public void SetSkill(string skillName)
    {
        this.skillName = skillName;
    }


    // UsedStats 공개
    public List<SkillStatKey> UsedStats => usedStats;

    // 스킬 타입(쿨타임형, 패시브형)
    public SkillType SkillType => skillType;

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
            if (skillType == SkillType.Passive && passiveRoutine != null)
            {
                StopCoroutine(passiveRoutine);
                passiveRoutine = null;
                Debug.Log($"[Skill] {skillName} | PassiveRoutine stopped due to disallowed state");
            }
            if (cdUI != null)
                cdUI.ForceFill();
            else
                Debug.LogWarning($"[Skill] {skillName} | ForceFill skipped, cdUI is null");
            return;
        }

        // 상태 허용
        if (skillType == SkillType.Passive)
        {
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
        else // 액티브형
        {
            if (!isOnCooldown)
            {
                if (cdUI != null)
                {
                    cdUI.StartCooldown(currentCooldown);
                    Debug.Log($"[Skill] {skillName} | Active skill StartCooldown for {currentCooldown}s");
                }
                else
                {
                    Debug.LogWarning($"[Skill] {skillName} | cdUI null, cannot StartCooldown");
                }
            }
        }
    }





    public bool IsSkillAllowed()
    {
        var state = PlayerStateLogic.Instance.CurrentState;
        return state == PlayerStateLogic.PlayerState.OverDrive ||
               state == PlayerStateLogic.PlayerState.Berserk;
    }

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
            GameObject proj = Instantiate(prefabToUse, transform.position, Quaternion.identity);

            float angle = baseAngle + (i * angleStep);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            if (proj.TryGetComponent<IProjectile>(out var projectile))
            {
                projectile.SetDamage(currentDamage);
                projectile.SetDuration(currentDuration);
            }

            if (proj.TryGetComponent<Rigidbody2D>(out var rb2d))
            {
                rb2d.linearVelocity = dir.normalized * currentSpeed;
            }
        }
    }




    // ================== 투사체 프리팹/크기 관리 ==================
    private GameObject GetProjectilePrefabForLevel()
    {
        // 최대 8단계
        int level = Mathf.Clamp(currentLevel, 1, 8);

        // 각 프리팹당 2단계씩
        int prefabIndex = (level - 1) / 2;
        prefabIndex = Mathf.Clamp(prefabIndex, 0, projectilePrefabs.Count - 1);

        GameObject prefab = projectilePrefabs[prefabIndex];

        // 홀수 단계면 약간 확대
        if (level % 2 == 0)
        {
            prefab = Instantiate(prefab);
            prefab.transform.localScale *= 1.2f; // 임의 크기 증가
        }

        return prefab;
    }

}
