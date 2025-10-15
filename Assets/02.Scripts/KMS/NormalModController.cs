using System;
using UnityEngine;
using static PlayerStateLogic;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class NormalModController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;        // 메인 카메라
    [SerializeField] private Rigidbody2D rb;    // 이동용 Rigidbody2D


    [Header("Settings")]
    [SerializeField] private float speed = 5f;  // 이동 속도

      // 내부 상태
    private Vector2 clickTarget;    // 클릭한 목표 지점
    private bool hasClickTarget;    // 목표 지점 존재 여부
    private bool isMove;            // 이동 중 여부
    private Vector3 camVel;
    [SerializeField] private Vector3 camOffset = new Vector3(0, 0, -10);
    [SerializeField] private float camSmooth = 0.15f;

    //이벤트
    public static event Action<bool> OnWalk;


    private void Awake()
    {
        // 참조 누락 시 자동 할당 시도
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

        print("클릭함!!!");
            
        speed = PlayerStatsManager.instance.GetStat(StatType.MoveSpeedUp);
        // 우클릭 입력 → 목표 지점 설정
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 m = Input.mousePosition;
            m.z = Mathf.Abs(cam.transform.position.z);
            clickTarget = cam.ScreenToWorldPoint(m);
            hasClickTarget = true;
            
        }


    }
    private void FixedUpdate()
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.Normal)
            return;

        if (hasClickTarget)
            {
                Vector2 pos = rb.position;
                Vector2 dir = (clickTarget - pos);
                float dist = dir.magnitude;

                if (dist < 0.05f) // 도착 판정
                {
                    rb.linearVelocity = Vector2.zero;
                    hasClickTarget = false;
                    isMove = false;
                    OnWalk?.Invoke(isMove);
                }
                else
                {
                    Vector2 step = dir.normalized * speed;
                    rb.MovePosition(pos + step * Time.fixedDeltaTime);
                    isMove = true;
                    OnWalk?.Invoke(isMove);
                }

                // 이동 방향 확인 (좌우 반전)
                if (dir.x > 0.05f)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }
                else if (dir.x < -0.05f)
                {
                    transform.localScale = new Vector3(1, 1, 1);
                }
            }
         
        
    }

    private void LateUpdate()
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.Normal)
            return;
        //카메라 이동
        Vector3 targetPos = (Vector3)rb.position + camOffset;
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, targetPos, ref camVel, camSmooth);
    }

    public void HandleResetNormal()
    {
       //노말 모드 진입 시 초기화
        hasClickTarget = false;
        isMove = false;
        clickTarget = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        // 걷기 이벤트도 false로 알림
        OnWalk?.Invoke(false);
    }


}
