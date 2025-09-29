using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Image 사용
using System;

public class GaugeOverdriveController : MonoBehaviour
{
    [SerializeField] private PlayerStat playerStat;


    [Header("Feel per Tier (PresetBlend)")]
    [Tooltip("티어별 묵직함(1=가벼움 ~ 100=묵직). 순서대로 20,50,80,100")]
    [Range(1, 100)] public int[] tierPresetBlend = { 30, 45, 65, 80 };

    [Tooltip("티어 전환 시 presetBlend 부드럽게 보간할 시간(초). 0이면 즉시")]
    public float presetBlendLerpTime = 0.15f;

    [Header("References (Optional but Recommended)")]
    [Tooltip("속도/외부제어를 단계적으로 바꾸려면 여기에 CursorControllerStable을 드래그")]
    public CursorController cursorController; // 선택: 이동 스크립트

    [Header("UI")]
    public Image fuelGaugeImg;        // 0~1 fillAmount
    public Image overDriveGaugeImg;   // 0~1 fillAmount
    [Header("현재Fuel 수치")]
    [Range(0, 100)] public float fuel = 100f;   // 연료 현재값
    [Header("현재OverDrive 수치")]
    [Range(0, 100)] public float overdrive = 0f; // 현재 OD 값

    [Header("Tier Settings (20 / 50 / 80 / 100)")]
    //"티어 경계 통과 시 externalControl 잠깐 켜지는 시간(초). 순서대로")]
    private float[] tierLockDur = { 0.2f, 0.4f, 0.6f, 0.8f };
    [Tooltip("티어 구간 동안 유지되는 속도 배율. 순서대로")]
    private float[] tierSpeedMul = { 1.2f, 1.5f, 2f, 2.5f };

    [Header("Misc")]
    public bool paused = false;

    // 구간 진입 시 켜질 오브젝트
    [Header("Boost Visual")]
    [Tooltip("티어 '상승' 진입 시 부스트 연출 오브젝트")]
    public GameObject boostObject;


    // 내부 상태
    private float odSinceRelease = 0f;
    private bool boostHeld = false;
    private int currentTier = 0;       // 0~4 (0: <20, 1:20~49, 2:50~79, 3:80~99, 4:==100)
    private int lastAppliedTier = 0;   // 마지막으로 연출(락)까지 적용한 티어


    //각종 코루틴
    private Coroutine lockRoutine;  //구간별 조작제어 코루틴
    private Coroutine boostRoutine; //구간별 부스터 on/off 코루틴
    private Coroutine presetRoutine; //구간별 회전력 제어 코루틴

    [SerializeField] private ClickModController clickModController;


    void Start()
    {

        ClampAll(); //각종 게이지 최소값,최대값 보정 
        ApplyTierSpeed(currentTier); // 시작 구간 속도 반영
        UpdateUI();

        //BOOST 시작 시엔 꺼두기(참조가 있을 때만)
        if (boostObject)
        {
            boostObject.SetActive(false);
        }

    }

    void Update()
    {
        if (paused) return;

        if (overdrive > 20) 
        {
            clickModController.isClickMode = false;
        }


        boostHeld = Input.GetKey(KeyCode.W);
        float dt = Time.deltaTime;
        ClampAll();//각종 게이지 최소값,최대값 보정 

        //연료게이지,od게이지 조작
        HandleFuelAndOverdrive(dt);

        // --- 티어 계산(20 / 50 / 80 / 100) ---
        int newTier = CalcTier_20_50_80_100(overdrive); // 0~4
        if (newTier != currentTier)
        {
            currentTier = newTier;
            ApplyTierSpeed(currentTier); //구간별 속도 조절
            ApplyTierPresetBlend(currentTier); //구간별 회전력 조절

            //구간별 상승효과
            if (currentTier > lastAppliedTier)
            {
                PlayTierLockBurst(currentTier); //조종제어
                TriggerBoostObject();           //순간부스터
                lastAppliedTier = currentTier;
            }
            //구간별 하락효과
            else if (currentTier < lastAppliedTier)
            {
                // 내려갈 때는 따로 효과없음
                lastAppliedTier = currentTier;
            }
        }

        UpdateUI(); //게이지 UI업데이트
    }

