using PixelUI;
using UnityEngine;

public class ridingGaugeUI : MonoBehaviour
{
    [SerializeField] ValueBar valueBar;

    [SerializeField] GameObject keyInfoBtn;

    private void OnEnable()
    {
        // 이벤트 구독
        GaugeRidingLogic.OnRidingGaugeTick += HandleRidingGaugeUpdate;
    }

    private void OnDisable()
    {
        // 이벤트 해제
        GaugeRidingLogic.OnRidingGaugeTick -= HandleRidingGaugeUpdate;
    }


    private void HandleRidingGaugeUpdate(float value)
    {
        valueBar.SetDirect(value);

        if (value >= 100)
        {
            keyInfoBtn.SetActive(true);
        }
        else
        {
            keyInfoBtn.SetActive(false);
        }
    }
}
