using System;
using UnityEngine;
using static PlayerStateLogic;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class NormalModController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private float speed = 5f;

    // 내부 상태
    private Vector2 clickTarget;
    private bool hasClickTarget;
    private bool isMove;
    private Vector3 camVel;
    [SerializeField] private Vector3 camOffset = new Vector3(0, 0, -10);
    [SerializeField] private float camSmooth = 0.15f;

    public static event Action<bool> OnWalk;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        GaugeOverdriveLogic.OnNormalEvent += HandleResetNormal;
    }

    private void Start()
    {
        speed = PlayerStatsManager.instance.GetStat(StatType.MoveSpeedUp);
    }

    private void OnDestroy()
    {
        GaugeOverdriveLogic.OnNormalEvent -= HandleResetNormal;
    }

    private void Update()
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.Normal)
            return;

        speed = PlayerStatsManager.instance.GetStat(StatType.MoveSpeedUp);

        // ✅ 1) 클릭한 지점으로 이동 (기존 로직)
        if (Input.GetMouseButtonDown(1))
        {
            UpdateClickTarget();
            hasClickTarget = true;
        }

        // ✅ 2) 마우스를 꾹 누르고 있을 때 현재 위치로 실시간 이동
        if (Input.GetMouseButton(1))
        {
            // 단, 기존 클릭 이동 도중에도 자연스럽게 반응하도록 함
            UpdateClickTarget();
            hasClickTarget = true;
        }

        // ✅ 3) 마우스 떼면 기존 클릭 이동만 유지 (지속 추적은 멈춤)
        if (Input.GetMouseButtonUp(1))
        {
            // 버튼을 떼도 클릭한 지점으로 이동 중이면 유지
            // 따라서 hasClickTarget = true 유지 (멈추지 않음)
            // 단, 마우스 누르고 있던 이동만 멈춘 상태로 전환
        }
    }

    private void FixedUpdate()
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.Normal)
            return;

        if (rb.bodyType != RigidbodyType2D.Kinematic)
            return;

        if (hasClickTarget)
        {
            Vector2 pos = rb.position;
            Vector2 dir = (clickTarget - pos);
            float dist = dir.magnitude;

            if (dist < 0.05f)
            {
                rb.linearVelocity = Vector2.zero;
                hasClickTarget = false;
                isMove = false;
                OnWalk?.Invoke(false);
            }
            else
            {
                // ✔ velocity 기반 이동
                Vector2 vel = dir.normalized * speed;
                rb.linearVelocity = vel;

                isMove = true;
                OnWalk?.Invoke(true);
            }

            // 좌우 반전 처리
            if (dir.x > 0.05f)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (dir.x < -0.05f)
                transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            // 클릭 타겟 없으면 멈춤
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void LateUpdate()
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.Normal)
            return;

        Vector3 targetPos = (Vector3)rb.position + camOffset;
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, targetPos, ref camVel, camSmooth);
    }

    private void UpdateClickTarget()
    {
        Vector3 m = Input.mousePosition;
        m.z = Mathf.Abs(cam.transform.position.z);
        clickTarget = cam.ScreenToWorldPoint(m);
    }

    public void HandleResetNormal()
    {
        hasClickTarget = false;
        isMove = false;
        clickTarget = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        OnWalk?.Invoke(false);
    }
}
