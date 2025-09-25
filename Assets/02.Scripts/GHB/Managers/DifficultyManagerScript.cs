using System.Collections.Generic;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [Header("난이도가 증가하는 지점(분 단위)")]
    [SerializeField] private List<float> difficultyIncreaseMinutes = new List<float>();
    private List<float> difficultyIncreaseTimes;
    private int currentIndex = 0;

    void Start()
    {
        difficultyIncreaseTimes = new List<float>(difficultyIncreaseMinutes.Count);
        for (int i = 0; i < difficultyIncreaseMinutes.Count; i++)
        {
            difficultyIncreaseTimes.Add(difficultyIncreaseMinutes[i] * 60f);
        }

    }

    void OnEnable()
    {
        TimerManager.OnTimerTick += HandleTimerTick;
    }

    void OnDisable()
    {
        TimerManager.OnTimerTick -= HandleTimerTick;
    }

    private void HandleTimerTick(float elapsedTime)
    {
        if (currentIndex >= difficultyIncreaseTimes.Count) return;

        if (elapsedTime >= difficultyIncreaseTimes[currentIndex])
        {
            IncreaseDifficulty();
            currentIndex++;
        }
    }

    public void IncreaseDifficulty()
    {
        Debug.Log("난이도 증가 로그");
        // 타이머 매니저와 난이도 매니저는 역할이 연결되지만, 구분되어야 한다고 생각하여 스크립트 분리
        // 난이도 매니저를 통한 적 생성 매니저의 변수 값 변경
    }
}