    void HandleFuelAndOverdrive(float dt)
    {
        // --- Fuel / OD 처리 ---
        if (boostHeld && fuel > 0f) //키를 누르고 연료가 0보다 클때
        {
            float consume = Mathf.Min(fuel, playerStat.fuelConsumePerSec * dt);
            fuel -= consume; //연료소모
            overdrive += consume * playerStat.odGainPerFuel; //od게이지 충전
            odSinceRelease = 0f; // 감소 지연 타이머 리셋
        }
        else//키를 뗀 상황
        {
            //연료게이지가 100미만일 경우 자동충전
            if (fuel < 100f)
            {
                fuel += playerStat.fuelRechargePerSec * dt;
            }

            //od게이지가 0보다 크면 자동식기
            if (overdrive > 0f)
            {
                odSinceRelease += dt; //자동식기 타이머 증가
                if (odSinceRelease >= playerStat.odDecayDelay) //딜레이 시간보다 클 경우 식기 시작
                    overdrive -= playerStat.odDecayPerSec * dt;
            }
        }

    }

    // ------------------------------------- Helpers ----------------------------------------------------

    void ClampAll()
    {
        fuel = Mathf.Clamp(fuel, 0f, 100f);
        overdrive = Mathf.Clamp(overdrive, 0f, 100f);
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
            int idx = Mathf.Clamp(tier - 1, 0, tierSpeedMul.Length - 1); // 0..3
            mul = tierSpeedMul[idx];
        }

        // 최종 속도 계산
        float finalSpeed = playerStat.speed * mul;
        cursorController.speed = finalSpeed;
    }


    void PlayTierLockBurst(int tier)//구간별 조종제어
    {
        if (cursorController == null) return;
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
        bool prevLock = cursorController.externalControl;
        cursorController.externalControl = true; // 잠깐 조향 제한(연출)

        float t = 0f;
        while (t < lockDur)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // 우리가 켰던 경우에만 해제(외부 로직 보호)
        if (!prevLock) cursorController.externalControl = false;
    }

    void ApplyTierPresetBlend(int tier)
    {
        if (cursorController == null) return;

        // tier: 0~4 (0:<20, 1:20~49, 2:50~79, 3:80~99, 4:==100)
        int targetBlend;
        if (tier <= 0)
        {
            // 티어 0은 "기본 감각"으로 복귀:저점이 10)
            targetBlend = 10;
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
            cursorController.presetBlend = targetBlend;
        }
        else
        {
            // 부드럽게 적용
            if (presetRoutine != null) StopCoroutine(presetRoutine);
            presetRoutine = StartCoroutine(LerpPresetBlendRoutine(targetBlend, presetBlendLerpTime));
        }
    } //구간별 회전력 

    IEnumerator LerpPresetBlendRoutine(int target, float dur)
    {
        if (cursorController == null) yield break;

        float t = 0f;
        float start = cursorController.presetBlend;
        while (t < dur)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / dur);
            cursorController.presetBlend = Mathf.RoundToInt(Mathf.Lerp(start, target, u));
            yield return null;
        }
        cursorController.presetBlend = target;
    }

    //구간 상승 시 1초간 오브젝트 켜기
    void TriggerBoostObject()
    {
        if (!boostObject) return;

        if (boostRoutine != null)
        {
            StopCoroutine(boostRoutine);
        }

        boostRoutine = StartCoroutine(BoostObjectRoutine(currentTier));
    }


    IEnumerator BoostObjectRoutine(int tier)
    {
        // 부스터 연출
        boostObject.SetActive(true);

        // 배열[0] = 티어 1
        int idx = Mathf.Clamp(tier - 1, 0, tierSpeedMul.Length - 1);
        float tierMul = (tier >= 1) ? tierSpeedMul[idx] : 1f;

        // 부스터 동안 속도 ↑
        float boostedSpeed = playerStat.speed * tierMul * playerStat.boostExtraSpeed;

        // CursorController 속도 갱신
        cursorController.speed = boostedSpeed;

        float t = 0f;
        while (t < playerStat.boostOnDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        boostObject.SetActive(false);

        // 부스터 끝 → 원래 티어 속도로 복귀
        ApplyTierSpeed(tier);
    }

    //각종 게이지 UI업데이트
    void UpdateUI()
    {
        if (fuelGaugeImg != null)
            fuelGaugeImg.fillAmount = fuel / 100f;

        if (overDriveGaugeImg != null)
            overDriveGaugeImg.fillAmount = overdrive / 100f;
    }




}
