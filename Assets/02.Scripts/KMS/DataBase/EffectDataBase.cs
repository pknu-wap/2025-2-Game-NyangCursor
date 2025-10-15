using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

public class EffectDatabase : MonoBehaviour
{
    //싱글톤 인스턴스
    public static EffectDatabase Instance { get; private set; }

    [Serializable]
    public class EffectData
    {
        public string name;
        public string title;
        public string info;
        public string type;
        public float startValue;
        public float minValue;
        public float maxValue;
        public int maxLevel;
        public string unit;
    }

    [Header("공개된 Google Sheets CSV URL")]
    public string googleSheetsCSVUrl;

    public Dictionary<string, EffectData> effectTable = new();
    public List<EffectData> allEffects = new(); // 모든 효과 리스트


    public bool isLoaded = false;

    //싱글톤 초기화
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(DownloadAndLoad());
    }

    IEnumerator DownloadAndLoad()
    {
        if (string.IsNullOrEmpty(googleSheetsCSVUrl))
        {
            Debug.LogError("Google Sheets CSV URL이 설정되지 않았습니다.");
            yield break;
        }

        UnityWebRequest www = UnityWebRequest.Get(googleSheetsCSVUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("CSV 다운로드 실패: " + www.error);
            yield break;
        }

        string csvText = www.downloadHandler.text;
        LoadCSVFromText(csvText);
     
        FinalizeList();

        //PrintAllEffects();
        isLoaded = true;
    }

    void LoadCSVFromText(string csvText)
    {
        string[] lines = csvText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV 데이터가 비어 있습니다.");
            return;
        }

        FieldInfo[] fields = typeof(EffectData).GetFields(BindingFlags.Public | BindingFlags.Instance);
        Debug.Log($"감지된 필드 수: {fields.Length}");

        for (int i = 1; i < lines.Length; i++) // 첫 행(헤더) 건너뜀
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');

            if (values.Length < fields.Length)
            {
                Debug.LogWarning($"행 {i}: 필드 개수({fields.Length})보다 CSV 열 개수({values.Length})가 적습니다. => {lines[i]}");
                continue;
            }

            EffectData data = new EffectData();

            for (int f = 0; f < fields.Length && f < values.Length; f++)
            {
                FieldInfo field = fields[f];
                string rawValue = values[f].Trim();

                try
                {
                    object converted = ConvertValue(rawValue, field.FieldType);
                    field.SetValue(data, converted);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"변환 실패 ({field.Name} = \"{rawValue}\"): {e.Message}");
                }
            }

            if (string.IsNullOrEmpty(data.name))
            {
                Debug.LogWarning($"행 {i}에 name 값이 비어 있음: {lines[i]}");
                continue;
            }

            if (effectTable.ContainsKey(data.name))
            {
                Debug.LogWarning($"중복된 key: {data.name}");
                continue;
            }

            effectTable[data.name] = data;
        }

        Debug.Log($"로드 완료: {effectTable.Count}개 항목");
    }

    object ConvertValue(string value, Type type)
    {
        if (type == typeof(string)) return value;
        if (type == typeof(int))
        {
            if (int.TryParse(value, out int i)) return i;
            return 0;
        }
        if (type == typeof(float))
        {
            if (float.TryParse(value, out float f)) return f;
            return 0f;
        }
        if (type == typeof(double))
        {
            if (double.TryParse(value, out double d)) return d;
            return 0.0;
        }
        if (type == typeof(bool))
            return value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1";
        return null;
    }

    void PrintAllEffects()
    {
        Debug.Log("------ EffectTable 전체 목록 ------");

        foreach (var kvp in effectTable)
        {
            var e = kvp.Value;
            Debug.Log($"{e.name} => {e.title}, {e.info}, {e.type}, {e.startValue}, {e.minValue}, {e.maxValue}, {e.maxLevel}, {e.unit}");
        }

        Debug.Log("------ 출력 완료 ------");
    }

    void FinalizeList()
    {
        allEffects = new List<EffectData>(effectTable.Values);
    }



    // 외부 접근용 Getter 함수 -------------------------

    /// <summary>
    /// 이름으로 EffectData 가져오기
    /// </summary>
    public EffectData GetEffectData(string name)
    {
        if (!isLoaded)
        {
            Debug.LogWarning("EffectDatabase가 아직 로드되지 않았습니다.");
            return null;
        }

        if (effectTable.TryGetValue(name, out var data))
            return data;

        Debug.LogWarning($"EffectData '{name}'를 찾을 수 없습니다.");
        return null;
    }
}