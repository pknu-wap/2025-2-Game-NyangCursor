using System;
using System.Collections;
using UnityEngine;
using static PlayerStateLogic;

[RequireComponent(typeof(Rigidbody2D))]
public class EBasicMoveController2 : MonoBehaviour, IMoveable
{
    private Transform target;     // 추적 대상(보통 플레이어)
    private Enemy owner;          // Enemy 루트 객체
    private Rigidbody2D rb;       // 적 이동 물리 처리
    private float moveSpeed;      // EnemyData 에서 받아오는 기본 이동 속도

    private float baseMoveSpeed; // 

    // ===========================================================
    // 이동 일시정지(스턴/빙결/넉백 등)를 총괄하는 pauseCount 시스템
    // - pauseCount > 0   → 이동 정지
    // - pauseCount <= 0  → 이동 가능
    // 복수 효과가 동시에 걸려도 정상 동작!
    // ===========================================================
    private int pauseCount = 0;
    private bool IsPaused => pauseCount > 0;

    //private void OnEnable()
    //{
    //    //GaugeOverdriveLogic.OnGetOffEvent += ChangeSpeed;
    //    //GaugeRidingLogic.OnOverDriveEvent += ChangeSpeed;

    //    // 글로벌 이벤트 버스 관련 (= 적과 외부 오브젝트 간의 통신)
    //    GameEvents.Subscribe(GameEventType.OnPlayerStartOverdrive, ChangeSpeed);
    //    GameEvents.Subscribe(GameEventType.OnPlayerFinishOverdrive, ChangeSpeed);
    //}

    //private void OnDisable()
    //{
    //    //GaugeOverdriveLogic.OnGetOffEvent -= ChangeSpeed;
    //    //GaugeRidingLogic.OnOverDriveEvent -= ChangeSpeed;

    //    // 글로벌 이벤트 버스 관련 (= 적과 외부 오브젝트 간의 통신)
    //    GameEvents.Subscribe(GameEventType.OnPlayerStartOverdrive, ChangeSpeed);
    //    GameEvents.Subscribe(GameEventType.OnPlayerFinishOverdrive, ChangeSpeed);
    //}

    private void FixedUpdate()
    {
        UpdateMovement(Time.fixedDeltaTime);
    }

    public void Initialize(Component owner)
    {
        if (owner is not Enemy enemy)
        {
            Debug.LogError($"[EBasicMoveController2] Enemy 타입만 지원합니다. 현재 타입: {owner.GetType().Name}");
            return;
        }

        // 풀에서 꺼낼 때도 초기화
        pauseCount = 0;

        this.owner = enemy;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = enemy.Data.moveSpeed;

        baseMoveSpeed = enemy.Data.moveSpeed; // 원래 속도 캐싱

        // Rigidbody 설정
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        rb.interpolation = RigidbodyInterpolation2D.Interpolate; //달달거림 해결

        SubscribeEvents();
    }

    public void Cleanup()
    {
        pauseCount = 0;
        Stop();
        UnsubscribeEvents();
    }

    // 현재 스크립트에서 사용하는 이벤트들을 구독함
    private void SubscribeEvents()
    {
        // 로컬 이벤트 버스 관련 (= 인터페이스 구현체 간의 통신)
        owner.EventBus.Subscribe<float>(EnemyEventType.OnKnockback, PauseMovementForSeconds);

        // 글로벌 이벤트 버스 관련 (= 적과 외부 오브젝트 간의 통신)
        GameEvents.Subscribe(GameEventType.OnPlayerStartOverdrive, ChangeSpeed);
        GameEvents.Subscribe(GameEventType.OnPlayerFinishOverdrive, ChangeSpeed);
    }

    // 현재 스크립트에서 사용하는 이벤트들을 구독 해제함
    private void UnsubscribeEvents()
    {
        owner.EventBus.Unsubscribe<float>(EnemyEventType.OnKnockback, PauseMovementForSeconds);

        GameEvents.Unsubscribe(GameEventType.OnPlayerStartOverdrive, ChangeSpeed);
        GameEvents.Unsubscribe(GameEventType.OnPlayerFinishOverdrive, ChangeSpeed);
    }

    public void ChangeSpeed()
    {
        var state = PlayerStateLogic.Instance.CurrentState;

        if (state == PlayerState.GetOff)
        {
            moveSpeed = baseMoveSpeed;
        }
        else if (state == PlayerState.Normal)
        {
            moveSpeed = baseMoveSpeed;
        }
        else if (state == PlayerState.OverDrive)
        {
            moveSpeed = baseMoveSpeed * 3f;
        }
    }

    public void UpdateMovement(float fixedDeltaTime)
    {
        if (IsPaused || target == null)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, fixedDeltaTime * 5f);
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;
        Vector2 velocity = direction * moveSpeed;

        rb.linearVelocity = velocity;

        FlipSprite(direction.x);
    }

    private void FlipSprite(float dirX)
    {
        if (dirX != 0)
        {
            transform.localScale = new Vector3(
                Mathf.Sign(dirX) * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    // 공격할 대상을 설정하는 함수
    // EnemyManager 에서 호출됨
    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    // ============================================
    // 이동 일시정지 시스템
    // ============================================

    // 이동 정지 요청 (넉백/스턴/빙결 등)
    public void PauseMovement()
    {
        pauseCount++;
    }

    // 이동 정지 해제
    public void ResumeMovement()
    {
        pauseCount = Mathf.Max(0, pauseCount - 1);
    }

    // -----------------------------
    // 일정 시간 멈추기 (코루틴 안전 추가!)
    // -----------------------------
    public void PauseMovementForSeconds(float seconds)
    {
        // 코루틴 시작하기 전에 비활성화 체크
        if (!gameObject.activeInHierarchy)
            return;

        StartCoroutine(PauseRoutine(seconds));
    }

    private IEnumerator PauseRoutine(float seconds)
    {
        PauseMovement();

        float t = seconds;
        while (t > 0f)
        {
            // ❗ 도중에 비활성화되면 즉시 종료 (코루틴 중단)
            if (!gameObject.activeInHierarchy)
                yield break;

            t -= Time.deltaTime;
            yield return null;
        }

        ResumeMovement();
    }

    // 즉시 정지
    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }
}
