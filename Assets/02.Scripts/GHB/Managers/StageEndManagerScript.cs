using UnityEngine;
using System;

public class StageEndManager : MonoBehaviour
{
    // 현재 스테이지 번호
    [SerializeField] private int stageIndex;
    // float로 변경하여 밀리초까지 전달
    public static event Action<bool, float, int> OnStageEnd;

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
        Debug.Log("Stage End Triggered");
        float survivedTime = currentTime;
        int reward = CalculateReward(survivedTime);
        SaveReward(reward);
        if (isCleared)
        {
            UnlockNextStage();
        }
        OnStageEnd?.Invoke(isCleared, survivedTime, reward);
    }

    private int CalculateReward(float seconds)
    {
        return Mathf.RoundToInt(seconds * rewardRatio);
    }

    private void SaveReward(int reward)
    {
        // 기존 돈 가져오기
        int currentMoney = PlayerPrefs.GetInt("Money", 0);
        currentMoney += reward;
        PlayerPrefs.SetInt("Money", currentMoney);
        PlayerPrefs.Save();
    }

    private void UnlockNextStage()
    {
        PlayerPrefs.SetInt($"StageCleared_{stageIndex}", 1);
        PlayerPrefs.Save();
    }

    // StageFlowManager 상태 변화에 따라 타이머 일시정지 / 재개
    private void HandleStageStateChanged(StageFlowManager.StageState state)
    {
        if (state == StageFlowManager.StageState.End)
        {
            NotifyStageEnd();
        }
    }
}
