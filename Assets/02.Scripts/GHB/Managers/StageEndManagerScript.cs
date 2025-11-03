using UnityEngine;
using System;

public class StageEndManager : MonoBehaviour
{
    [SerializeField] TimerManager timerManager;
    // 현재 스테이지 번호
    [SerializeField] private int stageIndex;
    // float로 변경하여 밀리초까지 전달
    public static event Action<bool, float, int> OnStageEnd;

    [SerializeField] private int rewardRatio;

    void OnEnable()
    {
        StageFlowManager.OnStageStateChanged += HandleStageStateChanged;
    }

    void OnDisable()
    {
        StageFlowManager.OnStageStateChanged -= HandleStageStateChanged;
    }

    // 게임 종료 시점에서 UI로 정보 전달
    public void NotifyStageEnd()
    {
        Debug.Log("Stage End Triggered");
        float survivedTime = timerManager.CurrentTime;
        int reward = CalculateReward(survivedTime);
        SaveReward(reward);
        if (timerManager.IsCleared)
        {
            UnlockNextStage();
        }
        OnStageEnd?.Invoke(timerManager.IsCleared, survivedTime, reward);
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

    private void HandleStageStateChanged(StageFlowManager.StageState state)
    {
        if (state == StageFlowManager.StageState.End)
        {
            NotifyStageEnd();
        }
    }
}
