using UnityEngine;
using System;

public class StageFlowManager : MonoBehaviour
{
    public enum StageState
    {
        Play,       // 일반 플레이
        Augment,    // 증강 선택 UI 활성화
        Pause,       // 완전 일시정지
        End       // 게임 종료
    }

    public StageState CurrentState { get; private set; }

    public static event Action<StageState> OnStageStateChanged;

    void Start()
    {
        // 타이머 매니저의 클리어 이벤트 구독
        TimerManager.OnStageClear += StageClear;
        // 시작은 플레이
        SetState(StageState.Play);
    }

    void OnDestroy()
    {
        TimerManager.OnStageClear -= StageClear;
    }

    // 임시 ESC 토글 일시정지
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentState == StageState.Play)
            {
                SetStateToPause();
            }
            else if (CurrentState == StageState.Pause || CurrentState == StageState.Augment)
            {
                SetStateToPlay();
            }
        }
        // 임시 증강선택 단축키
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (CurrentState == StageState.Play)
            {
                SetStateToAugment();
            }
            else if (CurrentState == StageState.Augment)
            {
                SetStateToPlay();
            }
        }
    }

    public void SetState(StageState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        // 전역 이벤트로 알림 (UIManager, EnemySpawner 등에서 구독 가능)
        OnStageStateChanged?.Invoke(CurrentState);

        // 여기서 직접 Time.timeScale 조절도 가능
        switch (CurrentState)
        {
            case StageState.Play:
                Time.timeScale = 1f;
                break;
            case StageState.Augment:
            case StageState.Pause:
            case StageState.End:
                // 플레이어 / 적 엔티티에 isPaused 변수를 둔 다음 OnStageStateChanged 이벤트를 구독해서 제어하는 방식이 좋을듯
                // timeScale = 0은 임시 로직
                Time.timeScale = 0f;
                break;
        }
    }

    // 버튼 참조용
    public void SetStateToPlay()
    {
        SetState(StageState.Play);
    }

    public void SetStateToAugment()
    {
        SetState(StageState.Augment);
    }

    public void SetStateToPause()
    {
        SetState(StageState.Pause);
    }

    public void SetStateToEnd()
    {
        SetState(StageState.End);
    }

    private void StageClear()
    {
        Debug.Log("스테이지 클리어 로그");

        // ✅ 여기서 클리어 UI 표시, 보상 지급, 다음 씬 로딩 등 추가 가능
        // 예: UIManager.Instance.ShowClearScreen();
    }
}
