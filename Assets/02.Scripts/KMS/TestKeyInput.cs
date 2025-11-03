using UnityEngine;

public class TestKeyInput : MonoBehaviour
{

    // 호출할 객체를 연결할 변수
    public GaugeRidingLogic gaugeRiding;
    public GaugeOverdriveLogic gaugeOverDrive;
    
    void Update()
    {

        // 키보드 숫자 1 키 감지
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // float 인자로 원하는 값 전달 (예: 10f)
          //  gaugeRiding.UpRidingGauge(10f);
        }

        // 키보드 숫자 2 키 감지
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // float 인자로 원하는 값 전달 (예: 10f)
            //gaugeOverDrive.UpOverDriveGauge(10f);
        }
    }
}
