using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [Header("참조하는 오브젝트들")]
    [SerializeField] private TimerManager timerManager; // 직접 참조
    [SerializeField] private EnemyManager enemyManager; // 직접 참조

    [Header("이 스테이지에서 사용하는 난이도들")]
    [SerializeField] private List<DifficultyData> difficulties;

    private int currentIndex = -1;    // 현재 난이도

    // 현재 난이도를 외부에서 읽을 수 있도록 하는 프로퍼티
    public DifficultyData CurrentDifficulty => currentIndex >= 0 ? difficulties[currentIndex] : null;

    void Awake()
    {
        // 오름차순 정렬
        difficulties.Sort((a, b) => a.startTimeMinutes.CompareTo(b.startTimeMinutes));

        // 리스트가 비어있지 않다면 첫 번째 난이도로 초기화
        if (difficulties.Count > 0)
            currentIndex = 0;
    }

    void Start()
    {
        // 트리거 시간 등록
        // 이때 첫 번째 난이도는 이미 적용되어 있으므로 스킵
        for (int i = 1; i < difficulties.Count; ++i)
        {
            int index = i;
            float triggerTime = difficulties[i].StartTimeSeconds;
            timerManager.RegisterTrigger(triggerTime, () => ApplyDifficulty(index));
        }
    }

    private void ApplyDifficulty(int index)
    {
        currentIndex = index;
        DifficultyData data = difficulties[index];

        Debug.Log($"난이도가 변경됨! 현재 단계 = {currentIndex}");
        enemyManager.ApplyDifficulty(data);
    }
}
