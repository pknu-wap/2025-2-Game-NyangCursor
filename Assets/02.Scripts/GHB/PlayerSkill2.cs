using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerSkill2 : MonoBehaviour, ISkill
{

    [SerializeField] private GameObject lightningEffectPrefab;

    [SerializeField] private GameObject hitEffectPrefab; // A 프리팹 할당

    private int currentLevel = 0;
    private string skillName;

    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private LayerMask enemyMask;

    [Header("이 스킬이 사용하는 공용 스탯 키들")]
    [SerializeField] private List<SkillStatKey> usedStats = new List<SkillStatKey>();

    [Header("기본값")]
    [SerializeField] private float baseDamage = 2f;
    [SerializeField] private float baseCooldown = 5f;
    [SerializeField] private float baseRange = 3f;
    [SerializeField] private float baseProjectileCount = 1;
    [SerializeField] private int baseDepth = 1; // 깊이는 너무 종속되는 변수여서 레벨당으로 할당


    // 각 STATKEY별 현재 값
    private Dictionary<SkillStatKey, float> statValues = new Dictionary<SkillStatKey, float>();

    // 현재값 변수
    public float currentDamage { get; private set; }
    public float currentCooldown { get; private set; }
    public float currentRange { get; private set; }
    public float currentProjectileCount { get; private set; }
    public int currentDepth { get; private set; }

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

        // 현재값 변수도 초기화
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
        currentRange = baseRange;
        currentProjectileCount = baseProjectileCount;
        currentDepth = baseDepth;
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
            FireLightning();
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
                if (currentLevel % 2 == 0)
                {
                    // 번개 깊이는 레벨에 종속, 2레벨마다 업그레이드
                    currentDepth++;
                    /*currentDepth = Mathf.Min(currentDepth + 1, 3);*/
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
            case SkillStatKey.Range:
                currentRange = statValues[key];
                break;
            case SkillStatKey.ProjectileCount:
                currentProjectileCount = statValues[key];
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
                _ => 0f
            };
        }
        currentLevel = 1;
        currentDepth = 1;
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
        currentProjectileCount = baseProjectileCount;
        currentRange = baseRange;


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

    private void FireLightning()
    {
        // 1) 범위 안의 적 찾기
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, currentRange, enemyMask);
        if (enemies.Length == 0) return;

        // 2) 거리순 정렬
        List<Transform> sortedEnemies = new List<Transform>();
        foreach (var e in enemies) sortedEnemies.Add(e.transform);

        sortedEnemies.Sort((a, b) =>
            Vector2.Distance(transform.position, a.position)
            .CompareTo(Vector2.Distance(transform.position, b.position)));

        // 3) 발사 개수 (최대 5 제한)
        int shots = (int)Mathf.Clamp(currentProjectileCount, 1, 5);
        int count = Mathf.Min(shots, sortedEnemies.Count);

        for (int i = 0; i < count; i++)
        {
            Transform startTarget = sortedEnemies[i];
            StartCoroutine(ChainLightning(startTarget, currentDepth));
        }
    }

    private IEnumerator ChainLightning(Transform startEnemy, int depth)
    {
        HashSet<Transform> visited = new HashSet<Transform>();
        Transform current = startEnemy;

        for (int i = 0; i < Mathf.Clamp(depth, 1, 5); i++)
        {
            if (current == null) yield break;

            visited.Add(current);

            ApplyDamage(current);
            Debug.Log($"Hit: {current.name}");

            // Player → current (첫 번째는 플레이어 기준)
            if (i == 0)
                SpawnLightningEffect(transform.position, current.position);

            // 다음 적 찾기 (범위 내 방문하지 않은 랜덤 적)
            Transform next = FindRandomNextEnemy(current.position, visited);
            if (next == null) yield break;

            // current → next
            SpawnLightningEffect(current.position, next.position);

            current = next;

            yield return null;
        }
    }



    private Transform FindRandomNextEnemy(Vector2 from, HashSet<Transform> visited)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(from, currentRange, enemyMask);

        List<Transform> candidates = new List<Transform>();
        foreach (var h in hits)
        {
            Transform t = h.transform;
            if (!visited.Contains(t))
                candidates.Add(t);
        }

        if (candidates.Count == 0)
            return null;

        // 랜덤으로 선택
        int index = UnityEngine.Random.Range(0, candidates.Count);
        return candidates[index];
    }

    private void ApplyDamage(Transform enemy)
    {
        var hp = enemy.GetComponent<IDamageable>();
        if (hp != null)
        {
            hp.TakeDamage(currentDamage);
            /// 타격 효과 생성 (PoolManager 사용)
            if (hitEffectPrefab != null)
            {
                GameObject effect = PoolManager.instance.Spawn(hitEffectPrefab, enemy.position);
                // 일정 시간 후 다시 Pool로 반환
                StartCoroutine(DespawnAfter(effect, 0.5f));
            }
        }


    }

    private void SpawnLightningEffect(Vector3 start, Vector3 end)
    {
        if (lightningEffectPrefab != null)
        {
            GameObject obj = PoolManager.instance.Spawn(lightningEffectPrefab, start);
            LineRenderer lr = obj.GetComponent<LineRenderer>();

            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            // 일정 시간 후 다시 Pool로 반환
            StartCoroutine(DespawnAfter(obj, 0.15f));
        }
    }

    private IEnumerator DespawnAfter(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        PoolManager.instance.Despawn(obj);
    }

}