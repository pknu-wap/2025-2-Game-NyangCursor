using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Image 사용

public class GaugeOverdriveController : MonoBehaviour
{
    [Header("Feel per Tier (PresetBlend)")]
    [Tooltip("티어별 묵직함(1=가벼움 ~ 100=묵직). 순서대로 20,50,80,100")]
    [Range(1, 100)] public int[] tierPresetBlend = { 55, 70, 85, 95 };

    [Tooltip("티어 전환 시 presetBlend 부드럽게 보간할 시간(초). 0이면 즉시")]
    public float presetBlendLerpTime = 0.15f;

    [Header("References (Optional but Recommended)")]
    [Tooltip("속도/외부제어를 단계적으로 바꾸려면 여기에 CursorControllerStable을 드래그")]
    public CursorControllerStable mover; // 선택: 이동 스크립트

    [Header("UI")]
    public Image fuelGage_img;        // 0~1 fillAmount
    public Image overDriveGage_img;   // 0~1 fillAmount

    [Header("Fuel")]
    [Range(0, 100)] public float fuel = 100f;   // 연료 현재값
    [Tooltip("연료 자연충전 속도 (초당). 20초 풀충전이면 5")]
    public float fuelRechargePerSec = 5f;
    [Tooltip("W키 누를 때 연료 소모 속도 (초당)")]
    public float fuelConsumePerSec = 10f;

    [Header("Overdrive")]
    [Tooltip("연료 1 소모당 오버드라이브 증가량 (기본 0.7)")]
    public float odGainPerFuel = 0.7f;
    [Tooltip("키를 뗀 뒤 감소 시작까지 지연(초)")]
    public float odDecayDelay = 3f;
    [Tooltip("감소 시작 후 초당 감소량")]
    public float odDecayPerSec = 8f;

    [Range(0, 100)] public float overdrive = 0f; // 현재 OD 값

    [Header("Tier Settings (20 / 50 / 80 / 100)")]
    [Tooltip("티어 경계 통과 시 externalControl 잠깐 켜지는 시간(초). 순서대로")]
    public float[] tierLockDur = { 0.2f, 0.4f, 0.6f, 0.8f};
    [Tooltip("티어 구간 동안 유지되는 속도 배율. 순서대로")]
    public float[] tierSpeedMul = { 1.2f, 1.4f, 1.6f, 1.8f };

    [Header("Misc")]
    public bool paused = false;

    // ★ BOOST: 구간 진입 시 켜질 오브젝트 & 지속 시간
    [Header("Boost Visual")]
    [Tooltip("티어 '상승' 진입 시 1초간 켜질 부스트 연출 오브젝트")]
    public GameObject boostObject;
    [Tooltip("부스트 오브젝트가 켜져 있을 시간(초)")]
    public float boostOnDuration = 1f;

    // 내부 상태
    float _odSinceRelease = 0f;
    bool _boostHeld = false;
    int _currentTier = 0;       // 0~4 (0: <20, 1:20~49, 2:50~79, 3:80~99, 4:==100)
    int _lastAppliedTier = 0;   // 마지막으로 연출(락)까지 적용한 티어
    float _baseSpeed = -1f;     // mover 속도 원본 저장
    Coroutine _lockRoutine;

    // ★ BOOST: 코루틴 핸들
    Coroutine _boostRoutine;

    void Start()
    {
        if (mover != null) _baseSpeed = mover.speed;
        ClampAll();
        ApplyTierSpeed(_currentTier); // 시작 구간 속도 반영
        UpdateUI();

        // ★ BOOST: 시작 시엔 꺼두기(참조가 있을 때만)
        if (boostObject) boostObject.SetActive(false);
    }

