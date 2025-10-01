using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class UpgradeManager : MonoBehaviour
{
    [Header("UI 슬롯 (프리팹 부모 오브젝트)")]
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
    [SerializeField] private TempPlayer player;

    private Dictionary<UpgradeScriptableObjects, float> slotValues = new Dictionary<UpgradeScriptableObjects, float>();
    private HashSet<UpgradeScriptableObjects> obtainedDemons = new HashSet<UpgradeScriptableObjects>();

    public static event Action OnAugmentSelected;
    void OnEnable()
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

        AssignSlot(slot2Prefab, pool23);
        AssignSlot(slot3Prefab, pool23);

        // =============================
        // 슬롯 4: demonProbability 적용
        // =============================
        AssignSlotForSlot4(slot4Prefab);
    }

    private void AssignSlot(GameObject slotPrefab, List<UpgradeScriptableObjects> pool)
    {
        UpgradeScriptableObjects choice = pool[UnityEngine.Random.Range(0, pool.Count)];
        float value = UnityEngine.Random.Range(choice.minvalue, choice.maxvalue);
        slotValues[choice] = value;

        // 하위 오브젝트 접근
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

            if (availableDemons.Count > 0)
            {
                choice = availableDemons[UnityEngine.Random.Range(0, availableDemons.Count)];
            }
            else
            {
                // 획득한 악마 없으면 일반 풀에서 선택
                choice = GetRandomNormalOption();
            }
        }
        else
        {
            // 일반 풀에서 선택
            choice = GetRandomNormalOption();
        }

        float value = UnityEngine.Random.Range(choice.minvalue, choice.maxvalue);
        slotValues[choice] = value;

        // 하위 오브젝트 접근
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
            player.AddStat(option.optionStatType, value);
            Debug.Log($"선택한 스탯: {option.optionStatType} +{value:F1}");

            // 획득한 악마 기록
            if (demonPool.Contains(option))
                obtainedDemons.Add(option);
        }
        OnAugmentSelected?.Invoke();
    }
}
