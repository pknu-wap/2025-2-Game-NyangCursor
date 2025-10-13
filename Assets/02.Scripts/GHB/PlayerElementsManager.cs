using System;
using System.Collections.Generic;
using UnityEngine;

public enum AttributeType { Fire, Water, Wind, Lightning }

[Serializable]
public class AttributeStatus
{
    public int Level = 0;
    [HideInInspector] public bool UsedInFusion = false; // 주 증강으로 사용되었는지
}

// 단일효과
[Serializable]
public class AttributeEffectSet
{
    public AttributeType type;
    public GameObject Level1to5Effect;
}

// 융합효과
[Serializable]
public class FusionEffectSet
{
    public AttributeType main;
    public AttributeType sub;
    public GameObject fusionEffect;
}

public class PlayerElementsManager : MonoBehaviour
{
    private Dictionary<AttributeType, AttributeStatus> Attributes = new();
    private Queue<AttributeType> Level5Order = new();

    [Header("단일 속성 효과 (1~5)")]
    [SerializeField] private List<AttributeEffectSet> SingleEffects = new();

    [Header("융합 속성 효과 (주+보조)")]
    [SerializeField] private List<FusionEffectSet> FusionEffectsList = new();

    private Dictionary<AttributeType, GameObject> SingleEffectMap;
    private Dictionary<(AttributeType, AttributeType), GameObject> FusionEffectMap;

    private void Awake()
    {
        // Attribute 초기화
        foreach (AttributeType t in Enum.GetValues(typeof(AttributeType)))
            Attributes[t] = new AttributeStatus();

        // 매핑 구조 생성
        SingleEffectMap = new Dictionary<AttributeType, GameObject>();
        foreach (var set in SingleEffects)
            SingleEffectMap[set.type] = set.Level1to5Effect;

        FusionEffectMap = new Dictionary<(AttributeType, AttributeType), GameObject>();
        foreach (var set in FusionEffectsList)
            FusionEffectMap[(set.main, set.sub)] = set.fusionEffect;

        UpdateAllEffects();
    }

    public void IncreaseAttribute(AttributeType type, int amount = 1)
    {
        var attr = Attributes[type];
        bool wasBelow5 = attr.Level < 5;

        attr.Level += amount;
        Debug.Log($"{type} 속성 레벨 업! 새 레벨: {attr.Level}");

        // 5레벨 달성 순서 기록
        if (wasBelow5 && attr.Level >= 5 && !Level5Order.Contains(type))
        {
            Level5Order.Enqueue(type);
            Debug.Log($"{type}이(가) 5레벨이 되어 Queue에 추가됨.");
        }

        UpdateAllEffects();
    }

    private void UpdateAllEffects()
    {
        // 🔹 융합 처리 (큐 길이가 2일 때만 발동)
        if (Level5Order.Count == 2)
        {
            AttributeType[] arr = Level5Order.ToArray();
            AttributeType main = arr[0];
            AttributeType sub = arr[1];

            // 주 증강의 단일 이펙트 끄기
            if (SingleEffectMap.TryGetValue(main, out GameObject mainEffect))
                mainEffect.SetActive(false);

            // 융합 이펙트 켜기
            ActivateFusion(main, sub);

            // 주 증강 UsedInFusion 기록
            Attributes[main].UsedInFusion = true;

            // 큐 정리 (가장 오래된 속성 제거)
            Level5Order.Dequeue();
        }

        // 🔹 단일 속성 처리
        foreach (var kvp in Attributes)
        {
            if (kvp.Value.Level > 0)
            {
                // 이미 융합의 주 증강으로 쓰인 애는 스킵
                if (kvp.Value.UsedInFusion)
                    continue;
                SingleEffectMap[kvp.Key].SetActive(true);
            }
        }

        Debug.Log("=== 융합 상태 업데이트 완료 ===");
        Debug.Log("Queue 순서: " + string.Join(" -> ", Level5Order));
    }

    private void ActivateFusion(AttributeType main, AttributeType sub)
    {
        if (FusionEffectMap.TryGetValue((main, sub), out GameObject fusion))
        {
            fusion.SetActive(true);
            Debug.Log($"융합 활성화: {main}+{sub}");
        }
    }

    public int GetAttributeLevel(AttributeType type) => Attributes[type].Level;

    public bool IsFusionAttribute(AttributeType type) => Attributes[type].UsedInFusion;

    // 디버그용
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("===== 현재 속성 상태 =====");
            foreach (var kvp in Attributes)
            {
                string status = $"{kvp.Key}: 레벨 {kvp.Value.Level}, " +
                                $"융합 참여={kvp.Value.UsedInFusion}";
                Debug.Log(status);
            }

            Debug.Log("===== 활성 융합 효과 =====");
            foreach (var fusion in FusionEffectMap)
            {
                if (fusion.Value.activeSelf)
                {
                    Debug.Log($"융합: {fusion.Key.Item1}+{fusion.Key.Item2} 활성");
                }
            }

            Debug.Log("===== Level5Queue 상태 =====");
            Debug.Log("Queue 순서: " + string.Join(" -> ", Level5Order));
        }
    }


}
