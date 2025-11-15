using UnityEngine;

public class ItemHandler : MonoBehaviour
{

    [SerializeField] private GoldManager goldManager;

    [SerializeField] private float itemBonusGuageValue;
    [SerializeField][Range(0f, 1f)] private float goldGainBonus = 0f;

    public void ExecuteItem(ItemSO item, Transform player)
    {
        switch (item.itemType)
        {
            case ItemType.GoldBonus:
                GoldBonusAction(item.value);
                break;

            case ItemType.SpeedBoost:
                SpeedBoostAction(player);
                break;

            case ItemType.ShotGun:
                ShotGunAction(item.value, player);
                break;

            default:
                Debug.LogWarning($"Unknown item type: {item.itemName}");
                break;
        }
    }

    private void GoldBonusAction(float bonus)
    {
        goldManager.SetBonus(bonus);
    }

    private void SpeedBoostAction(Transform player)
    {
        GaugeOverdriveLogic logic = player.GetComponent<GaugeOverdriveLogic>();
        logic.itemBonusGuage = itemBonusGuageValue;
    }

    private void ShotGunAction(float healAmount, Transform player)
    {
        UpgradeEventData data1 = new UpgradeEventData()
        {
            isSkillUpgrade = false,
            statKey = SkillStatKey.Damage,
            upgradeRatio = 100.0f,
            applyLevelUp = false
        };

        UpgradeEventData data2 = new UpgradeEventData()
        {
            isSkillUpgrade = false,
            statKey = SkillStatKey.Range,
            upgradeRatio = -50.0f,
            applyLevelUp = false
        };

        UpgradeManager.RaiseUpgrade(data1);
        UpgradeManager.RaiseUpgrade(data2);
    }
}
