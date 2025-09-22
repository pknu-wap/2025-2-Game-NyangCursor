using UnityEngine;
using System;

public class TimerManager : MonoBehaviour
{
    [Header("스테이지 클리어까지 버텨야 하는 시간 (분 단위)")]
    [SerializeField] private float clearMinutes;
    private float clearSeconds;

    private float currentTime; // 진행(Play상태로 유지)된 시간

    public static event Action<float> OnTimerTick; // UI에서 구독
    public static event Action OnStageClear;       // 클리어 이벤트

    private bool isPaused = false;

    void Start()
    {
        clearSeconds = clearMinutes * 60f;
        StageFlowManager.OnStageStateChanged += HandleStageStateChanged;
    }

    void OnDestroy()
    {
        StageFlowManager.OnStageStateChanged -= HandleStageStateChanged;
    }

    void Update()
    {
        if (isPaused) return;
        if (currentTime >= clearSeconds) return; // 이미 목표 시간 도달 → 더 진행 안 함

        // 0 → clearSeconds 로 증가
        currentTime += Time.deltaTime;
        currentTime = Mathf.Min(currentTime, clearSeconds);

        // 현재 진행 시간을 담은 이벤트 발송
        OnTimerTick?.Invoke(currentTime);

        // 클리어 조건
        if (currentTime >= clearSeconds)
        {
            Debug.Log("스테이지 클리어!");
            OnStageClear?.Invoke();
        }
    }

    private void HandleStageStateChanged(StageFlowManager.StageState state)
    {
        // Play 상태일 때만 타이머 진행
        if (state == StageFlowManager.StageState.Play)
        {
            isPaused = false;
        }
        else
        {
            isPaused = true;
        }
    }
}
