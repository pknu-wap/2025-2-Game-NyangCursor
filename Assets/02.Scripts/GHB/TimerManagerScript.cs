using UnityEngine;
using System;

public class TimerManager : MonoBehaviour
{
    [Header("스테이지 클리어까지 버텨야 하는 시간 (분 단위)")]
    [SerializeField] private float clearMinutes;

    private float RemainingTime;

    public static event Action<float> OnTimerTick; // UI에서 구독
    public static event Action OnStageClear;       // 클리어 이벤트

    private bool isPaused = false;

    void Start()
    {
        RemainingTime = clearMinutes * 60f;
        StageFlowManager.OnStageStateChanged += HandleStageStateChanged;
    }

    void OnDestroy()
    {
        StageFlowManager.OnStageStateChanged -= HandleStageStateChanged;
    }

    void Update()
    {
        if (isPaused) return;
        if (RemainingTime <= 0f) return;

        RemainingTime -= Time.deltaTime;
        RemainingTime = Mathf.Max(RemainingTime, 0f);

        // UI 갱신
        OnTimerTick?.Invoke(RemainingTime);

        if (RemainingTime <= 0f)
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
