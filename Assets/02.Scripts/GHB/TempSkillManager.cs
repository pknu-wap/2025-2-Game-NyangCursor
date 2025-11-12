using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempSkillManager : MonoBehaviour, ISkill
{
    private int currentLevel = 0;
    private string skillName;

    public SkillType skillType;

    [Header("이 스킬이 사용하는 공용 스탯 키들")]
    [SerializeField] private List<SkillStatKey> usedStats = new List<SkillStatKey>();

    [Header("기본값 (예시)")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseCooldown = 5f;

    // 각 STATKEY별 현재 값
    private Dictionary<SkillStatKey, float> statValues = new Dictionary<SkillStatKey, float>();

    // 현재값 변수
    public float currentDamage { get; private set; }
    public float currentCooldown { get; private set; }

    private bool isOnCooldown = false;
    private Coroutine cooldownRoutine;
    private Coroutine passiveRoutine;

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

        // 현재값 변수도 초기화
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;
    }


    private void OnEnable()
    {
        //UpgradeManager.OnUpgradeSelected += ApplyUpgrade;
        UpgradeManager.OnUpgradeSelected1 += ApplyUpgrade; //신규
        PlayerStateLogic.Instance.OnStateChanged += HandleStateChanged;

        // 시작 시 현재 상태 확인
        HandleStateChanged(PlayerStateLogic.Instance.CurrentState);
    }

    private void OnDisable()
    {
        //UpgradeManager.OnUpgradeSelected -= ApplyUpgrade;
        UpgradeManager.OnUpgradeSelected1 -= ApplyUpgrade; //신규
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
        // 주석 처리: 쿨다운 값이 0으로 초기화되는 문제 방지
        // statValues[key] *= 1f + data.upgradeRatio;

        // 4. current 변수 갱신
        if (key == SkillStatKey.Damage)
            currentDamage = statValues[key];
        else if (key == SkillStatKey.Cooldown)
            currentCooldown = statValues[key]; // 현재 값 그대로 유지

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
                _ => 0f
            };
        }
        currentLevel = 1;
        currentDamage = baseDamage;
        currentCooldown = baseCooldown;

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
        SkillIconUIManager.Instance?.TryGetCooldownUI(skillName, out cdUI);
        bool hasCooldown = usedStats.Contains(SkillStatKey.Cooldown);

        if (!allowed)
        {
            // 상태 불허용
            if (skillType == SkillType.Passive && passiveRoutine != null)
            {
                StopCoroutine(passiveRoutine);
                passiveRoutine = null;
            }
            cdUI?.ForceFill();
            return;
        }

        // 상태 허용
        if (skillType == SkillType.Passive)
        {
            if (hasCooldown)
            {
                // 쿨다운 있는 패시브 루프
                if (passiveRoutine == null)
                    passiveRoutine = StartCoroutine(PassiveLoop(currentCooldown, cdUI, hasCooldown));
            }
            else
            {
                // 쿨다운 없는 패시브 → UI 바로 시전 가능
                cdUI?.ForceReset();
            }
        }
        else // 액티브형
        {
            if (!isOnCooldown)
                cdUI?.ForceReset();
        }
    }




    public bool IsSkillAllowed()
    {
        var state = PlayerStateLogic.Instance.CurrentState;
        return state == PlayerStateLogic.PlayerState.OverDrive ||
               state == PlayerStateLogic.PlayerState.Berserk;
    }
}
