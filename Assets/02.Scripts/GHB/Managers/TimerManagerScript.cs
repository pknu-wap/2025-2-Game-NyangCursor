using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    [Header("스테이지 클리어까지 버텨야 하는 시간 (분 단위)")]
    [SerializeField] private float clearMinutes;
    private float clearSeconds;

    private float currentTime; // 진행(Play상태로 유지)된 시간

    public static event Action<float> OnTimerTick; // UI에서 실시간 시간 표시용
    public static event Action OnStageClear;       // 목표 시간 도달 시 스테이지 클리어 이벤트

    private bool isPaused = false;
    private bool isCleared = false;

    // 트리거 시스템
    private Dictionary<float, Action> triggerCallbacks = new Dictionary<float, Action>();
    private List<float> pendingTriggerTimes = new List<float>();


    // 외부에서 읽기 가능하도록 프로퍼티 추가
    public float CurrentTime => currentTime;
    public float ClearSeconds => clearSeconds;
    public bool IsCleared => isCleared;

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

        // 시간 누적
        currentTime += Time.deltaTime;

        // UI에 실시간 전달
        OnTimerTick?.Invoke(currentTime);

        // 트리거 체크
        for (int i = pendingTriggerTimes.Count - 1; i >= 0; --i)
        {
            float triggerTime = pendingTriggerTimes[i];
            if (currentTime >= triggerTime)
            {
                triggerCallbacks[triggerTime]?.Invoke();
                pendingTriggerTimes.RemoveAt(i);
            }
        }

        // 클리어 조건
        if (currentTime >= clearSeconds && !isCleared)
        {
            isCleared = true;
            Debug.Log("스테이지 클리어!");
            StageFlowManager.instance.SetStateToEnd();
        }
    }

    // 특정 시간에 콜백을 등록하는 함수
    public void RegisterTrigger(float timeSeconds, Action callback)
    {
        if (triggerCallbacks.ContainsKey(timeSeconds))
            triggerCallbacks[timeSeconds] += callback;
        else
        {
            triggerCallbacks[timeSeconds] = callback;
            pendingTriggerTimes.Add(timeSeconds);
        }
    }

    // StageFlowManager 상태 변화에 따라 타이머 일시정지 / 재개
    private void HandleStageStateChanged(StageFlowManager.StageState state)
    {
        Debug.Log(state);
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
