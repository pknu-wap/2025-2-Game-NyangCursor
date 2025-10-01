using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class ClickModController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;        // 메인 카메라s
    [SerializeField] private Rigidbody2D rb;    // 이동용 Rigidbody2D
    [SerializeField] private Animator animator; // 애니메이터
    [SerializeField] private CursorController cursorController;
    [SerializeField] private GameObject overDrivePlayer;
    [SerializeField] private GameObject walkPlayer;

    [Header("Settings")]
    [SerializeField] private float speed = 5f;  // 이동 속도
    public bool isClickMode = true; // 클릭 모드 활성화 여부

    // 내부 상태
    private Vector2 clickTarget;    // 클릭한 목표 지점
    private bool hasClickTarget;    // 목표 지점 존재 여부
    private bool isMove;            // 이동 중 여부

    private Vector3 camVel;
    [SerializeField] private Vector3 camOffset = new Vector3(0, 0, -10);
    [SerializeField] private float camSmooth = 0.15f;


    private void Reset()
    {
        // 컴포넌트 자동 할당 (에디터에서 Reset 버튼 누르면 잡힘)
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        cursorController = GetComponent<CursorController>();
        if (cam == null) cam = Camera.main;
    }

    private void Start()
    {
        // 참조 누락 시 자동 할당 시도
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (cam == null) cam = Camera.main;
    }


    private void Update()
    {
        // ✅ 클릭 모드 처리
        if (isClickMode)
        {
            HandleClickMode();

            cursorController.enabled = false;
            overDrivePlayer.SetActive(false);

            walkPlayer.SetActive(true);
        }
        else if(isClickMode == false)
        {
            cursorController.enabled = true;
            overDrivePlayer.SetActive(true);

            walkPlayer.SetActive(false);
        }

    }
    private void FixedUpdate()
    {
        if (isClickMode)
        {
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
                }
                else
                {
                    Vector2 step = dir.normalized * speed;
                    rb.MovePosition(pos + step * Time.fixedDeltaTime);
                    isMove = true;
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

                // 애니메이터 반영
                animator.SetBool("isMove", isMove);
            }
            return; // 기존 이동 차단
        }
    }

    private void LateUpdate()
    {
        if (!isClickMode) return;
        Vector3 targetPos = (Vector3)rb.position + camOffset;
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, targetPos, ref camVel, camSmooth);
    }


    private void HandleClickMode()
    {
        // 우클릭 입력 → 목표 지점 설정
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 m = Input.mousePosition;
            m.z = Mathf.Abs(cam.transform.position.z);
            clickTarget = cam.ScreenToWorldPoint(m);
            hasClickTarget = true;
        }

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
            }
            else
            {
                Vector2 step = dir.normalized * speed;
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = step;
#else
                rb.velocity = step;
#endif
                isMove = true;
            }
        }
        else
        {
            isMove = false; // 클릭 타겟이 없을 때
        }

        // 애니메이터에 값 전달
        if (animator != null)
            animator.SetBool("isMove", isMove);
    }
}
