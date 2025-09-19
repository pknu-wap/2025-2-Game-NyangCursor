using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class CursorControllerStable : MonoBehaviour
{
    [Header("Move (RUNTIME VALUES) - 읽기용")]
    [SerializeField, Tooltip("보간된 현재 이동 속도(옵션, 필요 없으면 삭제 가능)")]
    private float currentSpeed;
    [SerializeField] private float currentTurnRateDeg;
    [SerializeField] private float currentDeadzone;
    [SerializeField] private float currentTargetSmooth;

    [Header("Base Runtime Params (기본 시작값)")]
    public float speed = 8f;

    [Header("Preset Blend")]
    [Tooltip("1=기본 프리셋, 100=묵직 프리셋")]
    [Range(1, 100)] public int presetBlend = 50;

    [Header("Dynamic Deadzone")]
    [Tooltip("속도에 비례해서 deadzone을 보정하는 계수. 0이면 보정 없음")]
    public float deadzoneSpeedFactor = 2.0f;

    [System.Serializable]
    public struct TurnFeelPreset
    {
        [Tooltip("초당 회전(도/초). 낮을수록 묵직")]
        public float turnRateDeg;
        [Tooltip("목표점 스무딩 시간. 높을수록 묵직")]
        public float targetSmooth;
        [Tooltip("데드존 반경. 높을수록 묵직")]
        public float deadzone;
    }

    [Header("Presets")]
    [Tooltip("기본(경쾌) 프리셋")]
    public TurnFeelPreset basePreset = new TurnFeelPreset
    {
        turnRateDeg = 360f,
        targetSmooth = 0.12f,
        deadzone = 0.25f
    };

    [Tooltip("묵직 프리셋")]
    public TurnFeelPreset heavyPreset = new TurnFeelPreset
    {
        turnRateDeg = 180f,
        targetSmooth = 0.25f,
        deadzone = 0.35f
    };

    [Header("Camera Follow")]
    [SerializeField] private Transform followCam;
    [SerializeField] private Vector3 camOffset = new Vector3(0, 0, -10);
    [SerializeField] private float camSmooth = 0.15f;

    Camera cam;
    Rigidbody2D rb;
    Vector3 smoothedTarget;
    float desiredAngle;
    float angleVel;
    Vector3 camVel;

    // ✅ 외부 제어 플래그
    public bool externalControl = false;

    public GameObject deadZone_img;

    // ★ 히스테리시스/스냅 옵션
    [Header("Deadzone Stabilizer / Snap")]
    [Tooltip("히스테리시스 비율(입·출 반경 차이). 0.06~0.10 권장")]
    [Range(0f, 0.3f)] public float hysteresis = 0.08f;
    [Tooltip("아주 작은 각 변화 무시(도). 2~3도 권장")]
    [Range(0f, 10f)] public float minAngleDeg = 2.5f;
    [Tooltip("각도 차가 이 이상이면 빠르게 '확' 추종(도)")]
    [Range(0f, 90f)] public float snapThreshold = 22f;
    [Tooltip("스냅시 부드러움 시간(작을수록 즉각). 예: 0.03")]
    [Range(0.0f, 0.2f)] public float smoothTimeSnap = 0.03f;

    // 내부 상태(히스테리시스)
    bool inDeadzone = false;
    bool prevInDeadzone = false;

    void Awake()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        smoothedTarget = transform.position;
        desiredAngle = rb.rotation;

        ApplyPresetBlend();
        currentSpeed = speed; // 표시용
    }

    void Update()
    {
        if (externalControl) return;

        // 프리셋 블렌딩(실시간 튜닝 반영)
        ApplyPresetBlend();

        // 마우스 → 월드
        Vector3 m = Input.mousePosition;
        m.z = Mathf.Abs(cam.transform.position.z);
        Vector3 target = cam.ScreenToWorldPoint(m);

        // 목표점 스무딩
        float k = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.0001f, currentTargetSmooth));
        smoothedTarget = Vector3.Lerp(smoothedTarget, target, k);

        // 방향 벡터
        Vector2 to = (Vector2)(smoothedTarget - (Vector3)rb.position);
        float dist = to.magnitude;

        // 속도 기반 deadzone 보정
        float eff = currentDeadzone + deadzoneSpeedFactor * speed * Time.fixedDeltaTime;

        // ★ 히스테리시스: 입·출 반경 분리
        float enterR = eff;
        float exitR = eff * (1f + hysteresis);

        if (inDeadzone)
        {
            if (dist > exitR) inDeadzone = false;
        }
        else
        {
            if (dist <= enterR) inDeadzone = true;
        }

        // ★ 데드존 진입 '그 프레임'에 관성 제거 → 경계 꿈틀 억제
        if (!prevInDeadzone && inDeadzone)
            angleVel = 0f;
        prevInDeadzone = inDeadzone;

        if (!inDeadzone)
        {
            float targetAngle = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg - 90f;

            // ★ 미세 각 변화는 무시(데드밴드)
            float angDiff = Mathf.Abs(Mathf.DeltaAngle(desiredAngle, targetAngle));
            if (angDiff > minAngleDeg)
            {
                // ★ 큰 각도 차면 스냅(더 빠른 추종)
                float smoothTime = (angDiff >= snapThreshold)
                    ? smoothTimeSnap
                    : (1f / Mathf.Max(1f, currentTurnRateDeg));

                desiredAngle = Mathf.SmoothDampAngle(
                    desiredAngle, targetAngle, ref angleVel, smoothTime
                );
            }

            if (deadZone_img) deadZone_img.SetActive(false);
        }
        else
        {
            if (deadZone_img) deadZone_img.SetActive(true);
        }

        // 표시용 동기화(옵션)
        currentSpeed = speed;
    }

    void FixedUpdate()
    {
        // ★ newAngle 기준으로 회전/이동 동기화
        float newAngle = rb.rotation;

        if (!externalControl)
        {
            float maxStep = currentTurnRateDeg * Time.fixedDeltaTime;
            newAngle = Mathf.MoveTowardsAngle(rb.rotation, desiredAngle, maxStep);
            rb.MoveRotation(newAngle);
        }

        float rad = newAngle * Mathf.Deg2Rad;
        Vector2 forward = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = forward * speed;      // 항상 최신 speed 사용
#else
        rb.velocity = forward * speed;            // currentSpeed 대신 speed 권장
#endif
    }

    void LateUpdate()
    {
        if (!followCam) return;
        Vector3 targetPos = (Vector3)rb.position + camOffset;
        followCam.position = Vector3.SmoothDamp(followCam.position, targetPos, ref camVel, camSmooth);
    }

    // --- Helpers ---
    void ApplyPresetBlend()
    {
        // 1~100 → 0~1 (0=기본, 1=묵직)
        float t = Mathf.InverseLerp(1f, 100f, presetBlend);

        currentTurnRateDeg = Mathf.Lerp(basePreset.turnRateDeg, heavyPreset.turnRateDeg, t);
        currentTargetSmooth = Mathf.Lerp(basePreset.targetSmooth, heavyPreset.targetSmooth, t);
        currentDeadzone = Mathf.Lerp(basePreset.deadzone, heavyPreset.deadzone, t);
    }
}
