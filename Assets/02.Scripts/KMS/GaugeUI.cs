using UnityEngine;
using UnityEngine.UI;

public class GaugeUI : MonoBehaviour
{
    [SerializeField] private Image ridingGaugeImg; // 라이딩 게이지 이미지 (FillAmount 방식)
    [SerializeField] private Image overDriveGaugeImg; // 오버드라이브 게이지 이미지 (FillAmount 방식)
   

    private void OnEnable()
    {
        // 이벤트 구독
        GaugeRidingLogic.OnRidingGaugeTick += HandleUpdateRidingGauge;
        GaugeOverdriveLogic.OnOverDriveTick += HandleUpdateOverDriveGauge;
    }

    private void OnDisable()
    {
        // 이벤트 해제
        GaugeRidingLogic.OnRidingGaugeTick -= HandleUpdateRidingGauge;
        GaugeOverdriveLogic.OnOverDriveTick -= HandleUpdateOverDriveGauge;
    }


    private void HandleUpdateRidingGauge(float value)
    {
        if (ridingGaugeImg == null) return;

        // value(0~100)를 0~1로 변환해서 fillAmount에 반영
        ridingGaugeImg.fillAmount = value / 100f;
    }
    private void HandleUpdateOverDriveGauge(float value)
    {
        if (overDriveGaugeImg == null) return;

        // value(0~100)를 0~1로 변환해서 fillAmount에 반영
        overDriveGaugeImg.fillAmount = value / 100f;
    }



}
