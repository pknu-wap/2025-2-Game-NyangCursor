using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorSkill : MonoBehaviour, ISkill
{
    [Header("스킬이 사용하는 기본값")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseCooldown = 5f;
    [SerializeField] private float baseInterval = 1f;
    [SerializeField] private int baseProjectileCount = 3;
    [SerializeField] private int baseBlockCount = 3;

    [Header("스킬이 사용하는 공용 스탯 키들")]
    [SerializeField] private List<SkillStatKey> usedStats = new List<SkillStatKey>();

    // SkillStatKey 에 해당하는 값을 관리하기 위한 Dictionary
    private Dictionary<SkillStatKey, float> statValues = new Dictionary<SkillStatKey, float>();

    // 스킬 내부적으로 사용하는 멤버
    private int currentLevel = 0;   // 현재 스킬 레벨   
    private string skillName;   // 스킬명
    public SkillType skillType; // 스킬 타입 (Active, Passive)

    private bool isOnCooldown = false;  // 쿨타임 여부 확인용 Flag
    private Coroutine cooldownRoutine;

    [Header("레벨마다 사용하는 프리팹들")]
    [SerializeField] private List<GameObject> projectilePrefabs = new List<GameObject>();

    [SerializeField] private int blockSpace = 3;    // 블록 사이의 간격
    [SerializeField] private int projectileSpace = 3;    // 블록 내부의 투사체 사이의 간격

    // 프로퍼티 사용하여 멤버에 대해 외부 접근이 가능
    public List<SkillStatKey> UsedStats => usedStats;
    public SkillType SkillType => skillType;
    public int CurrentLevel
    {
        get => currentLevel;
        set => currentLevel = value;
    }
    public float CurrentDamage => statValues[SkillStatKey.Damage];
    public float CurrentCooldown => statValues[SkillStatKey.Cooldown];
    public float CurrentInterval => statValues[SkillStatKey.Interval];
    public int CurrentProjectileCount => (int)statValues[SkillStatKey.ProjectileCount];
    public int CurrentBlockCount => (int)statValues[SkillStatKey.BlockCount];


    private void Awake()
    {
        // 사용하는 스탯에 해당하는 값들을 초기화
        statValues[SkillStatKey.Damage] = baseDamage;
        statValues[SkillStatKey.Cooldown] = baseCooldown;
        statValues[SkillStatKey.Interval] = baseInterval;
        statValues[SkillStatKey.ProjectileCount] = baseProjectileCount;
        statValues[SkillStatKey.BlockCount] = baseBlockCount;
    }

    private void OnEnable()
    {
        //UpgradeManager.OnUpgradeSelected += ApplyUpgrade;
        UpgradeManager1.OnUpgradeSelected1 += ApplyUpgrade; // UpgradeManager 리팩토링이 안되어있는 상태라 UpgradeManager1 사용
        PlayerStateLogic.Instance.OnStateChanged += HandleStateChanged;

        // 스킬이 활성화될 때, 현재 플레이어 상태 확인
        HandleStateChanged(PlayerStateLogic.Instance.CurrentState);
    }

    private void OnDisable()
    {
        //UpgradeManager.OnUpgradeSelected -= ApplyUpgrade;
        UpgradeManager1.OnUpgradeSelected1 -= ApplyUpgrade;  // UpgradeManager 리팩토링이 안되어있는 상태라 UpgradeManager1 사용
        PlayerStateLogic.Instance.OnStateChanged -= HandleStateChanged;
    }

    public void Activate()
    {
        // 현재 플레이어 상태에서 사용 가능한지 체크
        if (!IsSkillAllowed())
        {
            Debug.Log($"{skillName} 현재 상태에서 사용 불가");
            return;
        }
        
        // 쿨타임이라면 Skip
        if (isOnCooldown)
        {
            Debug.Log($"{skillName} 쿨타임 중입니다!");
            return;
        }

        Debug.Log($"{skillName} 발동!");

        // 쿨타임 UI 동작
        SkillCooldownUI cdUI = null;
        SkillIconUIManager.Instance.TryGetCooldownUI(skillName, out cdUI);
        cdUI?.StartCooldown(CurrentCooldown);

        // 스킬 쿨타임 동작
        cooldownRoutine = StartCoroutine(CooldownRoutine(CurrentCooldown));
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
        // 1. 유효한 스킬 이벤트인지 확인, 맞다면 레벨업
        if (IsSkillMatch(data))
            currentLevel++;

        // 2. usedStats에 있는 키만 처리
        if (!usedStats.Contains(data.statKey))
            return;

        // 3. 스탯에 해당하는 값을 갱신
        statValues[data.statKey] *= 1f + data.upgradeRatio;

        Debug.Log($"스킬 업그레이드! 스탯 = {data.statKey}, 값 = {statValues[data.statKey]:F1}, 비율 = {data.upgradeRatio:F1}");
    }

    public void SetSkill(string skillName)
    {
        this.skillName = skillName;
    }

    public void ResetSkill()
    {
        // 사용하는 스탯의 값들을 초기화
        statValues[SkillStatKey.Damage] = baseDamage;
        statValues[SkillStatKey.Cooldown] = baseCooldown;
        statValues[SkillStatKey.Interval] = baseInterval;
        statValues[SkillStatKey.ProjectileCount] = baseProjectileCount;
        statValues[SkillStatKey.BlockCount] = baseBlockCount;

        currentLevel = 0;

        Debug.Log($"{gameObject.name} 스킬 초기화 완료");
    }

    // 플레이어 상태에 따라 스킬 사용 가능 여부를 제어하는 로직
    public void HandleStateChanged(PlayerStateLogic.PlayerState newState)
    {
        SkillCooldownUI cdUI = null;
        SkillIconUIManager.Instance?.TryGetCooldownUI(skillName, out cdUI);

        // 스킬을 사용할 수 없는 상태라면
        // UI 에서 시전 불가능을 표시 
        if (!IsSkillAllowed())
        {
            cdUI?.ForceFill();
            return;
        }

        // 스킬을 사용할 수 있는 상태며, 쿨타임이 지났다면
        // UI 에서 시전 가능함을 표시
        if (!isOnCooldown)
            cdUI?.ForceReset();
    }

    public bool IsSkillAllowed()
    {
        var state = PlayerStateLogic.Instance.CurrentState;
        return state == PlayerStateLogic.PlayerState.OverDrive ||
               state == PlayerStateLogic.PlayerState.Berserk;
    }

    public bool IsSkillMatch(UpgradeEventData data)
    {
        if (!data.isSkillUpgrade)
            return false;

        return string.Equals(data.skillName, skillName, StringComparison.OrdinalIgnoreCase);
    }
}
