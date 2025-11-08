using UnityEngine;
using System;

public class StageEndManager : MonoBehaviour
{
    [SerializeField] TimerManager timerManager;
    [SerializeField] GoldManager goldManager;

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

        // 플레이 시간을 TimeManager 에서 가져옴
        float survivedTime = timerManager.CurrentTime;

        // 이전까지 얻은 골드와 이번 게임에서 얻은 골드를 GoldManager 에서 가져옴
        int savedGold = goldManager.SavedGold;
        int earnedGold = goldManager.EarnedGold;

        // 획득한 골드는 PlayerPrefs 에 저장
        SaveGold(savedGold, earnedGold);

        if (timerManager.IsCleared)
        {
            UnlockNextStage();
        }

        OnStageEnd?.Invoke(timerManager.IsCleared, survivedTime, earnedGold);
    }

    private void SaveGold(int savedGold, int earnedGold)
    {
        // 기존에 저장된 골드에 새로 획득한 골드를 추가
        int totalGold = savedGold + earnedGold;

        // PlayerPrefs 에 쓰기 후 저장
        PlayerPrefs.SetInt("Money", totalGold);
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
