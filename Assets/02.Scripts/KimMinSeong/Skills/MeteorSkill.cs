using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    [SerializeField] private int maxLevel = 8;   // 최대 레벨
    [SerializeField] private int upgradeStep = 2; // 프리펩 변경 간격
    private int currentLevel = 0; // 현재 레벨

    private string skillName;   // 스킬명
    public SkillType skillType; // 스킬 타입 (Active, Passive)

    private bool isOnCooldown = false;  // 쿨타임 여부 확인용 Flag
    private Coroutine cooldownRoutine;  // 쿨타임 제어 코루틴

    [Header("플레이어 위치를 사용하기 위한 변수")]
    [SerializeField] Transform playerTransform;

    [Header("레벨마다 사용하는 투사체 프리팹들")]
    [SerializeField] private List<GameObject> projectilePrefabs = new List<GameObject>();

    [Header("블록과 투사체 사이 간격 제어 변수")]
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

        // 메테오 생성 코루틴 동작
        StartCoroutine(SpawnMeteorBlocks());
    }

    private IEnumerator CooldownRoutine(float cooldown)
    {
        isOnCooldown = true;
        Debug.Log($"{skillName} 쿨다운 시작 ({cooldown:F1}s)");

        yield return new WaitForSeconds(cooldown);

        isOnCooldown = false;
        Debug.Log($"{skillName} 준비 완료!");
    }

    private IEnumerator SpawnMeteorBlocks()
    {
        // 플레이어 앞쪽으로 블록을 생성하기 위해 플레이어 앞쪽의 방향 벡터를 구함
        Vector3 forward = playerTransform.forward;

        // 해당 블록에서 좌우로 투사체를 생성하기 위해 플레이어 우측의 방향 벡터를 구함
        Vector3 right = playerTransform.right;

        // 리스트에서 현재 레벨에 맞는 메테오 프리팹을 가져옴
        GameObject meteor = GetMeteorForLevel();

        // 블록 수만큼 생성
        for (int blockIndex = 0; blockIndex < CurrentBlockCount; ++blockIndex)
        {
            // 블록마다 간격을 두기 위한 코드
            float depth = blockIndex * blockSpace;

            // 플레이어 위치에서 depth 만큼 전진한 좌표가 블록의 중심
            Vector3 blockCenter = playerTransform.position + forward * depth;

            // 해당 블록의 좌표에 메테오들을 소환
            SpawnMeteors(blockCenter, right, meteor);

            // 다음 블록 생성 전까지 잠시 대기
            // 마지막 블록은 대기할 필요가 없음 
            if (blockIndex <= CurrentBlockCount - 2)
                yield return new WaitForSeconds(CurrentInterval);
        }
    }

    private void SpawnMeteors(Vector3 blockCenter, Vector3 right, GameObject meteor)
    {
        for (int i = 0; i < CurrentProjectileCount; ++i)
        {
            // 중앙을 기준으로 좌우 대칭적으로 투사체를 배치
            float offset = (i - (CurrentProjectileCount - 1) / 2f) * projectileSpace;

            // 생성 위치 계산
            Vector3 spawnPosition = blockCenter + right * offset;

            // 풀매니저 사용
            GameObject spawnedMeteor = PoolManager.instance.Spawn(meteor, spawnPosition);

            // MeteorProjectile 초기화
            MeteorProjectile meteorComponent = spawnedMeteor.GetComponent<MeteorProjectile>();
            if (meteorComponent != null)
                meteorComponent.Initialize(CurrentDamage, spawnPosition);
        }
    }

    private GameObject GetMeteorForLevel()
    {
        if (projectilePrefabs == null || projectilePrefabs.Count == 0)
            return null;

        // 레벨에 맞는 프리팹 선택
        // 이때 리스트는 0-index 이므로 -1 을 하여 보정
        int index = (currentLevel - 1) / upgradeStep;

        // 인덱스 범위를 넘어서지 않도록 보정
        index = Mathf.Clamp(index, 0, projectilePrefabs.Count - 1);

        return projectilePrefabs[index];
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
        // 쿨타임 업그레이드는 쿨타임이 감소하도록 설정
        if (data.statKey == SkillStatKey.Cooldown)
            statValues[data.statKey] *= 1f - data.upgradeRatio;
        else
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
