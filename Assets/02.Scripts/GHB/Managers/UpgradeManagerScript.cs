using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class UpgradeManager : MonoBehaviour
{
    [Header("UI 슬롯 (프리팹 부모 오브젝트)")]
    [SerializeField] private GameObject slot1Prefab;
    [SerializeField] private GameObject slot2Prefab;
    [SerializeField] private GameObject slot3Prefab;
    [SerializeField] private GameObject slot4Prefab;

    [Header("스탯 풀")]
    [SerializeField] private List<UpgradeScriptableObjects> resourceStatPool = new List<UpgradeScriptableObjects>();
    [SerializeField] private List<UpgradeScriptableObjects> economyNGrowthStatPool = new List<UpgradeScriptableObjects>();
    [SerializeField] private List<UpgradeScriptableObjects> movementStatPool = new List<UpgradeScriptableObjects>();
    [SerializeField] private List<UpgradeScriptableObjects> generalElementStatPool = new List<UpgradeScriptableObjects>(); // 속성강화
    [SerializeField] private List<UpgradeScriptableObjects> specificElementStatPool = new List<UpgradeScriptableObjects>(); // 속성강화
    [SerializeField] private List<UpgradeScriptableObjects> demonPool = new List<UpgradeScriptableObjects>();

    [Header("플레이어 참조")]
    [SerializeField] private PlayerStatsManager playerStatsManager;
    [SerializeField] private PlayerElementsManager playerElementsManager;

    private Dictionary<UpgradeScriptableObjects, float> slotValues = new Dictionary<UpgradeScriptableObjects, float>();
    private HashSet<UpgradeScriptableObjects> obtainedDemons = new HashSet<UpgradeScriptableObjects>();

    public static event Action OnAugmentSelected;

    [Header("사이클 진행도")]
    [SerializeField] private int cycleStep = 1;   // 1~5
    [SerializeField] private int cycleRound = 0;  // 완료된 라운드 수

    private void OnEnable()
    {
        PopulateSlots();
    }

    private void PopulateSlots()
    {
        slotValues.Clear();

        // ✅ 0단계: 모든 속성이 레벨 0이면 1~4 전부 속성 슬롯
        if (AreAllAttributesZero())
        {
            // 고정된 속성 순서로 각 슬롯에 배정
            AssignAttributeSlot(slot1Prefab, new List<AttributeType> { AttributeType.Fire });
            AssignAttributeSlot(slot2Prefab, new List<AttributeType> { AttributeType.Water });
            AssignAttributeSlot(slot3Prefab, new List<AttributeType> { AttributeType.Lightning });
            AssignAttributeSlot(slot4Prefab, new List<AttributeType> { AttributeType.Wind });
            return;
        }

        // ✅ 현재 사이클 풀 계산
        List<UpgradeScriptableObjects> pool234 = BuildPoolForCurrentCycle();
        List<AttributeType> validAttributes = GetValidAttributes();

        // ✅ 1번 슬롯
        if (validAttributes.Count > 0)
        {
            // 속성 가능 → 속성전용
            AssignAttributeSlot(slot1Prefab, validAttributes);
        }
        else
        {
            // 모든 속성이 만렙 → 현재 사이클에 맞는 풀에서 선택
            AssignSlot(slot1Prefab, pool234, true);
        }

        // ✅ 2,3번 슬롯 (중복 방지)
        List<UpgradeScriptableObjects> workingPool23 = new List<UpgradeScriptableObjects>(pool234);
        AssignSlot(slot2Prefab, workingPool23, true);
        AssignSlot(slot3Prefab, workingPool23, true);

        // ✅ 4번 슬롯
        if (cycleStep == 5)
            AssignDemonSlot(slot4Prefab);
        else
            AssignSlot(slot4Prefab, pool234, true);

        // ✅ 사이클 진행
        AdvanceCycle();
    }

    private GameObject GetSlotByIndex(int index)
    {
        switch (index)
        {
            case 0: return slot1Prefab;
            case 1: return slot2Prefab;
            case 2: return slot3Prefab;
            case 3: return slot4Prefab;
            default: return null;
        }
    }

    // =============================
    // 사이클 로직
    // =============================
    private List<UpgradeScriptableObjects> BuildPoolForCurrentCycle()
    {
        //1,3 케이스, 2,4케이스 묶은것
        switch (cycleStep)
        {
            case 1:
            case 3:
                return BuildPool(resourceStatPool, economyNGrowthStatPool, movementStatPool); // R/E/M
            case 2:
            case 4:
                return BuildPool(generalElementStatPool, specificElementStatPool); // G/S
            case 5:
                return BuildPool(
                    resourceStatPool, economyNGrowthStatPool, movementStatPool,
                    generalElementStatPool, specificElementStatPool
                ); // R/E/M/G/S
            default:
                return new List<UpgradeScriptableObjects>();
        }
    }

    private void AdvanceCycle()
    {
        if (cycleStep < 5)
            cycleStep++;
        else
        {
            cycleStep = 1;
            cycleRound++;
        }
    }

    private List<UpgradeScriptableObjects> BuildPool(params List<UpgradeScriptableObjects>[] pools)
    {
        List<UpgradeScriptableObjects> list = new List<UpgradeScriptableObjects>();
        foreach (var p in pools)
        {
            if (p != null && p.Count > 0)
                list.AddRange(p);
        }
        return list;
    }

    // =============================
    // 속성 관련 함수
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

    //지금은 안쓰는데 playerElementsManager 변경으로 필요할수도 있어서 놔둠
    private bool AreAllAttributesMax()
    {
        foreach (AttributeType attr in Enum.GetValues(typeof(AttributeType)))
        {
            if (playerElementsManager.GetAttributeLevel(attr) < 5)
                return false;
        }
        return true;
    }

    private List<AttributeType> GetValidAttributes()
    {
        List<AttributeType> valid = new List<AttributeType>();
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

        Button button = slotPrefab.GetComponentInChildren<Button>();
        TMP_Text desc = slotPrefab.transform.Find("UpgradeDescription")?.GetComponent<TMP_Text>();
        Image bg = slotPrefab.transform.Find("BackGround")?.GetComponent<Image>();

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
    // 일반 슬롯 (중복 방지 지원)
    // =============================
    private void AssignSlot(GameObject slotPrefab, List<UpgradeScriptableObjects> pool, bool removeFromPool)
    {
        if (slotPrefab == null || pool == null || pool.Count == 0) return;

        int index = UnityEngine.Random.Range(0, pool.Count);
        UpgradeScriptableObjects choice = pool[index];
        if (removeFromPool) pool.RemoveAt(index);

        float value = UnityEngine.Random.Range(choice.minvalue, choice.maxvalue);
        slotValues[choice] = value;

        Button button = slotPrefab.GetComponentInChildren<Button>();
        TMP_Text desc = slotPrefab.transform.Find("UpgradeDescription")?.GetComponent<TMP_Text>();
        Image bg = slotPrefab.transform.Find("BackGround")?.GetComponent<Image>();

        if (desc != null)
            desc.text = $"{choice.optionDescription} +{value:F1}";

        if (bg != null)
            bg.color = demonPool.Contains(choice)
                ? new Color(0.6f, 0.2f, 0.8f)
                : new Color(0.3f, 0.6f, 1f);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ApplyStat(choice));
        }
    }

    // =============================
    // Demon 슬롯 (5사이클의 4번)
    // =============================
    private void AssignDemonSlot(GameObject slotPrefab)
    {
        if (slotPrefab == null) return;

        UpgradeScriptableObjects choice = null;
        List<UpgradeScriptableObjects> availableDemons = new List<UpgradeScriptableObjects>();

        foreach (var d in demonPool)
            if (!obtainedDemons.Contains(d)) availableDemons.Add(d);

        if (availableDemons.Count > 0)
            choice = availableDemons[UnityEngine.Random.Range(0, availableDemons.Count)];
        else
        {
            var fallback = BuildPool(resourceStatPool, economyNGrowthStatPool, movementStatPool,
                                     generalElementStatPool, specificElementStatPool);
            if (fallback.Count == 0) return;
            choice = fallback[UnityEngine.Random.Range(0, fallback.Count)];
        }

        float value = UnityEngine.Random.Range(choice.minvalue, choice.maxvalue);
        slotValues[choice] = value;

        Button button = slotPrefab.GetComponentInChildren<Button>();
        TMP_Text desc = slotPrefab.transform.Find("UpgradeDescription")?.GetComponent<TMP_Text>();
        Image bg = slotPrefab.transform.Find("BackGround")?.GetComponent<Image>();

        if (desc != null)
            desc.text = $"{choice.optionDescription} +{value:F1}";
        if (bg != null)
            bg.color = new Color(0.6f, 0.2f, 0.8f);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ApplyStat(choice));
        }
    }

    private void ApplyStat(UpgradeScriptableObjects option)
    {
        if (slotValues.TryGetValue(option, out float value))
        {
            playerStatsManager.AddStat(option.optionStatType, value);
            Debug.Log($"선택한 스탯: {option.optionStatType} +{value:F1}");

            if (demonPool.Contains(option))
                obtainedDemons.Add(option);
        }
        OnAugmentSelected?.Invoke();
    }
}
