using UnityEngine;
using System;

public class TimerManager : MonoBehaviour
{
    [Header("스테이지 클리어까지 버텨야 하는 시간 (분 단위)")]
    [SerializeField] private float clearMinutes;
    private float clearSeconds;

    private float currentTime; // 진행(Play상태로 유지)된 시간

    public static event Action<float> OnTimerTick; // UI에서 실시간 시간 표시용
    public static event Action OnStageClear;       // 목표 시간 도달 시 스테이지 클리어 이벤트

    private bool isPaused = false;

    // 외부에서 읽기 가능하도록 프로퍼티 추가
    public float CurrentTime => currentTime;
    public float ClearSeconds => clearSeconds;

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
        if (currentTime >= clearSeconds) return; // 이미 목표 시간 도달 → 더 이상 증가 안 함

        // 시간 누적
        currentTime += Time.deltaTime;
        currentTime = Mathf.Min(currentTime, clearSeconds);

        // UI에 실시간 전달
        OnTimerTick?.Invoke(currentTime);

        // 목표 시간 도달 시 클리어 이벤트
        if (currentTime >= clearSeconds)
        {
            Debug.Log("스테이지 클리어!");
            OnStageClear?.Invoke();
        }
    }

    // StageFlowManager 상태 변화에 따라 타이머 일시정지 / 재개
    private void HandleStageStateChanged(StageFlowManager.StageState state)
    {
        isPaused = state != StageFlowManager.StageState.Play;
    }
}
