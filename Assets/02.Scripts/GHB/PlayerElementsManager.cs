using System;
using System.Collections.Generic;
using UnityEngine;

public enum AttributeType { Fire, Water, Lightning, Wind }

[Serializable]
public class AttributeStatus
{
    public int Level = 0;
    [HideInInspector] public bool Level1to4Active = false;
    [HideInInspector] public bool Level5PlusActive = false;
}

public class PlayerElementsManager : MonoBehaviour
{
    private Dictionary<AttributeType, AttributeStatus> Attributes = new Dictionary<AttributeType, AttributeStatus>();

    // 최초 융합 결정 속성 2개
    private AttributeType? fusionA = null;
    private AttributeType? fusionB = null;

    [Header("속성 효과 오브젝트")]
    [SerializeField] private GameObject Fire_Level1to4;
    [SerializeField] private GameObject Fire_Level5Plus;
    [SerializeField] private GameObject Water_Level1to4;
    [SerializeField] private GameObject Water_Level5Plus;
    [SerializeField] private GameObject Lightning_Level1to4;
    [SerializeField] private GameObject Lightning_Level5Plus;
    [SerializeField] private GameObject Wind_Level1to4;
    [SerializeField] private GameObject Wind_Level5Plus;

    [Header("융합 효과 오브젝트")]
    [SerializeField] private GameObject FireWater_Fusion;
    [SerializeField] private GameObject FireLightning_Fusion;
    [SerializeField] private GameObject FireWind_Fusion;
    [SerializeField] private GameObject WaterLightning_Fusion;
    [SerializeField] private GameObject WaterWind_Fusion;
    [SerializeField] private GameObject LightningWind_Fusion;

    private void Awake()
    {
        foreach (AttributeType type in Enum.GetValues(typeof(AttributeType)))
            Attributes[type] = new AttributeStatus();

        UpdateAllEffects();
    }

    public void IncreaseAttribute(AttributeType type, int amount = 1)
    {
        var attr = Attributes[type];
        bool wasBelow5 = attr.Level < 5;

        attr.Level += amount;
        Debug.Log($"{type} 속성 레벨 업! 새 레벨: {attr.Level}");

        // Level1~4 활성화
        if (!attr.Level1to4Active && attr.Level > 0 && attr.Level < 5)
        {
            attr.Level1to4Active = true;
            Debug.Log($"{type} Level 1~4 효과 활성화");
        }

        // Level5+ 달성 시
        if (wasBelow5 && attr.Level >= 5)
        {
            attr.Level1to4Active = false;
            attr.Level5PlusActive = true;
            Debug.Log($"{type} Level5+ 효과 활성화");

            // 융합 A/B 결정 (둘 다 아직 널이면 채움)
            if (fusionA == null)
                fusionA = type;
            else if (fusionB == null)
                fusionB = type;
        }

        UpdateAllEffects();
    }

