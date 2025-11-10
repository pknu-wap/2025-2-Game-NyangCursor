using UnityEngine;

// 아이템 행동 인터페이스
public interface IItemAction
{
    void Execute(Transform player);
}

// 아이템 SO
[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    [Header("공통 데이터")]
    public string itemName;
    public Sprite itemImage;
    [TextArea(2, 5)] public string itemDescription;

    [Header("아이템 고유 행동")]
    public ScriptableObject itemActionSO; // IItemAction 구현체 SO로 참조

    public void Execute(Transform player)
    {
        if (itemActionSO is IItemAction action)
            action.Execute(player);
        else
            Debug.LogWarning("행동 SO 누락");
    }
}
