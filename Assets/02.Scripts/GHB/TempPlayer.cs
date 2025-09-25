using UnityEngine;
using System.Collections.Generic;
using UnityEditor.EditorTools;

public class TempPlayer : MonoBehaviour
{
    [Header("기본 스탯")]
    [Tooltip("플레이어의 기본 스탯 능력치를 작성하세요")]
    public List<StatEntry> baseStats = new List<StatEntry>(); // 인스펙터에서 기본값 설정 가능

    [Header("상점 스탯")]
    [Tooltip("상점 데이터 ScriptableObject들을 끌어 두세요")]
    public List<ShopStatData> shopStats = new List<ShopStatData>();

    private Dictionary<string, float> currentStats = new Dictionary<string, float>();
    private Dictionary<string, float> baseStatDict = new Dictionary<string, float>();

    [System.Serializable]
    public class StatEntry
    {
        public string statName;
        public float value;
    }

    void Start()
    {
        // 기본 스탯 딕셔너리로 변환
        baseStatDict.Clear();
        foreach (var stat in baseStats)
        {
            baseStatDict[stat.statName] = stat.value;
        }

        InitializeStats();
    }

    private void InitializeStats()
    {
        currentStats.Clear();

        foreach (var stat in shopStats)
        {
            float baseValue = baseStatDict.ContainsKey(stat.itemStatName) ? baseStatDict[stat.itemStatName] : 0f;
            float addedValue = PlayerPrefs.GetFloat(stat.itemStatName, 0f);
            currentStats[stat.itemStatName] = baseValue + addedValue;
            Debug.Log($"{stat.itemStatName}의 기본스탯 {baseValue} + 추가구매 스탯 {addedValue} = {currentStats[stat.itemStatName]}");
        }
    }

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
