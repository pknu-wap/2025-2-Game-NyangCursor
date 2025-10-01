using UnityEngine;

[CreateAssetMenu(fileName = "IngameStat", menuName = "IngameStat/UpgradeStatData")]
public class UpgradeScriptableObjects : ScriptableObject
{
    public StatType optionStatType;
    public float maxvalue = 5;
    public float minvalue = 1;
    [Header("설명 (UI 표시용)")]
    [TextArea(2, 5)]
    public string optionDescription;
}
