using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Image 사용
using System;
using UnityEditor.ShaderGraph;
using static PlayerStateLogic;

public class GaugeOverdriveLogic : MonoBehaviour
{
    [Header("Feel per Tier (PresetBlend)")]
    [Tooltip("티어별 묵직함(1=가벼움 ~ 100=묵직). 순서대로 20,50,80,100")]
    [Range(1, 100)] public int[] tierPresetBlend = { 30, 45, 65, 80 };

    [Tooltip("티어 전환 시 presetBlend 부드럽게 보간할 시간(초). 0이면 즉시")]
    public float presetBlendLerpTime = 0.15f;

    [Header("References (Optional but Recommended)")]
    [Tooltip("속도/외부제어를 단계적으로 바꾸려면 여기에 CursorControllerStable을 드래그")]
    public OverDriveModController overDriveModController; // 선택: 이동 스크립트

    [Header("현재OverDrive 수치")]
    [Range(0, 100)] public float overdrive = 0f; // 현재 OD 값

    [Header("Tier Settings (20 / 50 / 80 / 100)")]
    //"티어 경계 통과 시 externalControl 잠깐 켜지는 시간(초). 순서대로")]
    private float[] tierLockDur = { 0.6f, 0.6f, 0.6f, 0.6f };
    [Tooltip("티어 구간 동안 유지되는 속도 배율. 순서대로")]
    private float[] tierSpeedMul = { 1.2f, 1.5f, 2f, 2.5f };

    // 구간 진입 시 켜질 오브젝트
    [Header("Boost Visual")]
    [Tooltip("티어 '상승' 진입 시 부스트 연출 오브젝트")]
    public GameObject boostObject;


    // 내부 상태
    private int currentTier = 0;       // 0~4티어  (0티어: <20티어, 1티어: 20~49, 2티어:50~79, 3티어 : 80~99, 4티어: 100)
    private int lastAppliedTier = 0;   // 마지막으로 연출(락)까지 적용한 티어
    private float overdriveTimer = 0f; //오버드라이브 전용 타이머
    private int lockId = 0; //구간별 조종제어 코루틴 아이디
    [SerializeField] private float baseDecreaseRate = 2f; // 기본 게이지 감소량(초당)



    //각종 코루틴
    private Coroutine lockRoutine;  //구간별 조작제어 코루틴
    private Coroutine boostRoutine; //구간별 부스터 on/off 코루틴
    private Coroutine presetRoutine; //구간별 회전력 제어 코루틴

    //이벤트
    public static event Action<float> OnOverDriveTick;//오버드라이브 게이지 틱 이벤트 발생
    public static event Action OnGetOffEvent; //게이지0으로 내리는 이벤트 발생 
    public static event Action OnNormalEvent; //노말모드 진입 이벤트 발생

    void Awake()
    {
        OnOverDriveTick?.Invoke(overdrive);//UI이벤트발송(GaugeUI)
        

        //BOOST 시작 시엔 꺼두기(참조가 있을 때만)
        if (boostObject)
        {
            boostObject.SetActive(false);
        }

        GaugeRidingLogic.OnOverDriveEvent += HandleInitialOverDrive;

    }

    void Start()
    {
        ClampGauge(); //각종 게이지 최소값,최대값 보정 
        ApplyTierSpeed(currentTier); // 시작 구간 속도 반영
    }

    private void OnDestroy()
    {
        GaugeRidingLogic.OnOverDriveEvent -= HandleInitialOverDrive;
    }

