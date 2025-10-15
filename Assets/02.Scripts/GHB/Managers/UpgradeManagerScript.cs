using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// CSV(EffectDatabase)에서 로드한 EffectData를 풀로 사용하여
/// 슬롯을 구성/적용하는 매니저.
/// - ScriptableObject 의존성 제거
/// - 풀: gauge(게이지강화), utility(유틸강화), stat(스탯강화), ability(어빌리티강화)
/// - 사이클: 1/2 -> gauge+utility+stat, 3 -> ability
/// - 악마/무브 관련 기능 삭제
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    [Header("UI 슬롯 (프리팹 부모 오브젝트)")]
    [SerializeField] private GameObject slot1Prefab;
    [SerializeField] private GameObject slot2Prefab;
    [SerializeField] private GameObject slot3Prefab;
    [SerializeField] private GameObject slot4Prefab;

    [Header("플레이어 참조")]
    [SerializeField] private PlayerStatsManager playerStatsManager;
    [SerializeField] private PlayerElementsManager playerElementsManager;

    // ▼ CSV에서 가져온 EffectData를 슬롯별 랜덤값과 매핑(빠른 접근 테이블)
    private readonly Dictionary<EffectDatabase.EffectData, float> slotValues = new();

    // ▼ 카테고리 풀 (CSV type 값으로 분류)
    private List<EffectDatabase.EffectData> gaugePool = new();    // type == "게이지강화"
    private List<EffectDatabase.EffectData> utilityPool = new();  // type == "유틸강화"
    private List<EffectDatabase.EffectData> statPool = new();     // type == "스탯강화"
    private List<EffectDatabase.EffectData> abilityPool = new();  // type == "어빌리티강화"

    public static event Action OnAugmentSelected;

    [Header("사이클 진행도 (1~3)")]
    [SerializeField] private int cycleStep = 1;   // 1~3
    [SerializeField] private int cycleRound = 0;  // 완료된 라운드 수

    private bool poolsBuilt = false;

    private void OnEnable()
    {
        // EffectDatabase 로드 타이밍을 고려해 초기화
        StartCoroutine(InitAndPopulate());
    }

    private IEnumerator InitAndPopulate()
    {
        // EffectDatabase가 아직이면 기다림
        while (EffectDatabase.Instance == null || !EffectDatabase.Instance.isLoaded)
            yield return null;

        if (!poolsBuilt)
        {
            BuildPoolsFromDatabase();
            poolsBuilt = true;
        }

        PopulateSlots();
    }

    /// <summary>
    /// CSV에서 가져온 allEffects를 type 기준으로 각 풀에 분류
    /// </summary>
    private void BuildPoolsFromDatabase()
    {
        gaugePool.Clear();
        utilityPool.Clear();
        statPool.Clear();
        abilityPool.Clear();

        var all = EffectDatabase.Instance.allEffects;
        foreach (var e in all)
        {
            switch (e.type)
            {
                case "게이지강화":
                    gaugePool.Add(e);
                    break;
                case "유틸강화":
                    utilityPool.Add(e);
                    break;
                case "스탯강화":
                    statPool.Add(e);
                    break;
                case "어빌리티강화":
                    abilityPool.Add(e);
                    break;
                default:
                    // 필요시: 기타 타입 로깅
                    // Debug.Log($"[UpgradeManager] 알려지지 않은 type: {e.type} ({e.name})");
                    break;
            }
        }

        Debug.Log($"[UpgradeManager] 풀 구성 완료 → " +
                  $"게이지:{gaugePool.Count}, 유틸:{utilityPool.Count}, 스탯:{statPool.Count}, 어빌리티:{abilityPool.Count}");
    }

    private void PopulateSlots()
    {
        slotValues.Clear();

        // 0단계: 모든 속성이 레벨 0이면 1~4 전부 속성 슬롯 유지(기존 로직)
        if (AreAllAttributesZero())
        {
            AssignAttributeSlot(slot1Prefab, new List<AttributeType> { AttributeType.Fire });
            AssignAttributeSlot(slot2Prefab, new List<AttributeType> { AttributeType.Water });
            AssignAttributeSlot(slot3Prefab, new List<AttributeType> { AttributeType.Lightning });
            AssignAttributeSlot(slot4Prefab, new List<AttributeType> { AttributeType.Wind });
            return;
        }

        // 현재 사이클 풀 구성
        List<EffectDatabase.EffectData> poolForCycle = BuildPoolForCurrentCycle();

        // 1번 슬롯: 속성 우선 (가능하면)
        var validAttributes = GetValidAttributes();
        if (validAttributes.Count > 0)
            AssignAttributeSlot(slot1Prefab, validAttributes);
        else
            AssignSlot(slot1Prefab, poolForCycle, removeFromPool: true);

        // 2, 3번 슬롯 (중복 방지)
        var workingPool23 = new List<EffectDatabase.EffectData>(poolForCycle);
        AssignSlot(slot2Prefab, workingPool23, removeFromPool: true);
        AssignSlot(slot3Prefab, workingPool23, removeFromPool: true);

        // 4번 슬롯
        AssignSlot(slot4Prefab, poolForCycle, removeFromPool: true);

        // 사이클 진행 (1→2→3→1…)
        AdvanceCycle();
    }

    // =============================
    // 사이클 로직 (1/2: 게이지+유틸+스탯, 3: 어빌리티)
    // =============================
    private List<EffectDatabase.EffectData> BuildPoolForCurrentCycle()
    {
        switch (cycleStep)
        {
            case 1:
            case 2:
                return BuildPool(gaugePool, utilityPool, statPool);
            case 3:
                return BuildPool(abilityPool);
            default:
                return new List<EffectDatabase.EffectData>();
        }
    }

    private void AdvanceCycle()
    {
        if (cycleStep < 3)
            cycleStep++;
        else
        {
            cycleStep = 1;
            cycleRound++;
        }
    }

    private List<EffectDatabase.EffectData> BuildPool(params List<EffectDatabase.EffectData>[] pools)
    {
        var list = new List<EffectDatabase.EffectData>();
        foreach (var p in pools)
        {
            if (p != null && p.Count > 0)
                list.AddRange(p);
        }
        return list;
    }

    // =============================
    // 속성 관련 (기존 유지)
    // =============================
    private bool AreAllAttributesZero()
    {
        foreach (AttributeType attr in Enum.GetValues(typeof(AttributeType)))
        {
            if (playerElementsManager.GetAttributeLevel(attr) > 0)
                return false;
        }
        return true;
    }

    private List<AttributeType> GetValidAttributes()
    {
        var valid = new List<AttributeType>();
        foreach (AttributeType attr in Enum.GetValues(typeof(AttributeType)))
        {
            int level = playerElementsManager.GetAttributeLevel(attr);
            bool isFusion = playerElementsManager.IsFusionAttribute(attr);
            if (level < 5 && !isFusion)
                valid.Add(attr);
        }
        return valid;
    }

    private void AssignAttributeSlot(GameObject slotPrefab, List<AttributeType> validAttrs)
    {
        if (slotPrefab == null || validAttrs.Count == 0) return;

        AttributeType chosen = validAttrs[UnityEngine.Random.Range(0, validAttrs.Count)];

        var button = slotPrefab.GetComponentInChildren<Button>();
        var desc = slotPrefab.transform.Find("UpgradeDescription")?.GetComponent<TMP_Text>();
        var bg = slotPrefab.transform.Find("BackGround")?.GetComponent<Image>();

        if (bg != null)
        {
            switch (chosen)
            {
                case AttributeType.Fire: bg.color = Color.red; break;
                case AttributeType.Water: bg.color = Color.blue; break;
                case AttributeType.Lightning: bg.color = Color.yellow; break;
                case AttributeType.Wind: bg.color = Color.gray; break;
            }
        }

        if (desc != null)
        {
            string name = chosen switch
            {
                AttributeType.Fire => "불",
                AttributeType.Water => "물",
                AttributeType.Lightning => "번개",
                AttributeType.Wind => "바람",
                _ => "속성"
            };
            desc.text = $"{name} 증강 레벨 업";
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                playerElementsManager.IncreaseAttribute(chosen, 1);
                Debug.Log($"{chosen} 속성 레벨 업!");
                OnAugmentSelected?.Invoke();
            });
        }
    }

    // =============================
    // 일반 슬롯 (EffectData 기반)
    // =============================
    private void AssignSlot(GameObject slotPrefab, List<EffectDatabase.EffectData> pool, bool removeFromPool)
    {
        if (slotPrefab == null || pool == null || pool.Count == 0) return;

        int index = UnityEngine.Random.Range(0, pool.Count);
        var choice = pool[index];
        if (removeFromPool) pool.RemoveAt(index);

        float value = UnityEngine.Random.Range(choice.minValue, choice.maxValue);
        slotValues[choice] = value;

        var button = slotPrefab.GetComponentInChildren<Button>();
        var desc = slotPrefab.transform.Find("UpgradeDescription")?.GetComponent<TMP_Text>();
        var bg = slotPrefab.transform.Find("BackGround")?.GetComponent<Image>();

        if (desc != null)
        {
            // title / info / 단위 표시 (CSV 기준)
            string unit = string.IsNullOrEmpty(choice.unit) ? "" : choice.unit;
            desc.text = $"{choice.title}\n{choice.info}  +{value:F1}{unit}";
        }

        if (bg != null)
        {
            // 간단한 색상 (카테고리별로 다르게 주고 싶으면 여기서 분기)
            bg.color = new Color(0.3f, 0.6f, 1f);
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ApplyStat(choice));
        }
    }

    /// <summary>
    /// 선택한 EffectData의 랜덤 값(value)을 실게임 스탯에 적용
    /// </summary>
    private void ApplyStat(EffectDatabase.EffectData effect)
    {
        if (slotValues.TryGetValue(effect, out float value))
        {
            // ▼ 프로젝트 규칙에 맞게 적용:
            // 1) StatType enum 사용 시: effect.type를 enum으로 파싱하여 AddStat(enum, value)
            // 2) 문자열 키 사용 시: AddStat(string key, float value) 형태로 호출
            // 아래는 1) 예시. 실패 시 문자열 버전으로 fallback.

            if (TryToStatType(effect.name, out StatType statType))
            {
                playerStatsManager.AddStat(statType, value); // TODO: 네 PlayerStatsManager 시그니처 확인
            }
            else
            {
                //todo StatType과 매핑이되야함
                Debug.LogWarning($"[UpgradeManager] StatType 변환 실패(type={effect.type}), 적용은 스킵됩니다. " +
                                 $"PlayerStatsManager에 문자열 기반 API가 있다면 연결하세요.");
            }

            Debug.Log($"선택한 효과: {effect.name}  +{value:F1}{effect.unit}");
        }

        OnAugmentSelected?.Invoke();
    }

    // StatType 매핑 도우미 (enum 이름과 CSV type이 같은 경우)
    private bool TryToStatType(string type, out StatType stat)
    {
        return Enum.TryParse(type, ignoreCase: true, out stat);
    }
}
