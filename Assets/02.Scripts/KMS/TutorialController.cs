using UnityEngine;
using TMPro;
using PixelUI;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections; // Coroutine을 사용하기 위해 필요합니다

public class TutorialController : MonoBehaviour
{

    public GameObject tutorialPanel;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;

    public TextMeshProUGUI buttonText;

    public Image tutorialImg;

    public List<Sprite> tutorialImages; 

    public UnityEngine.UI.Button button;

    private int saveTutorialState ; //튜토리얼 상태 저장용



    public void OnEnable()
    {
        if (PlayerPrefs.HasKey("TutorialState") == true)
        {
            saveTutorialState = PlayerPrefs.GetInt("TutorialState"); 
        }

        if(saveTutorialState == 0)
        {
            Tutorial1();
        }

        // 이벤트 구독: 함수 시그니처(void)를 유지하기 위해 기존 함수명 유지
        GaugeRidingLogic.OnFullRidingGaue += Tutorial2; //라이딩 게이지가 100프로 찼을때
        GaugeRidingLogic.OnOverDriveEvent += Tutorial3; //오버드라이브 진입시 
    }
    public void OnDisable()
    {
        GaugeRidingLogic.OnFullRidingGaue -= Tutorial2;
        GaugeRidingLogic.OnOverDriveEvent -= Tutorial3;
    }

   void Tutorial1()
    {
        Time.timeScale = 0;
        
        tutorialPanel.SetActive(true);
        
        title.text = "기본 조작";
        description.text = "마우스 우클릭으로 이동이 가능합니다";

        buttonText.text = "다음";

        tutorialImg.sprite = tutorialImages[0];
        tutorialImg.SetNativeSize();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => Tutorial1_1());
        
    }
    void Tutorial1_1()
    {
        title.text = "기본 조작";
        description.text = "[SpaceBar]로 적을 밀쳐낼수있습니다. \n적중 시 탑승 게이지를 획득합니다.";

        buttonText.text = "다음";

        tutorialImg.sprite = tutorialImages[1];
        tutorialImg.SetNativeSize();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => Tutorial1_2());
    }
        void Tutorial1_2()
    {
        title.text = "각종 제단";
        description.text = "각종 제단에 근처에서 일정 시간 머무르면 이로운 효과를 얻습니다.";

        buttonText.text = "닫기";

        tutorialImg.sprite = tutorialImages[2];
        tutorialImg.SetNativeSize();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => CloseTutorial(1));
    }
    
    // ----------------------------------------------------
    // [수정] 이벤트 핸들러: 코루틴 시작 역할만 수행
    void Tutorial2()
    {
        // 튜토리얼2의 코루틴 실행
        StartCoroutine(Tutorial2Coroutine());
    }
    
    // [추가] 딜레이와 실제 로직을 수행하는 코루틴
    IEnumerator Tutorial2Coroutine()
    {
        // 1초 딜레이 (Time.timeScale = 0 이어도 작동하도록 Realtime 사용)
        yield return new WaitForSecondsRealtime(0f);
        
        if(saveTutorialState >= 2)
        {
            yield break; // 튜토리얼 완료 상태면 코루틴 종료
        }

        Time.timeScale = 0; // 딜레이 후 시간 정지

        tutorialPanel.SetActive(true); 
    
        title.text = "커서 탑승";
        description.text = "e키를 눌러서 커서에 탑승이 가능합니다";

        buttonText.text = "닫기";

        tutorialImg.sprite = tutorialImages[3];
        tutorialImg.SetNativeSize();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => CloseTutorial(2));
    }
    // ----------------------------------------------------
    
    // ----------------------------------------------------
    // [수정] 이벤트 핸들러: 코루틴 시작 역할만 수행
    void Tutorial3()
    {
        // 튜토리얼3의 코루틴 실행
        StartCoroutine(Tutorial3Coroutine());
    }

    // [추가] 딜레이와 실제 로직을 수행하는 코루틴
    IEnumerator Tutorial3Coroutine()
    {
       // 1초 딜레이 (Time.timeScale = 0 이어도 작동하도록 Realtime 사용)
       yield return new WaitForSecondsRealtime(1f);

       if(saveTutorialState >= 3)
        {
            yield break; // 튜토리얼 완료 상태면 코루틴 종료
        }
        
        Time.timeScale = 0; // 딜레이 후 시간 정지

        tutorialPanel.SetActive(true); 
    
        title.text = "오버드라이브";
        description.text = "이동은 마우스 커서를 따라갑니다, \n이제 자동으로 스킬이 발동됩니다.";

        buttonText.text = "닫기";

        tutorialImg.sprite = tutorialImages[4];
        tutorialImg.SetNativeSize();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => CloseTutorial(3));
    }
    // ----------------------------------------------------

    void CloseTutorial(int state)
    {
        Time.timeScale = 1 ;
        //튜토리얼 창닫기
        tutorialPanel.SetActive(false);
        
        // [개선] 튜토리얼 상태가 하락하지 않도록 현재 저장된 상태보다 높을 때만 저장
        if (state > saveTutorialState)
        {
             PlayerPrefs.SetInt("TutorialState", state);
             saveTutorialState = state;
        }
    }


//버튼클릭시 튜토리얼 초기화
   public void ResetTutorialState()
    {
        PlayerPrefs.DeleteKey("TutorialState");
        saveTutorialState = 0;
    }
}