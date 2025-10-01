using UnityEngine;

[CreateAssetMenu(menuName = "Shop/ItemData")]
public class ShopStatData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;
    public Sprite icon;
    public int price;

    [Header("스탯 정보")]
    public StatType itemStatType;
    public float itemStatValue;

    [Header("설명 (마우스 오버 시 보여줄 텍스트)")]
    [TextArea(3, 10)] // 에디터에서 여러 줄 입력 가능
    public string description;
}
