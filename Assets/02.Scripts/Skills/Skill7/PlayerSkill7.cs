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
                fireThrowerInstance.SetActive(false);
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

        // ==================== 1. 투사체 크기 적용 ====================
        // ProjectileSize 증가 → angle 확대, startSize 확대
        shape.angle = shape.angle * currentProjectileSize;

        // 현재 startSpeed가 constant인지 확인 후 값을 꺼내기
        float baseSpeed = main.startSpeed.constant;

        float rangeFactor = currentRange / baseRange;
        main.startSpeed = baseSpeed * rangeFactor;

        // ==================== 3. Emission rate 보정 ====================
        // 크기, 범위 변화에 따라 자연스럽게 rate 조정
        float baseRate = emission.rateOverTime.constant;
        float sizeFactor = currentProjectileSize;
        emission.rateOverTime = baseRate * Mathf.Sqrt(sizeFactor) * Mathf.Sqrt(rangeFactor);

        // ==================== 4. 데미지는 FireDamageParticle에서 처리 ====================
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

        // PS 기본값 초기화
        main.startSize = 1.8f;
        main.startSpeed = 40f;
        shape.angle = 20f;
        emission.rateOverTime = 200f;

        // Damage 초기화
        var fireDamage = firePS.GetComponent<FireDamageParticle>();
        if (fireDamage != null)
            fireDamage.damagePerTick = baseDamage;

        // PS 끄기
        fireThrowerInstance.SetActive(false);
    }

}