using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class UpgradeManager : MonoBehaviour
{
    [Header("UI 슬롯 (프리팹 부모 오브젝트)")]
    [SerializeField] private GameObject slot1Prefab; // 속성 전용 슬롯
    [SerializeField] private GameObject slot2Prefab;
    [SerializeField] private GameObject slot3Prefab;
    [SerializeField] private GameObject slot4Prefab;

    [Header("스탯 풀")]
    [SerializeField] private List<UpgradeScriptableObjects> attackPool = new List<UpgradeScriptableObjects>();
    [SerializeField] private List<UpgradeScriptableObjects> utilityPool = new List<UpgradeScriptableObjects>();
    [SerializeField] private List<UpgradeScriptableObjects> elementPool = new List<UpgradeScriptableObjects>();
    [SerializeField] private List<UpgradeScriptableObjects> demonPool = new List<UpgradeScriptableObjects>();

    [Header("악마 증강 확률")]
    [Range(0f, 1f)]
    [SerializeField] private float demonProbability = 0.1f; // 10% 확률

    [Header("플레이어 참조")]
    [SerializeField] private PlayerStatsManager playerStatsManager; // 스탯 관리 매니저 참조
    [SerializeField] private PlayerElementsManager playerElementsManager; // 속성 관리 매니저 참조

    private Dictionary<UpgradeScriptableObjects, float> slotValues = new Dictionary<UpgradeScriptableObjects, float>();
    private HashSet<UpgradeScriptableObjects> obtainedDemons = new HashSet<UpgradeScriptableObjects>();

    public static event Action OnAugmentSelected;

    private void OnEnable()
    {
        PopulateSlots();
    }

    private void PopulateSlots()
    {
        slotValues.Clear();

        // =============================
        // 슬롯 2,3 풀
        // =============================
        List<UpgradeScriptableObjects> pool23 = new List<UpgradeScriptableObjects>();
        pool23.AddRange(attackPool);
        pool23.AddRange(utilityPool);
        pool23.AddRange(elementPool);

        // =============================
        // 1번 슬롯: 속성 전용 가능 여부 체크
        // =============================
        List<AttributeType> validAttributes = GetValidAttributes();

        // 속성 선택 가능 여부에 따라 슬롯 1 분기
        if (validAttributes.Count > 0)
            AssignSlot1(validAttributes);          // 속성 전용 슬롯
        else
            AssignSlot(slot1Prefab, pool23);      // 일반 풀로 처리 (5/5/5/5 상황 대비)

        // =============================
        // 슬롯 2,3 일반 풀
        // =============================
        AssignSlot(slot2Prefab, pool23);
        AssignSlot(slot3Prefab, pool23);

        // =============================
        // 슬롯 4: demon 확률 적용
        // =============================
        AssignSlotForSlot4(slot4Prefab);
    }

    // =============================
    // 유효 속성 체크 함수 (레벨 < 5 && 융합 아님)
    // =============================
    private List<AttributeType> GetValidAttributes()
    {
        List<AttributeType> validAttributes = new List<AttributeType>();
        foreach (AttributeType attr in Enum.GetValues(typeof(AttributeType)))
        {
            int level = playerElementsManager.GetAttributeLevel(attr);
            bool isFusion = playerElementsManager.IsFusionAttribute(attr);
            if (level < 5 && !isFusion)
                validAttributes.Add(attr);
        }
        return validAttributes;
    }

    // =============================
    // 슬롯 1: 속성 전용
    // =============================
    private void AssignSlot1(List<AttributeType> validAttributes)
    {
        if (slot1Prefab == null || validAttributes.Count == 0)
            return;

        // 랜덤 속성 선택
        AttributeType chosenAttr = validAttributes[UnityEngine.Random.Range(0, validAttributes.Count)];

        // =============================
        // UI 요소 접근
        // =============================
        Button button = slot1Prefab.GetComponentInChildren<Button>();
        TMP_Text descriptionText = slot1Prefab.transform.Find("UpgradeDescription")?.GetComponent<TMP_Text>();
        Image backgroundImage = slot1Prefab.transform.Find("BackGround")?.GetComponent<Image>();

        // 색상 적용 (유니티 기본 색상)
        if (backgroundImage != null)
        {
            switch (chosenAttr)
            {
                case AttributeType.Fire: backgroundImage.color = Color.red; break;
                case AttributeType.Water: backgroundImage.color = Color.blue; break;
                case AttributeType.Lightning: backgroundImage.color = Color.yellow; break;
                case AttributeType.Wind: backgroundImage.color = Color.gray; break;
            }
        }

        // 텍스트 적용 (한글)
        if (descriptionText != null)
        {
            string attrName = chosenAttr switch
            {
                AttributeType.Fire => "불",
                AttributeType.Water => "물",
                AttributeType.Lightning => "번개",
                AttributeType.Wind => "바람",
                _ => "속성"
            };

            descriptionText.text = $"{attrName} 증강 레벨 업";
        }

        // 버튼 클릭 시 레벨 업
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                playerElementsManager.IncreaseAttribute(chosenAttr, 1);
                Debug.Log($"{chosenAttr} 속성 레벨 업!");
                OnAugmentSelected?.Invoke();
            });
        }
    }

    // =============================
    // 슬롯 2,3 일반 풀
    // =============================
    private void AssignSlot(GameObject slotPrefab, List<UpgradeScriptableObjects> pool)
    {
        UpgradeScriptableObjects choice = pool[UnityEngine.Random.Range(0, pool.Count)];
        float value = UnityEngine.Random.Range(choice.minvalue, choice.maxvalue);
        slotValues[choice] = value;

        // =============================
        // UI 요소 접근
        // =============================
        Button button = slotPrefab.GetComponentInChildren<Button>();
        TMP_Text descriptionText = slotPrefab.transform.Find("UpgradeDescription")?.GetComponent<TMP_Text>();
        Image backgroundImage = slotPrefab.transform.Find("BackGround")?.GetComponent<Image>();

        if (descriptionText != null)
            descriptionText.text = $"{choice.optionDescription} +{value:F1}";

        if (backgroundImage != null)
            backgroundImage.color = demonPool.Contains(choice) ? new Color(0.6f, 0.2f, 0.8f) : new Color(0.3f, 0.6f, 1f);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ApplyStat(choice));
        }
    }

    // =============================
    // 슬롯 4: demon 확률 적용
    // =============================
    private void AssignSlotForSlot4(GameObject slotPrefab)
    {
        UpgradeScriptableObjects choice;

        // 악마 증강 등장 여부 판단
        if (demonPool.Count > 0 && UnityEngine.Random.value < demonProbability)
        {
            List<UpgradeScriptableObjects> availableDemons = new List<UpgradeScriptableObjects>();
            foreach (var demon in demonPool)
            {
                if (!obtainedDemons.Contains(demon))
                    availableDemons.Add(demon);
            }

            choice = availableDemons.Count > 0 ?
                     availableDemons[UnityEngine.Random.Range(0, availableDemons.Count)] :
                     GetRandomNormalOption();
        }
        else
        {
            // 일반 풀에서 선택
            choice = GetRandomNormalOption();
        }

        float value = UnityEngine.Random.Range(choice.minvalue, choice.maxvalue);
        slotValues[choice] = value;

        // =============================
        // UI 요소 접근
        // =============================
        Button button = slotPrefab.GetComponentInChildren<Button>();
        TMP_Text descriptionText = slotPrefab.transform.Find("UpgradeDescription")?.GetComponent<TMP_Text>();
        Image backgroundImage = slotPrefab.transform.Find("BackGround")?.GetComponent<Image>();

        if (descriptionText != null)
            descriptionText.text = $"{choice.optionDescription} +{value:F1}";

        if (backgroundImage != null)
            backgroundImage.color = demonPool.Contains(choice) ? new Color(0.6f, 0.2f, 0.8f) : new Color(0.3f, 0.6f, 1f);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ApplyStat(choice));
        }
    }

    private UpgradeScriptableObjects GetRandomNormalOption()
    {
        List<UpgradeScriptableObjects> normalPool = new List<UpgradeScriptableObjects>();
        normalPool.AddRange(attackPool);
        normalPool.AddRange(utilityPool);
        normalPool.AddRange(elementPool);
        return normalPool[UnityEngine.Random.Range(0, normalPool.Count)];
    }

    private void ApplyStat(UpgradeScriptableObjects option)
    {
        if (slotValues.TryGetValue(option, out float value))
        {
            playerStatsManager.AddStat(option.optionStatType, value);
            Debug.Log($"선택한 스탯: {option.optionStatType} +{value:F1}");

            // 획득한 악마 기록
            if (demonPool.Contains(option))
                obtainedDemons.Add(option);
        }
        OnAugmentSelected?.Invoke();
    }
}
