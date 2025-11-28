using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerSkil7 : MonoBehaviour, ISkill
{
    [Header("프리팹")]
    [SerializeField] private GameObject fireThrowerPrefab; // 프리팹 참조
    private GameObject fireThrowerInstance; // 생성된 인스턴스
    private ParticleSystem firePS;


    private int currentLevel = 0;
    private string skillName;

    [Header("이 스킬이 사용하는 공용 스탯 키들")]
    [SerializeField] private List<SkillStatKey> usedStats = new List<SkillStatKey>();

    [Header("기본값")]
    [SerializeField] private float baseDamage = 5f;
    [SerializeField] private float baseCooldown = 5f;
    [SerializeField] private float baseRange = 4f;
    [SerializeField] private float baseProjectileSize = 1f;
    [SerializeField] private float baseDuration = 2f;


    // 각 STATKEY별 현재 값
    private Dictionary<SkillStatKey, float> statValues = new Dictionary<SkillStatKey, float>();

    // 현재값 변수
    public float currentCooldown { get; private set; }
    public float currentDamage { get; private set; }
    public float currentRange { get; private set; }
    public float currentProjectileSize { get; private set; }
    public float currnetDuration { get; private set; }
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
        statValues[SkillStatKey.Duration] = baseDuration;

        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
        currentRange = baseRange;
        currentProjectileSize = baseProjectileSize;
        currnetDuration = baseDuration;

        if (fireThrowerPrefab != null)
        {
            fireThrowerInstance = Instantiate(fireThrowerPrefab);
            fireThrowerInstance.transform.SetParent(transform, false);

            Transform flameTransform = fireThrowerInstance.transform.Find("FireSprayFlame");
            if (flameTransform != null)
                firePS = flameTransform.GetComponent<ParticleSystem>();
            fireThrowerInstance.SetActive(false);
        }

        UpdateParticleStats();
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

    private IEnumerator PassiveLoop(float cooldown, SkillCooldownUI cdUI, bool showUI)
    {
        while (true)
        {
            Debug.Log("루프 시작");
            // 1 화염방사기 켬
            fireThrowerInstance.SetActive(true);

            // UI 쿨다운 시작
            if (showUI)
                cdUI?.StartCooldown(cooldown);

            // 2 지속시간 동안 대기
            float timer = 0f;
            while (timer < currnetDuration)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // 3 지속시간 끝나면 화염방사기 끔
            var fireDamage = firePS.GetComponent<FireDamageParticle>();
            fireDamage.SetDeactive();
            fireThrowerInstance.SetActive(false);

            // 4 쿨타임 > 지속시간일 경우 남은 쿨타임만큼 대기
            float remainingCooldown = Mathf.Max(0f, cooldown - currnetDuration);
            if (remainingCooldown > 0f)
                yield return new WaitForSeconds(remainingCooldown);

            // 1번으로 다시 루프
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
            case SkillStatKey.ProjectileSize:
                currentProjectileSize = statValues[key];
                break;
            case SkillStatKey.Duration:
                currnetDuration = statValues[key];
                break;
        }
        UpdateParticleStats();
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
                SkillStatKey.Duration => baseDuration,
                _ => 0f
            };
        }
        currentLevel = 1;
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
        currentRange = baseRange;
        currentProjectileSize = baseProjectileSize;
        currnetDuration = baseDuration;


        StopAllCoroutines();
        passiveRoutine = null;
        ResetParticleStats();
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


    private void UpdateParticleStats()
    {
        if (firePS == null) return;

        var main = firePS.main;
        var emission = firePS.emission;
        var shape = firePS.shape;

        // ============================
        //  0. 기준값
        // ============================
        const float baseMinLifetime = 0.35f;
        const float baseMaxLifetime = 0.50f;

        const float baseAngle = 5f;
        const float baseRate = 100f;

        // 최대 값
        const float maxMinLifetime = 0.8f;
        const float maxMaxLifetime = 1.0f;

        const float maxAngle = 30f;
        const float maxRate = 450f;

        // 스탯 비율
        float rangeFactor = currentRange / baseRange;
        float sizeFactor = currentProjectileSize;

        // ============================
        // 1. Start Lifetime (사거리)
        // ============================
        float newMinLife = baseMinLifetime * rangeFactor;
        float newMaxLife = baseMaxLifetime * rangeFactor;

        // Clamp
        newMinLife = Mathf.Min(newMinLife, maxMinLifetime);
        newMaxLife = Mathf.Min(newMaxLife, maxMaxLifetime);

        main.startLifetime = new ParticleSystem.MinMaxCurve(newMinLife, newMaxLife);

        // ============================
        // 2. Angle (범위)
        // ============================
        float newAngle = baseAngle * sizeFactor;
        shape.angle = Mathf.Clamp(newAngle, 0f, maxAngle);

        // ============================
        // 3. Rate Over Time (파티클 수)
        // ============================
        float newRate = baseRate * Mathf.Sqrt(sizeFactor) * Mathf.Sqrt(rangeFactor);
        emission.rateOverTime = Mathf.Clamp(newRate, 0f, maxRate);

        // ============================
        // 4. 데미지 반영
        // ============================
        var fireDamage = firePS.GetComponent<FireDamageParticle>();
        if (fireDamage != null)
        {
            fireDamage.damagePerTick = currentDamage;
        }
    }



    private void ResetParticleStats()
    {
        if (firePS == null) return;

        var main = firePS.main;
        var emission = firePS.emission;
        var shape = firePS.shape;

        // 0. PS 기본값 (Update와 동일)
        const float baseMinLifetime = 0.35f;
        const float baseMaxLifetime = 0.50f;

        const float baseAngle = 5f;
        const float baseRate = 100f;

        // 1. Lifetime 초기화
        main.startLifetime = new ParticleSystem.MinMaxCurve(baseMinLifetime, baseMaxLifetime);

        // 2. Angle 초기화
        shape.angle = baseAngle;

        // 3. Rate Over Time 초기화
        emission.rateOverTime = baseRate;

        // 4. 데미지 리셋
        var fireDamage = firePS.GetComponent<FireDamageParticle>();
        if (fireDamage != null)
            fireDamage.damagePerTick = baseDamage;

        // 6. 파티클 시스템 비활성화
        fireThrowerInstance.SetActive(false);
    }


}