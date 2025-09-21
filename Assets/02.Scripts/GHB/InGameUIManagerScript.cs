using UnityEngine;
using TMPro; // TextMeshProUGUI 사용
using System;

public class InGameUIManager : MonoBehaviour
{
    [Header("타이머 텍스트")]
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("일시정지 관련 UI")]
    [SerializeField] private GameObject PausePanel;

    void OnEnable()
    {
        TimerManager.OnTimerTick += UpdateTimerUI;
        StageFlowManager.OnStageStateChanged += HandleInGameUI;
    }

    void OnDisable()
    {
        TimerManager.OnTimerTick -= UpdateTimerUI;
        StageFlowManager.OnStageStateChanged += HandleInGameUI;
    }

    // 타이머 매니저에서 시간을 받아온 후 시간 UI 갱신
    private void UpdateTimerUI(float elapsedTime)
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        // 밀리초 계산
        int milliseconds = Mathf.FloorToInt(elapsedTime * 1000f % 1000f / 10f);

        // "MM:SS:ms" 형태로 표시
        timeText.text = $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }


    // 플로우 매니저에서 상태를 받아온 후 UI 갱신
    private void HandleInGameUI(StageFlowManager.StageState newState)
    {
        switch (newState)
        {
            case StageFlowManager.StageState.Play:
                Time.timeScale = 1f;
                PausePanel.SetActive(false);
                break;
            case StageFlowManager.StageState.Augment:
            case StageFlowManager.StageState.Pause:
                PausePanel.SetActive(true);
                break;
            case StageFlowManager.StageState.Clear:
                // 플레이어 / 적 엔티티에 isPaused 변수를 둔 다음 OnStageStateChanged 이벤트를 구독해서 제어하는 방식이 좋을듯
                // timeScale = 0은 임시 로직
                Time.timeScale = 0f;
                break;
        }
    }
}
