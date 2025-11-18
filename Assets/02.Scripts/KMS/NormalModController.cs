using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using static PlayerStateLogic;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class NormalModController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Tilemap groundTilemap;

    [Header("Settings")]
    [SerializeField] private float speed = 5f;

    [Header("Ground Check by Tile")]
    [SerializeField] private float checkAheadDistance = 0.45f; // 진행방향 검사 거리
    [SerializeField] private float checkDownOffset = 0.2f; // 발바닥 보정
    [SerializeField] private float checkGizmoSize = 0.07f; // 시각화용 구 크기

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

        if (Input.GetMouseButtonDown(1) || Input.GetMouseButton(1))
        {
            UpdateClickTarget();
            hasClickTarget = true;
        }
    }

    private void FixedUpdate()
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.Normal)
            return; // Normal 상태가 아니면 이동 차단 역할도 안 함

        if (!hasClickTarget)
        {
            if (isMove) rb.linearVelocity = Vector2.zero; // 정상 멈춤 처리만
            return;
        }

        Vector2 pos = rb.position;
        Vector2 dir = (clickTarget - pos);
        float dist = dir.magnitude;

        if (dist < 0.05f)
        {
            StopMove();
            return;
        }

        Vector2 dirNormalized = dir.normalized;

        // 이동 방향 앞 검사 위치 (한 칸 앞)
        Vector2 aheadPos = pos + dirNormalized * checkAheadDistance;
        aheadPos += Vector2.down * checkDownOffset;

        // 타일 검사
        Vector3Int cell = groundTilemap.WorldToCell(aheadPos);
        TileBase tileAhead = groundTilemap.GetTile(cell);

        if (tileAhead == null)
        {
            StopMove();
            Debug.Log("앞이 낭떠러지라 이동을 멈췄습니다.");
            return;
        }

        // 이동 적용
        rb.linearVelocity = dirNormalized * speed;
        isMove = true;
        OnWalk?.Invoke(true);

        // 좌우 반전 처리
        if (dir.x > 0.05f)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (dir.x < -0.05f)
            transform.localScale = new Vector3(1, 1, 1);
    }

    private void StopMove()
    {
        rb.linearVelocity = Vector2.zero;
        isMove = false;
        hasClickTarget = false;
        OnWalk?.Invoke(false);
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
        StopMove();
        clickTarget = Vector2.zero;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !hasClickTarget)
            return;

        Vector2 pos = transform.position;
        Vector2 dir = ((Vector2)clickTarget - pos).normalized;

        Vector2 aheadPos = pos + dir * checkAheadDistance;
        aheadPos += Vector2.down * checkDownOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(aheadPos, checkGizmoSize);
    }
#endif
}

