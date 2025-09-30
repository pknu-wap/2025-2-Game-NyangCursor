using UnityEngine;
using System.Collections.Generic;

public class TempPlayer : MonoBehaviour
{
    // 스탯 종류 정의 (추가되면 여기만 수정하면 됨)
    public enum StatType
    {
        Health,
        Attack
    }

    [System.Serializable]
    public class StatEntry
    {
        public StatType statType;
        public float value;
    }

    [Header("기본 스탯")]
    [Tooltip("플레이어의 기본 스탯 능력치를 작성하세요")]
    public List<StatEntry> BaseStats = new List<StatEntry>();

    [Header("상점 스탯")]
    [Tooltip("상점 데이터 ScriptableObject들을 끌어 두세요")]
    public List<ShopStatData> shopStats = new List<ShopStatData>();

    private Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>();
    private Dictionary<StatType, float> currentStats = new Dictionary<StatType, float>();

    void Start()
    {
        InitializeBaseStats();
        InitializeCurrentStats();
    }

    /// 인스펙터에서 입력한 기본 스탯을 Dictionary로 초기화
    private void InitializeBaseStats()
    {
        baseStats.Clear();
        foreach (var stat in BaseStats)
        {
            baseStats[stat.statType] = stat.value;
        }
    }

    /// PlayerPrefs 값과 합산해서 currentStats 초기화
    private void InitializeCurrentStats()
    {
        currentStats.Clear();

        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            float baseValue = baseStats.ContainsKey(type) ? baseStats[type] : 0f;
            float addedValue = PlayerPrefs.GetFloat(type.ToString(), 0f);
            currentStats[type] = baseValue + addedValue;

            Debug.Log($"{type} : 기본 {baseValue} + 추가 {addedValue} = {currentStats[type]}");
        }
    }

    // ==================== 스탯 접근용 메서드 ====================

    public float GetStat(StatType type)
    {
        return currentStats.ContainsKey(type) ? currentStats[type] : 0f;
    }

    public void SetStat(StatType type, float value)
    {
        currentStats[type] = value;
    }

    public void AddStat(StatType type, float value)
    {
        if (!currentStats.ContainsKey(type))
            currentStats[type] = 0f;

        currentStats[type] += value;
    }

    // ==================== 디버그 ====================

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            foreach (var kvp in currentStats)
            {
                Debug.Log($"{kvp.Key} : {kvp.Value}");
            }
        }
    }
}
