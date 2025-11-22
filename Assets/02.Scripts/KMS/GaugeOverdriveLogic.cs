using System.Collections;
using UnityEngine;
using System;
using static PlayerStateLogic;

public class GaugeOverdriveLogic : MonoBehaviour
{
    [Header("References")]
    [Tooltip("오버드라이브 이동 제어 스크립트")]
    public OverDriveModController overDriveModController;

    [Header("현재 OverDrive 수치")]
    public float overdrive = 0f; // 현재 OD 값

    [Header("감소 속도 설정")]
    [SerializeField] private float baseDecreaseRate = 10f; // 기본 게이지 감소량(초당)

    [Header("부스터 설정")]
    [Tooltip("부스터 지속 시간(초)")]
    [SerializeField] private float boostDuration = 1f;

    [Tooltip("부스터 시 속도 배율")]
    [SerializeField] private float boostExtraSpeed = 2f;

    [Tooltip("부스터 시 켜질 비주얼 오브젝트")]
    public GameObject boostObject;

    [HideInInspector]
    public float itemBonusGuage = 0f;

    private Coroutine boostRoutine;
    private Coroutine presetRoutine;

    // 이벤트
    public static event Action<float> OnOverDriveTick; // 게이지 변화 시 이벤트

    public static event Action OnUpOverDriveGauge; //게이지 차거나 감소했을때 이벤트(ui깜빡임위함)

    public static event Action OnGetOffEvent; // 게이지가 0이 되었을 때 이벤트
    public static event Action OnNormalEvent; // 노말 상태 복귀 이벤트

    public static event Action OnBoostEvent; //부스터 발동 이벤트

    private void Awake()
    {
        OnOverDriveTick?.Invoke(overdrive);

        if (boostObject)
            boostObject.SetActive(false);

        GaugeRidingLogic.OnOverDriveEvent += HandleInitialOverDrive;

        // 글로벌 이벤트 버스 사용
        GameEvents.Subscribe<float>(GameEventType.OnEnemyDeath, UpOverDriveGauge);
    }

    private void Start()
    {
        ClampGauge();
    }

    private void OnDestroy()
    {
        GaugeRidingLogic.OnOverDriveEvent -= HandleInitialOverDrive;

        // 글로벌 이벤트 버스 사용
        GameEvents.Unsubscribe<float>(GameEventType.OnEnemyDeath, UpOverDriveGauge);
    }

    private void Update()
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.OverDrive)
            return;

        ClampGauge();
        OnOverDriveTick?.Invoke(overdrive);

        // 게이지 바닥 처리
        if (overdrive <= 0f)
        {
            PlayerStateLogic.Instance.ChangeState(PlayerState.GetOff);
            overdrive = 0f;
            OnGetOffEvent?.Invoke();

            // 글로벌 이벤트 버스 사용
            GameEvents.Publish(GameEventType.OnPlayerFinishOverdrive);

            Invoke(nameof(ChangeStateToNormal), 1f);
            return;
        }

        // 게이지 감소
        DecreaseOverdriveGauge();
    }

    private void DecreaseOverdriveGauge()
    {
        overdrive -= baseDecreaseRate * Time.deltaTime;
    }

    public void UpOverDriveGauge(float amount)
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.OverDrive)
            return;

        overdrive += amount + itemBonusGuage;
        OnOverDriveTick?.Invoke(overdrive);
        OnUpOverDriveGauge?.Invoke();
    }

    private void HandleInitialOverDrive()
    {
        overdrive = 100f;
        OnOverDriveTick?.Invoke(overdrive);
        OnUpOverDriveGauge?.Invoke();
        ApplyPresetBlend(60); // 기본값 중간 정도로 설정

        TriggerBooster();
        Debug.Log("HandleInitialOverDrive 호출됨 - 게이지 100으로 설정됨");
    }

    private void ChangeStateToNormal()
    {
        PlayerStateLogic.Instance.ChangeState(PlayerState.Normal);
        OnNormalEvent?.Invoke();//pushBlast에서 착지 후 넉백

    }

    private void ClampGauge()
    {
        overdrive = Mathf.Clamp(overdrive, 0f, 100f);
    }

    // -------------------------------------------------------------------
    // 🎯 신규 기능: 프리셋 값(1~100)을 직접 적용하는 함수
    // -------------------------------------------------------------------
    public void ApplyPresetBlend(int presetValue)
    {
        if (overDriveModController == null)
            return;

        presetValue = Mathf.Clamp(presetValue, 1, 100);

        if (presetRoutine != null)
            StopCoroutine(presetRoutine);

        presetRoutine = StartCoroutine(LerpPresetBlendRoutine(presetValue, 0.2f));
    }

    private IEnumerator LerpPresetBlendRoutine(int target, float dur)
    {
        if (overDriveModController == null)
            yield break;

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

    // -------------------------------------------------------------------
    // 🎯 신규 기능: 언제든 호출 가능한 부스터 발동 함수
    // -------------------------------------------------------------------
    public void TriggerBooster(float duration = -1f, float extraSpeed = -1f)
    {
        if (overDriveModController == null)
            return;

        if (boostRoutine != null)
            StopCoroutine(boostRoutine);

        float finalDur = (duration > 0) ? duration : boostDuration;
        float finalExtra = (extraSpeed > 0) ? extraSpeed : boostExtraSpeed;

        OnBoostEvent?.Invoke();

        boostRoutine = StartCoroutine(BoostRoutine(finalDur, finalExtra));
    }

    private IEnumerator BoostRoutine(float duration, float extraSpeed)
    {
        if (boostObject)
            boostObject.SetActive(true);

        //조작제어
        overDriveModController.externalControl = true;

        // 현재 속도 저장
        float originalSpeed = overDriveModController.speed;

        // 부스터 속도 적용
        overDriveModController.speed = originalSpeed * extraSpeed;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }



        // 부스터 종료 후 복귀
        if (boostObject)
            boostObject.SetActive(false);

        overDriveModController.speed = originalSpeed;


        //조작제어 풀기
        overDriveModController.externalControl = false;
    }
}