    void Update()
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.OverDrive)
            return;

        ClampGauge();//게이지 최소값,최대값 보정 

        OnOverDriveTick?.Invoke(overdrive);//UI이벤트발송(GaugeUI)

        // 게이지 바닥 처리
        if (overdrive <= 0f)
        {
            
            PlayerStateLogic.Instance.ChangeState(PlayerStateLogic.PlayerState.GetOff);
            overdrive = 0f;
            overdriveTimer = 0f; // 리셋
            currentTier = 0;
            lastAppliedTier = 0;
            OnGetOffEvent?.Invoke();//내리는이벤트 발생 To (NormalCursorMove,PlayerAnimatorController)
            Invoke("CahngeStateToNormal", 1); //1초뒤에 노말 상태로 변경

        }

        //오버드라이브 게이지가 100이 되면 폭주모드 진입
        if (overdrive >= 100)
        {
            //PlayerStateLogic.Instance.ChangeState(PlayerState.Berserk);//폭주모드 상태변경
            print("폭주모드진입!!!!!!!!!!!");
        }

        DecreaseOverdriveGauge();


        // --- 티어 계산(20 / 50 / 80 / 100) ---
        int newTier = CalcTier_20_50_80_100(overdrive); // 0~4
        if (newTier != currentTier)
        {
            // 티어 갱신
            currentTier = newTier;

            // 구간별 상승효과만 적용
            if (currentTier > lastAppliedTier)
            {
                ApplyTierSpeed(currentTier);       // 속도 갱신
                ApplyTierPresetBlend(currentTier); // 블렌드 갱신
                PlayTierLockBurst(currentTier);   //조종제어
                TriggerBoostObject(currentTier);  //순간부스터
                lastAppliedTier = currentTier;     // 상승했을 때만 기록
                print("현재티어 :" + currentTier );
            }

            // 하락 시에는 아무것도 안 함 (lastAppliedTier 유지)
        }
    }

    private void DecreaseOverdriveGauge()
    {
        // 오버드라이브 시간 누적
        overdriveTimer += Time.deltaTime;

        // 기본 감소 속도
        float decreaseRate = baseDecreaseRate;


        // --- 티어별 감소 속도 ---
        switch (lastAppliedTier)
        {
            case 0: // <20 → 감소 없음
                decreaseRate = 0f;
                break;
            case 1: // 20~49
                decreaseRate = baseDecreaseRate * 5f;
                break;
            case 2: // 50~79
                decreaseRate = baseDecreaseRate * 7.5f;
                break;
            case 3: // 80~99
                decreaseRate = baseDecreaseRate * 10f;
                break;
            case 4: // ==100
                decreaseRate = baseDecreaseRate * 5f;
                break;
        }

        // 게이지 감소
        overdrive -= decreaseRate * Time.deltaTime;
    }


    public void UpOverDriveGauge(float amount) //적처치시 오버드라이브 게이지 상승함수
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.OverDrive)
            return;

        if (lastAppliedTier == 4) return; //폭주모드 일 경우방지

        overdrive += amount;
        OnOverDriveTick?.Invoke(overdrive);//라이딩 게이지 틱 이벤트 발송

    }





    // ------------------------------------- Helpers ----------------------------------------------------

    //오버드라이브 첫 진입 시 게이지 지급 및 티어 초기화
    void HandleInitialOverDrive()
    {
        overdrive = 5;
        OnOverDriveTick?.Invoke(overdrive);//UI이벤트발송(GaugeUI)
        ApplyTierSpeed(currentTier);
        ApplyTierPresetBlend(currentTier);
        Debug.Log("HandleInitialOverDrive 호출됨");
    }

    private void CahngeStateToNormal()
    {
        PlayerStateLogic.Instance.ChangeState(PlayerState.Normal);
        OnNormalEvent?.Invoke(); //(NormalModController에서 이동초기화)
    }


    void ClampGauge()
    {
        overdrive = Mathf.Clamp(overdrive, 0f, 101f);
    }

    // 0: <20, 1: [20,50), 2: [50,80), 3:[80,100), 4: ==100
    int CalcTier_20_50_80_100(float od)
    {
        if (od >= 100f) return 4;
        if (od >= 80f) return 3;
        if (od >= 50f) return 2;
        if (od >= 20f) return 1;
        return 0;
    }

    void ApplyTierSpeed(int tier)
    {
        float mul = 1f; // Tier 0 기본
        if (tier >= 1)
        {
            int idx = Mathf.Clamp(tier - 1, 0, tierSpeedMul.Length - 1); // 0.1.2.3
            mul = tierSpeedMul[idx];
        }

        // 최종 속도 계산 
        float finalSpeed = PlayerStatsManager.instance.GetStat(StatType.OverdriveModeMoveSpeed) * mul; //todo 참조변경
        overDriveModController.speed = finalSpeed;
        
    }


    void PlayTierLockBurst(int tier)//구간별 조종제어
    {
        if (overDriveModController == null) return;
        if (tier <= 0) return;

        //배열[0] = 티어 1
        int idx = Mathf.Clamp(tier - 1, 0, tierLockDur.Length - 1); // 0..3
        float lockDur = tierLockDur[idx]; //구간별 락 시간 넘겨줌

        if (lockRoutine != null)
        {
            StopCoroutine(lockRoutine);
        }

        lockRoutine = StartCoroutine(LockRoutine(lockDur));
    }



    IEnumerator LockRoutine(float lockDur)
    {
        lockId++;              // 새 락 시작 시 세대 증가
        int myId = lockId;     // 내 세대 번호 저장

        overDriveModController.externalControl = true; // 무조건 잠금

        float t = 0f;
        while (t < lockDur)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // 내가 최신 락일 때만 false로 해제
        if (myId == lockId)
            overDriveModController.externalControl = false;
    }

    void ApplyTierPresetBlend(int tier)
    {
        if (overDriveModController == null) return;

        // tier: 0~4 (0:<20, 1:20~49, 2:50~79, 3:80~99, 4:==100)
        int targetBlend;
        if (tier <= 0)
        {
            // 티어 0은 "기본 감각"으로 복귀:저점이 10)
            targetBlend = 20;
        }
        else
        {
            int idx = Mathf.Clamp(tier - 1, 0, tierPresetBlend.Length - 1); // 0..3
            targetBlend = tierPresetBlend[idx];
        }

        targetBlend = Mathf.Clamp(targetBlend, 1, 100);

        if (presetBlendLerpTime <= 0f)
        {
            // 즉시 적용
            overDriveModController.presetBlend = targetBlend;
        }
        else
        {
            // 부드럽게 적용
            if (presetRoutine != null) StopCoroutine(presetRoutine);
            presetRoutine = StartCoroutine(LerpPresetBlendRoutine(targetBlend, presetBlendLerpTime));
        }
    } 

    IEnumerator LerpPresetBlendRoutine(int target, float dur)
    {
        if (overDriveModController == null) yield break;

        float t = 0f;
        float start = overDriveModController.presetBlend;
        while (t < dur)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / dur);
            overDriveModController.presetBlend = Mathf.RoundToInt(Mathf.Lerp(start, target, u));
            yield return null;
        }
        overDriveModController.presetBlend = target;
    }

    //구간 상승 시 부스터 오브젝트 켜기
    void TriggerBoostObject(int tier)
    {
        if (!boostObject) return;

        if (boostRoutine != null)
        {
            StopCoroutine(boostRoutine);
        }

        boostRoutine = StartCoroutine(BoostObjectRoutine(tier));
    }


    IEnumerator BoostObjectRoutine(int tier)
    {
        // 부스터 연출
        boostObject.SetActive(true);

        // 배열[0] = 티어 1
        int idx = Mathf.Clamp(tier - 1, 0, tierSpeedMul.Length - 1);
        float tierMul = (tier >= 1) ? tierSpeedMul[idx] : 1f;

        // 부스터 동안 속도 ↑
        float boostedSpeed = PlayerStatsManager.instance.GetStat(StatType.OverdriveModeMoveSpeed) * tierMul * PlayerStatsManager.instance.GetStat(StatType.BoostExtraSpeed); //todo 참조변경

        // overDriveModController 속도 갱신
        overDriveModController.speed = boostedSpeed;

        float t = 0f;
        while (t < PlayerStatsManager.instance.GetStat(StatType.BoostOnDuration))  //todo 참조변경
        {
            t += Time.deltaTime;
            yield return null;
        }

        boostObject.SetActive(false);

        // 부스터 끝 → 원래 티어 속도로 복귀
        ApplyTierSpeed(tier);
    }

}
