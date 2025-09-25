using UnityEngine;

[CreateAssetMenu(menuName = "Shop/ItemData")]
public class ShopStatData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int price;
}
