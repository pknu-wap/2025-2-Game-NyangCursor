using UnityEngine;
using static PlayerStateLogic;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class OverDriveModController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI rbSpeedText;

    [Header("Move (RUNTIME VALUES) - 읽기용")]
    [SerializeField] private float currentTurnRateDeg;
    [SerializeField] private float currentDeadzone;
    [SerializeField] private float currentTargetSmooth;

    [Header("Preset Blend")]
    [Range(1, 100)] public int presetBlend = 20;

    public float speed;
    [HideInInspector] public float baseSpeed;

    [System.Serializable]
    public struct TurnFeelPreset
    {
        public float turnRateDeg;
        public float targetSmooth;
        public float deadzone;
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

    // 오버드라이브 종료 감속
    private float maxDecelSpeed = 10f;
    private Coroutine decelRoutine;

    // 충돌 감속 + 회복
    private Coroutine recoverRoutine;
    [SerializeField] private float decelDuration = 0.2f; //감속 속도
    [SerializeField] private float recoverDuration = 3f; //회복 속도


    private void Awake()
    {
        cam = followCam.GetComponent<Camera>();
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        smoothedTarget = transform.position;
        lastTarget = smoothedTarget;
        desiredAngle = rb.rotation;

        GaugeOverdriveLogic.OnGetOffEvent += HandleOverdriveExit;
    }

    private void Start()
    {
        ApplyPresetBlend();
        baseSpeed = speed; //나중에 값 받아올때도 한번 초기화 필요!!!
    }

    private void OnDestroy()
    {
        GaugeOverdriveLogic.OnGetOffEvent -= HandleOverdriveExit;
    }

    // 오버드라이브 종료 시 떨어지는 속도 처리
    private void HandleOverdriveExit()
    {
        if (recoverRoutine != null)
        {
            StopCoroutine(recoverRoutine);
            recoverRoutine = null;
        }

        if (decelRoutine != null)
            StopCoroutine(decelRoutine);

        decelRoutine = StartCoroutine(DecelerateToZero());
    }

    private IEnumerator DecelerateToZero()
    {
        Vector2 vel = rb.linearVelocity;

        if (vel.magnitude <= maxDecelSpeed)
            yield break;

        rb.linearVelocity = vel.normalized * maxDecelSpeed;
    }


    // 외부에서 충돌 감속 요청
    public void ApplyCollisionSlow(float slowFactor)
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.OverDrive)
            return;

        if (recoverRoutine != null)
            StopCoroutine(recoverRoutine);

        recoverRoutine = StartCoroutine(ReduceAndRecoverSpeed(slowFactor));
    }

    private IEnumerator ReduceAndRecoverSpeed(float slowFactor)
    {
        
        float startSpeed = speed;
        float reducedSpeed = baseSpeed * slowFactor;

        float elapsed = 0f;


        //감속
        while (elapsed < decelDuration)
        {
            elapsed += Time.deltaTime;
            speed = Mathf.Lerp(startSpeed, reducedSpeed, elapsed / decelDuration);
            yield return null;
        }


        //회복
        elapsed = 0f;
        while (elapsed < recoverDuration)
        {
            elapsed += Time.deltaTime;
            speed = Mathf.Lerp(reducedSpeed, baseSpeed, elapsed / recoverDuration);
            yield return null;
        }

        speed = baseSpeed;
        recoverRoutine = null;
    }


    // ------------------ 움직임 로직 ------------------

    private float smoothLockTimer = 0f;

    private void Update()
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.OverDrive)
            return;

        Vector3 m = Input.mousePosition;
        m.z = Mathf.Abs(cam.transform.position.z);
        Vector3 target = cam.ScreenToWorldPoint(m);

        bool mouseStopped = (target - lastTarget).sqrMagnitude < 0.01f;

        if (smoothLockTimer > 0f)
            smoothLockTimer -= Time.deltaTime;

        if (mouseStopped && smoothLockTimer <= 0f)
        {
            smoothedTarget = target;
            targetVel = Vector3.zero;
        }
        else
        {
            smoothedTarget = Vector3.SmoothDamp(smoothedTarget, target, ref targetVel, currentTargetSmooth);

            if (!mouseStopped)
                smoothLockTimer = 0.1f;
        }

        lastTarget = target;
        ApplyPresetBlend();

        if (externalControl) return;

        Vector2 to = (Vector2)(smoothedTarget - (Vector3)rb.position);
        float dist = to.magnitude;

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

                desiredAngle = Mathf.SmoothDampAngle(desiredAngle, targetAngle, ref angleVel, smoothTime);
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
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.OverDrive)
            return;

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

        rbSpeedText.text = rb.linearVelocity.magnitude.ToString();
    }

    private void LateUpdate()
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.OverDrive)
            return;

        Vector3 targetPos = (Vector3)rb.position + camOffset;
        followCam.position = Vector3.SmoothDamp(followCam.position, targetPos, ref camVel, camSmooth);
    }


    private void ApplyPresetBlend()
    {
        float trNorm = Mathf.InverseLerp(
            1f, 10f,
            PlayerStatsManager.instance.GetStat(StatType.RotationPowerUp));

        trNorm = Mathf.Clamp01(trNorm);

        float boostedTurn = Mathf.Lerp(10f, 360f, trNorm);
        float boostedSmooth = Mathf.Lerp(0.3f, 0.12f, trNorm);
        float boostedDeadzone = Mathf.Lerp(0.4f, 0.25f, trNorm);

        float t = Mathf.InverseLerp(1f, 100f, presetBlend);

        currentTurnRateDeg = Mathf.Lerp(basePreset.turnRateDeg, boostedTurn, t);
        currentTargetSmooth = Mathf.Lerp(basePreset.targetSmooth, boostedSmooth, t);
        currentDeadzone = Mathf.Lerp(basePreset.deadzone, boostedDeadzone, t);
    }


    private void OnDrawGizmos()
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
