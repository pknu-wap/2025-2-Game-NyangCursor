using UnityEngine;

public class ItemHandler : MonoBehaviour
{

    [SerializeField] private GoldManager goldManager;
    [SerializeField] private GaugeOverdriveLogic gaugeOverdriveLogic;

    [SerializeField] private float itemBonusGuageValue;
    [SerializeField][Range(0f, 1f)] private float goldGainBonus = 0f;

    public void ExecuteItem(ItemSO item, Transform player)
    {
        switch (item.itemType)
        {
            case ItemType.GoldBonus:
                GoldBonusAction();
                break;

            case ItemType.BoostBonus:
                BoostBonusAction(player);
                break;

            case ItemType.ShotGun:
                ShotGunAction();
                break;

            default:
                Debug.LogWarning($"아이템타입 결정 안됨: {item.itemName}");
                break;
        }
    }

    private void GoldBonusAction()
    {
        goldManager.SetBonus(goldGainBonus);
    }

    private void BoostBonusAction(Transform player)
    {
        gaugeOverdriveLogic.itemBonusGuage = itemBonusGuageValue;
    }

    private void ShotGunAction()
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
