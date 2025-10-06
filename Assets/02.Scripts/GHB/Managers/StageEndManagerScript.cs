using UnityEngine;
using System;

public class StageEndManager : MonoBehaviour
{
    public static event Action<bool, int, int> OnStageEnd;

    [SerializeField] private int rewardRatio;

    private float currentTime;
    private bool isCleared = false;   // 클리어 여부

    void OnEnable()
    {
        TimerManager.OnTimerTick += CacheTime;
        TimerManager.OnStageClear += HandleStageClear;
        StageFlowManager.OnStageStateChanged += HandleStageStateChanged;
    }

    void OnDisable()
    {
        TimerManager.OnTimerTick -= CacheTime;
        TimerManager.OnStageClear -= HandleStageClear;
        StageFlowManager.OnStageStateChanged -= HandleStageStateChanged;
    }

    private void CacheTime(float elapsedTime)
    {
        currentTime = elapsedTime;
    }

    // 클리어 여부 기록
    private void HandleStageClear()
    {
        isCleared = true;
    }

    // 게임 종료 시점에서 UI로 정보 전달
    public void NotifyStageEnd()
    {
        Debug.Log("됨");
        int survivedSeconds = Mathf.FloorToInt(currentTime);
        int reward = CalculateReward(survivedSeconds);

        OnStageEnd?.Invoke(isCleared, survivedSeconds, reward);
    }

    private int CalculateReward(int seconds)
    {
        return seconds * rewardRatio;
    }

    // StageFlowManager 상태 변화에 따라 타이머 일시정지 / 재개
    private void HandleStageStateChanged(StageFlowManager.StageState state)
    {
        if (state == StageFlowManager.StageState.Clear)
        {
            NotifyStageEnd();
        }
    }
}
