using UnityEngine;
using TMPro;
using System;

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
        int ms = Mathf.FloorToInt(elapsedTime * 1000f % 1000f / 10f);
        timeText.text = $"{minutes:00}:{seconds:00}:{ms:00}";
    }

    // 종료 시 UI 표시
    private void DisplayEndResult(bool isCleared, int survivedSeconds, int reward)
    {
        gameEndPanel.SetActive(true);

        // 생존 시간 및 보상 표시
        clearTimeText.text = $"생존 시간 : {FormatTime(survivedSeconds)}";
        rewardText.text = $"획득 보상 : {reward} G";

        // 클리어 여부에 따라 메시지와 색상 변경
        if (isCleared)
        {
            resultText.text = "STAGE CLEAR";
            resultText.color = Color.green;
        }
        else
        {
            resultText.text = "GAME OVER";
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
            case StageFlowManager.StageState.Clear:
                // 게임 종료 패널은 DisplayEndResult에서 활성화
                break;
        }
    }

    private string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }
}
