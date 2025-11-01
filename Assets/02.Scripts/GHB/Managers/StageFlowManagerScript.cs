using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class StageFlowManager : MonoBehaviour
{
    public enum StageState
    {
        Play,       // 일반 플레이
        Augment,    // 증강 선택 UI 활성화
        Altar, // 제단 접속
        Pause,       // 완전 일시정지
        End       // 게임 종료
    }

    public StageState CurrentState { get; private set; }

    public static event Action<StageState> OnStageStateChanged;

    public static StageFlowManager instance;

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 업그레이드 매니저 선택 이벤트 구독
        //UpgradeManager.OnUpgradeSelected += SelectedandSetStateToPlay;
        UpgradeManager1.OnUpgradeSelected1 += SelectedandSetStateToPlay; //신규
        SkillRemoveAltarUIManager.OnAltarEvent += SetStateToPlay;
        // 시작은 플레이
        SetState(StageState.Play);
    }


    void OnDestroy()
    {
        //UpgradeManager.OnUpgradeSelected -= SelectedandSetStateToPlay;
        UpgradeManager1.OnUpgradeSelected1 -= SelectedandSetStateToPlay; //신규
        SkillRemoveAltarUIManager.OnAltarEvent -= SetStateToPlay;
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
        if (Input.GetKeyDown(KeyCode.O))
        {
            GoToLobby();
        }
    }

    public void SetState(StageState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        // 상태별 TimeScale 자동 제어
        switch (CurrentState)
        {
            case StageState.Play:
                Time.timeScale = 1f;
                break;

            case StageState.Augment:
            case StageState.Pause:
                Time.timeScale = 0f;
                break;

            case StageState.Altar:
            case StageState.End:
                // 필요시 따로 제어 가능 (기본은 1)
                Time.timeScale = 1f;
                break;

            default:
                Time.timeScale = 1f;
                break;
        }

        // 전역 이벤트로 알림 (UIManager, EnemySpawner 등에서 구독 가능)
        OnStageStateChanged?.Invoke(CurrentState);

        Debug.Log($"[StageFlow] State Changed → {CurrentState}, TimeScale: {Time.timeScale}");
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

    public void SetStateToAltar()
    {
        SetState(StageState.Altar);
    }

    public void SetStateToPause()
    {
        SetState(StageState.Pause);
    }

    public void SetStateToEnd()
    {
        SetState(StageState.End);
    }

    public void SelectedandSetStateToPlay(UpgradeEventData data)
    {
        SetState(StageState.Play);
    }

    // 임시 씬 이동 메서드
    private void GoToLobby()
    {
        SceneManager.LoadScene("StageUnlockMakeScene");
    }
}
