using UnityEngine;

// 아이템 SO 추상 클래스
public abstract class ItemSO : ScriptableObject
{
    [Header("공통 데이터")]
    public string itemName;
    public Sprite itemImage;
    [TextArea(2, 5)] public string itemDescription;

    public abstract void Execute(Transform player);
}