    void Update()
    {
        if (paused) return;

        _boostHeld = Input.GetKey(KeyCode.W);
        float dt = Time.deltaTime;

        // --- Fuel / OD 처리 ---
        if (_boostHeld && fuel > 0f)
        {
            float consume = Mathf.Min(fuel, fuelConsumePerSec * dt);
            fuel -= consume;
            overdrive += consume * odGainPerFuel;
            _odSinceRelease = 0f; // 감소 지연 타이머 리셋
        }
        else
        {
            if (fuel < 100f) fuel += fuelRechargePerSec * dt;

            if (overdrive > 0f)
            {
                _odSinceRelease += dt;
                if (_odSinceRelease >= odDecayDelay)
                    overdrive -= odDecayPerSec * dt;
            }
        }

        ClampAll();

        // --- 티어 계산(20 / 50 / 80 / 100) ---
        int newTier = CalcTier_20_50_80_100(overdrive); // 0~4
        if (newTier != _currentTier)
        {
            // 구간 속도는 "현재 티어"에 맞춰 '유지'되도록 즉시 갱신
            _currentTier = newTier;
            ApplyTierSpeed(_currentTier);
            ApplyTierPresetBlend(_currentTier);

            // 티어가 상승할 때만 짧은 externalControl 연출 + BOOST 오브젝트
            if (_currentTier > _lastAppliedTier)
            {
                PlayTierLockBurst(_currentTier);
                TriggerBoostObject(); // ★ BOOST: 1초간 켜기
                _lastAppliedTier = _currentTier;
            }
            else if (_currentTier < _lastAppliedTier)
            {
                // 내려갈 때는 연출만 건너뛰고 기준만 갱신
                _lastAppliedTier = _currentTier;
            }
        }

        UpdateUI();
    }

    // ----- Helpers -----

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
        if (mover == null) return;

        if (_baseSpeed < 0f) _baseSpeed = mover.speed; // 안전장치

        float mul = 1f; // Tier 0 기본
        if (tier >= 1)
        {
            int idx = Mathf.Clamp(tier - 1, 0, tierSpeedMul.Length - 1); // 0..3
            mul = tierSpeedMul[idx];
        }

        mover.speed = _baseSpeed * mul; // 구간 동안 '유지'되는 계단식 속도
    }

    void PlayTierLockBurst(int tier)
    {
        if (mover == null) return;
        if (tier <= 0) return;

        int idx = Mathf.Clamp(tier - 1, 0, tierLockDur.Length - 1); // 0..3
        float lockDur = tierLockDur[idx];

        if (_lockRoutine != null) StopCoroutine(_lockRoutine);
        _lockRoutine = StartCoroutine(LockRoutine(lockDur));
    }

    IEnumerator LockRoutine(float lockDur)
    {
        bool prevLock = mover.externalControl;
        mover.externalControl = true; // 잠깐 조향 제한(연출)

        float t = 0f;
        while (t < lockDur)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // 우리가 켰던 경우에만 해제(외부 로직 보호)
        if (!prevLock) mover.externalControl = false;
    }

    void ApplyTierPresetBlend(int tier)
    {
        if (mover == null) return;

        // tier: 0~4 (0:<20, 1:20~49, 2:50~79, 3:80~99, 4:==100)
        int targetBlend;
        if (tier <= 0)
        {
            // 티어 0은 "기본 감각"으로 복귀: 보통 1~50 사이(너 기본값이면 50 권장)
            targetBlend = 50;
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
            mover.presetBlend = targetBlend;
        }
        else
        {
            // 부드럽게 적용
            if (_presetRoutine != null) StopCoroutine(_presetRoutine);
            _presetRoutine = StartCoroutine(LerpPresetBlendRoutine(targetBlend, presetBlendLerpTime));
        }
    }

    // 코루틴 보관용
    Coroutine _presetRoutine;

    IEnumerator LerpPresetBlendRoutine(int target, float dur)
    {
        if (mover == null) yield break;

        float t = 0f;
        float start = mover.presetBlend;
        while (t < dur)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / dur);
            mover.presetBlend = Mathf.RoundToInt(Mathf.Lerp(start, target, u));
            yield return null;
        }
        mover.presetBlend = target;
    }

    // ★ BOOST: 구간 상승 시 1초간 오브젝트 켜기
    void TriggerBoostObject()
    {
        if (!boostObject) return;

        if (_boostRoutine != null) StopCoroutine(_boostRoutine);
        _boostRoutine = StartCoroutine(BoostObjectRoutine());
    }

    IEnumerator BoostObjectRoutine()
    {
        boostObject.SetActive(true);
        float t = 0f;
        while (t < boostOnDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }
        boostObject.SetActive(false);
    }

    void UpdateUI()
    {
        if (fuelGage_img != null)
            fuelGage_img.fillAmount = fuel / 100f;

        if (overDriveGage_img != null)
            overDriveGage_img.fillAmount = overdrive / 100f;
    }
}
