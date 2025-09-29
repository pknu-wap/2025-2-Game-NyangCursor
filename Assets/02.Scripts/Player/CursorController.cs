using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class CursorController : MonoBehaviour
{
    [SerializeField] PlayerStat playerStat;

    [Header("Move (RUNTIME VALUES) - 읽기용")]
    [SerializeField] private float currentTurnRateDeg;
    [SerializeField] private float currentDeadzone;
    [SerializeField] private float currentTargetSmooth;

    [Header("Preset Blend")]
    [Range(1, 100)] public int presetBlend = 10;

    public float speed;

    [System.Serializable]
    public struct TurnFeelPreset
    {
        public float turnRateDeg;   // 회전 속도
        public float targetSmooth;  // 마우스 스무딩
        public float deadzone;      // 데드존 반경
    }

    [Header("Presets")]
    private TurnFeelPreset basePreset = new TurnFeelPreset
    {
        turnRateDeg = 360f,
        targetSmooth = 0.12f,
        deadzone = 0.25f
    };

    private TurnFeelPreset heavyPreset = new TurnFeelPreset
    {
        turnRateDeg = 90f,
        targetSmooth = 0.3f,
        deadzone = 0.4f
    };

    [Header("Camera Follow")]
    [SerializeField] private Transform followCam;
    [SerializeField] private Vector3 camOffset = new Vector3(0, 0, -10);
    [SerializeField] private float camSmooth = 0.15f;

    private Camera cam;
    private Rigidbody2D rb;
    private Vector3 smoothedTarget;
    private Vector3 targetVel;
    private Vector3 lastTarget;

    private float desiredAngle;
    private float angleVel;
    private Vector3 camVel;

    public bool externalControl = false;
    public GameObject deadZoneImg;

    [Header("Deadzone / Snap")]
    [Range(0f, 10f)] public float minAngleDeg = 2.5f;
    [Range(0f, 90f)] public float snapThreshold = 22f;
    [Range(0.0f, 0.2f)] public float smoothTimeSnap = 0.03f;

    private bool inDeadzone = false;
    private bool prevInDeadzone = false;

    [SerializeField] bool drawDeadzoneGizmos = true;

    //마우스 업데이트 관련
    private float stopTimer = 0f;          // 마우스 멈춤 누적 시간
    private float mouseStopDelay = 0.1f; // 몇 초 멈추면 딱 붙일지

    private void Awake()
    {
        speed = playerStat.speed; //처음 속도 초기화

        cam = followCam.GetComponent<Camera>();
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        smoothedTarget = transform.position;
        lastTarget = smoothedTarget;
        desiredAngle = rb.rotation;

        ApplyPresetBlend();
    }
    private float smoothLockTimer = 0f;  // 남은 락 시간(초)


    private void Update()
    {
        // --- 마우스 위치 ---
        Vector3 m = Input.mousePosition;
        m.z = Mathf.Abs(cam.transform.position.z);
        Vector3 target = cam.ScreenToWorldPoint(m);


        // --- 마우스 움직임 체크 ---
        bool mouseStopped = (target - lastTarget).sqrMagnitude < 0.01f;

        // --- 타이머 갱신 ---
        if (smoothLockTimer > 0f)
            smoothLockTimer -= Time.deltaTime;

        if (mouseStopped && smoothLockTimer <= 0f)
        {
            // 멈췄고, 락 해제된 상태 → 딱 붙이기
            smoothedTarget = target;
            targetVel = Vector3.zero; // 관성 제거
            print("d");
        }
        else
        {
            // 움직이거나(마우스 움직임), 혹은 락 타이머 중일 때 → 스무딩 유지
            smoothedTarget = Vector3.SmoothDamp(smoothedTarget, target, ref targetVel, currentTargetSmooth);
            print("s");

            // 움직임이 발생한 순간 → 락 걸기
            if (!mouseStopped)
                smoothLockTimer = 0.2f; // 0.5초 동안 d 금지
        }

        lastTarget = target;

        ApplyPresetBlend();

        // --- 외부 제어면 여기서 끝 ---
        if (externalControl) return;


        // --- 방향 계산 ---
        Vector2 to = (Vector2)(smoothedTarget - (Vector3)rb.position);
        float dist = to.magnitude;

        // Deadzone 체크
        float enterR = currentDeadzone;
        float exitR = currentDeadzone;

        if (inDeadzone)
        {
            if (dist > exitR) inDeadzone = false;
        }
        else
        {
            if (dist <= enterR) inDeadzone = true;
        }

        if (!prevInDeadzone && inDeadzone)
            angleVel = 0f;
        prevInDeadzone = inDeadzone;

        if (!inDeadzone)
        {
            float targetAngle = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg - 90f;

            float angDiff = Mathf.Abs(Mathf.DeltaAngle(desiredAngle, targetAngle));
            if (angDiff > minAngleDeg)
            {
                float smoothTime = (angDiff >= snapThreshold)
                    ? smoothTimeSnap
                    : (1f / Mathf.Max(1f, currentTurnRateDeg));

                desiredAngle = Mathf.SmoothDampAngle(
                    desiredAngle, targetAngle, ref angleVel, smoothTime
                );
            }

            if (deadZoneImg) deadZoneImg.SetActive(false);
        }
        else
        {
            if (deadZoneImg) deadZoneImg.SetActive(true);
        }


    }


    private void FixedUpdate()
    {
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
        rb.linearVelocity = forward * speed;
#else
        rb.velocity = forward * speed;
#endif
    }

    private void LateUpdate()
    {
        if (!followCam) return;
        Vector3 targetPos = (Vector3)rb.position + camOffset;
        followCam.position = Vector3.SmoothDamp(followCam.position, targetPos, ref camVel, camSmooth);
    }

    private void ApplyPresetBlend()
    {
        // playerStat.turnRateDeg → 1일 때 10f, 10일 때 360f
        float trNorm = Mathf.InverseLerp(1f, 10f, playerStat.turnRateDeg);
        trNorm = Mathf.Clamp01(trNorm); // 안전하게 0~1 범위 제한

        // --- playerStat 기반 "동적 heavyPreset" ---
        float boostedTurn = Mathf.Lerp(10f, 360f, trNorm);
        float boostedSmooth = Mathf.Lerp(0.3f, 0.12f, trNorm);
        float boostedDeadzone = Mathf.Lerp(0.4f, 0.25f, trNorm);

        // --- presetBlend(1~100) 보간 ---
        float t = Mathf.InverseLerp(1f, 100f, presetBlend);

        currentTurnRateDeg = Mathf.Lerp(basePreset.turnRateDeg, boostedTurn, t);
        currentTargetSmooth = Mathf.Lerp(basePreset.targetSmooth, boostedSmooth, t);
        currentDeadzone = Mathf.Lerp(basePreset.deadzone, boostedDeadzone, t);

        Debug.Log(currentTurnRateDeg);
    }

    void OnDrawGizmos()
    {
        if (!drawDeadzoneGizmos) return;

        Vector3 center = (rb != null) ? (Vector3)rb.position : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, currentDeadzone);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(smoothedTarget, 0.1f);
        }
    }
}