    private void UpdateAllEffects()
    {
        // 단일 속성 Level5+ 꺼야 하는지 결정
        bool fusionActive = fusionA.HasValue && fusionB.HasValue;

        // 단일 효과 업데이트
        foreach (var kvp in Attributes)
        {
            var type = kvp.Key;
            var status = kvp.Value;

            bool isFusionParticipant = fusionActive && (type == fusionA || type == fusionB);

            // Level1~4
            status.Level1to4Active = status.Level1to4Active && !isFusionParticipant;

            // Level5+
            status.Level5PlusActive = status.Level5PlusActive && !isFusionParticipant;
        }

        // GameObject에 적용
        SetEffect(Fire_Level1to4, Attributes[AttributeType.Fire].Level1to4Active);
        SetEffect(Water_Level1to4, Attributes[AttributeType.Water].Level1to4Active);
        SetEffect(Lightning_Level1to4, Attributes[AttributeType.Lightning].Level1to4Active);
        SetEffect(Wind_Level1to4, Attributes[AttributeType.Wind].Level1to4Active);

        SetEffect(Fire_Level5Plus, Attributes[AttributeType.Fire].Level5PlusActive);
        SetEffect(Water_Level5Plus, Attributes[AttributeType.Water].Level5PlusActive);
        SetEffect(Lightning_Level5Plus, Attributes[AttributeType.Lightning].Level5PlusActive);
        SetEffect(Wind_Level5Plus, Attributes[AttributeType.Wind].Level5PlusActive);

        // 융합 효과
        SetEffect(FireWater_Fusion, fusionA == AttributeType.Fire && fusionB == AttributeType.Water || fusionA == AttributeType.Water && fusionB == AttributeType.Fire);
        SetEffect(FireLightning_Fusion, fusionA == AttributeType.Fire && fusionB == AttributeType.Lightning || fusionA == AttributeType.Lightning && fusionB == AttributeType.Fire);
        SetEffect(FireWind_Fusion, fusionA == AttributeType.Fire && fusionB == AttributeType.Wind || fusionA == AttributeType.Wind && fusionB == AttributeType.Fire);
        SetEffect(WaterLightning_Fusion, fusionA == AttributeType.Water && fusionB == AttributeType.Lightning || fusionA == AttributeType.Lightning && fusionB == AttributeType.Water);
        SetEffect(WaterWind_Fusion, fusionA == AttributeType.Water && fusionB == AttributeType.Wind || fusionA == AttributeType.Wind && fusionB == AttributeType.Water);
        SetEffect(LightningWind_Fusion, fusionA == AttributeType.Lightning && fusionB == AttributeType.Wind || fusionA == AttributeType.Wind && fusionB == AttributeType.Lightning);

        // 디버그
        Debug.Log("===== 융합 상태 =====");
        Debug.Log($"FireWater_Fusion 활성화: {FireWater_Fusion != null && FireWater_Fusion.activeSelf}");
        Debug.Log($"FireLightning_Fusion 활성화: {FireLightning_Fusion != null && FireLightning_Fusion.activeSelf}");
        Debug.Log($"FireWind_Fusion 활성화: {FireWind_Fusion != null && FireWind_Fusion.activeSelf}");
        Debug.Log($"WaterLightning_Fusion 활성화: {WaterLightning_Fusion != null && WaterLightning_Fusion.activeSelf}");
        Debug.Log($"WaterWind_Fusion 활성화: {WaterWind_Fusion != null && WaterWind_Fusion.activeSelf}");
        Debug.Log($"LightningWind_Fusion 활성화: {LightningWind_Fusion != null && LightningWind_Fusion.activeSelf}");
    }

    private void SetEffect(GameObject obj, bool active)
    {
        if (obj != null && obj.activeSelf != active)
            obj.SetActive(active);
    }

    public int GetAttributeLevel(AttributeType type) => Attributes[type].Level;

    // 슬롯 1용: 융합 속성인지 체크
    public bool IsFusionAttribute(AttributeType type)
    {
        return (fusionA.HasValue && fusionA.Value == type) || (fusionB.HasValue && fusionB.Value == type);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("===== 현재 속성 상태 =====");
            foreach (var kvp in Attributes)
            {
                Debug.Log($"{kvp.Key}: 레벨 {kvp.Value.Level}, Level1~4Active={kvp.Value.Level1to4Active}, Level5PlusActive={kvp.Value.Level5PlusActive}");
            }

            Debug.Log("===== 융합 체크 =====");
            Debug.Log($"Fire+Water 융합: {FireWater_Fusion != null && FireWater_Fusion.activeSelf}");
            Debug.Log($"Fire+Lightning 융합: {FireLightning_Fusion != null && FireLightning_Fusion.activeSelf}");
            Debug.Log($"Fire+Wind 융합: {FireWind_Fusion != null && FireWind_Fusion.activeSelf}");
            Debug.Log($"Water+Lightning 융합: {WaterLightning_Fusion != null && WaterLightning_Fusion.activeSelf}");
            Debug.Log($"Water+Wind 융합: {WaterWind_Fusion != null && WaterWind_Fusion.activeSelf}");
            Debug.Log($"Lightning+Wind 융합: {LightningWind_Fusion != null && LightningWind_Fusion.activeSelf}");
        }
    }
}
