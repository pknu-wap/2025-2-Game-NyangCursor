using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerSkill5 : MonoBehaviour, ISkill
{
    [Header("프리팹")]
    [SerializeField] private GameObject auraPrefab;         // 플레이어를 도는 오오라
    [SerializeField] private GameObject waterBeamPrefab;    // 적에게 발사하는 물대포
    [SerializeField] private GameObject beamStartEnd;
    private ParticleSystem auraPS;

    [Header("적 레이어")]
    [SerializeField] private LayerMask enemyLayer;

    private int currentLevel = 0;
    private string skillName;

    [Header("이 스킬이 사용하는 공용 스탯 키들")]
    [SerializeField] private List<SkillStatKey> usedStats = new List<SkillStatKey>();

    [Header("기본값")]
    [SerializeField] private float baseDamage = 5f;
    [SerializeField] private float baseCooldown = 5f;
    [SerializeField] private float baseRange = 4f;
    [SerializeField] private float baseProjectileSize = 1f;

    // 차징 시간은 레벨에 비례해서 감소하도록 설정
    [SerializeField] private float baseChargeTime = 1f;


    // 각 STATKEY별 현재 값
    private Dictionary<SkillStatKey, float> statValues = new Dictionary<SkillStatKey, float>();

    // 현재값 변수
    public float currentCooldown { get; private set; }
    public float currentDamage { get; private set; }
    public float currentRange { get; private set; }
    public float currentProjectileSize { get; private set; }
    public float currentChargeTime { get; private set; }
    private Coroutine passiveRoutine;


    #region 스킬 스크립트 기본 구조

    private void Awake()
    {
        foreach (SkillStatKey key in Enum.GetValues(typeof(SkillStatKey)))
        {
            statValues[key] = 0f;
        }

        // 각 스킬 스크립트에서 쓰이는 변수는 따로 초기화
        statValues[SkillStatKey.Damage] = baseCooldown;
        statValues[SkillStatKey.Cooldown] = baseCooldown;
        statValues[SkillStatKey.Range] = baseRange;
        statValues[SkillStatKey.ProjectileSize] = baseProjectileSize;

        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
        currentRange = baseRange;
        currentProjectileSize = baseProjectileSize;
        currentChargeTime = baseChargeTime;

        if (auraPrefab != null)
        {
            GameObject obj = Instantiate(auraPrefab, transform.position, Quaternion.identity);
            auraPS = obj.GetComponent<ParticleSystem>();
            obj.SetActive(false);
        }
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
            Debug.Log($"{skillName} (패시브 효과 발동 중...)");
            if (showUI)
                cdUI?.StartCooldown(cd);

            float chargeTime = currentChargeTime;      // 장전 시간
            float cooldown = currentCooldown;          // 전체 쿨타임
            float restTime = Mathf.Max(0f, cooldown - chargeTime); // 발사 후 대기시간

            // 1) 오오라 활성 + 페이드인
            if (auraPS != null)
            {
                auraPS.gameObject.SetActive(true);

                // 현재 색상 가져오기
                ParticleSystem.MainModule main = auraPS.main;
                Color startColor = main.startColor.color;
                Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

                // 알파 0으로 초기화
                main.startColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

                float timer = 0f;
                while (timer < chargeTime)
                {
                    timer += Time.deltaTime;
                    float alpha = Mathf.Clamp01(timer / chargeTime);
                    main.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);

                    // 플레이어 주변 회전
                    auraPS.transform.position = transform.position;
                    auraPS.transform.Rotate(Vector3.forward * 180f * Time.deltaTime);

                    yield return null;
                }

                main.startColor = targetColor;
            }

            // 발사
            FireWaterBeam();

            // 오오라 알파 0 후 비활성화
            if (auraPS != null)
            {
                ParticleSystem.MainModule main = auraPS.main;
                Color startColor = main.startColor.color;
                main.startColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
                auraPS.gameObject.SetActive(false);
            }

            // 나머지 시간 대기
            if (restTime > 0f)
                yield return new WaitForSeconds(restTime);
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
                currentChargeTime -= 0.15f;
                MathF.Max(0.1f, currentChargeTime);
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
            case SkillStatKey.ProjectileSize:
                currentProjectileSize = statValues[key];
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
                SkillStatKey.ProjectileSize => baseProjectileSize,
                _ => 0f
            };
        }
        currentLevel = 1;
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
        currentRange = baseRange;
        currentProjectileSize = baseProjectileSize;
        currentChargeTime = baseChargeTime;


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


    private void FireWaterBeam()
    {
        // 1) 범위 내 적 탐색
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, currentRange, enemyLayer);
        Transform target = FindClosestEnemy(transform.position, enemies);
        if (target == null) return;

        Vector2 dir = (target.position - transform.position).normalized;

        // 2) 물대포(LineRenderer) 생성
        GameObject beam = PoolManager.instance.Spawn(waterBeamPrefab, transform.position);

        LineRenderer beamline = beam.GetComponent<LineRenderer>();
        beamline.positionCount = 2;

        // 3) 두께 설정
        float width = currentProjectileSize;
        beamline.startWidth = width;
        beamline.endWidth = width;

        // 4) 길이 설정
        float beamLength = currentRange * 1.3f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (Vector3)dir * beamLength;

        beamline.SetPosition(0, startPos);
        beamline.SetPosition(1, endPos);

        if (beamStartEnd != null)
        {
            // 시작점
            GameObject startObj = PoolManager.instance.Spawn(beamStartEnd, startPos);
            startObj.transform.localScale = Vector3.one * width * 0.3f;

            // 끝점
            GameObject endObj = PoolManager.instance.Spawn(beamStartEnd, endPos);
            endObj.transform.localScale = Vector3.one * width * 0.3f;

            // Beam 사라질 때 같이 despawn
            StartCoroutine(Despawn(startObj, 0.5f));
            StartCoroutine(Despawn(endObj, 0.5f));
        }

        // 5) BoxCast 충돌 처리
        BeamHitCheck(startPos, dir, beamLength, width);

        // 6) 사라지기
        StartCoroutine(Despawn(beam, 0.5f));
    }

    private void BeamHitCheck(Vector2 origin, Vector2 dir, float length, float width)
    {
        // BoxCast 중심 계산
        Vector2 boxCenter = origin + dir * (length * 0.5f);

        // Raycast 박스 크기 (길이 x 너비)
        Vector2 boxSize = new Vector2(length, width * 0.5f);

        // 방향 → 각도 변환
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // BoxCastAll 실행
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            boxCenter,
            boxSize,
            angle,
            Vector2.zero,
            0,
            enemyLayer
        );

        // 충돌 처리
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            EBasicHpController hp = hit.collider.GetComponent<EBasicHpController>();
            if (hp != null)
            {
                hp.TakeDamage(currentDamage); // 데미지 주기
            }
        }
    }


    private IEnumerator Despawn(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        PoolManager.instance.Despawn(obj);
    }


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