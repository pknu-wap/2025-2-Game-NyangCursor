using UnityEngine;
using TMPro;

public class InGameUIManager : MonoBehaviour
{
    [Header("타이머 텍스트")]
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("일시정지 관련 UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("증강 선택 관련 UI")]
    [SerializeField] private GameObject augmentPanel;


    [Header("게임 종료 UI")]
    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private TextMeshProUGUI clearTimeText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private TextMeshProUGUI resultText; // 종료 메시지

    void OnEnable()
    {
        TimerManager.OnTimerTick += UpdateTimerUI;
        StageFlowManager.OnStageStateChanged += HandleInGameUI;
        StageEndManager.OnStageEnd += DisplayEndResult;
    }

    void OnDisable()
    {
        TimerManager.OnTimerTick -= UpdateTimerUI;
        StageFlowManager.OnStageStateChanged -= HandleInGameUI;
        StageEndManager.OnStageEnd -= DisplayEndResult;
    }

    private void UpdateTimerUI(float elapsedTime)
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt(elapsedTime * 1000f % 1000f / 10f);
        timeText.text = $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }

    // 종료 시 UI 표시
    private void DisplayEndResult(bool isCleared, float survivedTime, int reward)
    {
        gameEndPanel.SetActive(true);

        int minutes = Mathf.FloorToInt(survivedTime / 60f);
        int seconds = Mathf.FloorToInt(survivedTime % 60f);
        int milliseconds = Mathf.FloorToInt(survivedTime * 1000f % 1000f / 10f);

        clearTimeText.text = $"생존 시간 : {minutes:00}:{seconds:00}:{milliseconds:00}";
        rewardText.text = $"획득 보상 : {reward} G";

        if (isCleared)
        {
            resultText.text = "MISSION COMPLETE";
            resultText.color = Color.green;
        }
        else
        {
            resultText.text = "MISSION FAILED";
            resultText.color = Color.red;
        }
    }

    private void HandleInGameUI(StageFlowManager.StageState newState)
    {
        switch (newState)
        {
            case StageFlowManager.StageState.Play:
                pausePanel.SetActive(false);
                augmentPanel.SetActive(false); 
                break;
            case StageFlowManager.StageState.Augment:
                augmentPanel.SetActive(true);
                break;
            case StageFlowManager.StageState.Pause:
                pausePanel.SetActive(true);
                break;
            // End 상태의 경우 DisplayEndResult에서 따로 관리
        }
    }
}
