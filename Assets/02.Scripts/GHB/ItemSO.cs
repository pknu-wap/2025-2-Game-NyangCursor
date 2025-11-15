using UnityEngine;

public enum ItemRarity
{
    Normal,
    Rare,
    Legendery
}

public enum ItemType
{
    GoldBonus,
    SpeedBoost,
    ShotGun
    // 나중에 추가 가능
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    [Header("공통 데이터")]
    public string itemName;
    public Sprite itemImage;
    public ItemRarity itemRarity;
    [TextArea(2, 5)] public string itemDescription;

    public ItemType itemType;
    public float value; // 예: 골드 보너스 %, 스피드 배율 등
}
