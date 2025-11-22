using System.Collections;
using UnityEngine;
using static PlayerStateLogic;

public class EvadeMoveController : MonoBehaviour, IMoveable
{
    private Transform target;
    private Enemy owner;
    private Rigidbody2D rb;

    private enum MovementState
    {
        Chase,          // 추적
        Attacking,      // 공격
        Evading,        // 회피
    }

    [Header("이동 관련")]
    private MovementState currentState = MovementState.Chase;   // 현재 이동 상태
    private float currentSpeedMultiplier = 1f;    //  현재 속도 배율 
    [SerializeField] private float overdriveSpeedMultiplier = 3f; // 오버드라이브 모드일 때 추가 속도 배율

    [Header("Chase 상태")]
    private float baseMoveSpeed;    // 기본 이동 속도, EnemyData에서 가져옴

    [Header("Attacking 상태")]
    [SerializeField] private float attackMoveSpeed = 0.5f;   // 공격 상태일 때 이동 속도

    [Header("Evading 상태")]
    [SerializeField] private float evadeMoveSpeed = 5f;   // 회피 상태일 때 이동 속도

    public void Initialize(Component owner)
    {
        if (owner is not Enemy enemy)
        {
            Debug.LogError("EvadeMoveController 는 Enemy 타입만 지원합니다");
            return;
        }

        this.owner = enemy;
        baseMoveSpeed = enemy.Data.moveSpeed;
        rb = GetComponent<Rigidbody2D>();

        // Rigidbody 설정
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        SubscribeEvents();
    }

    public void Cleanup()
    {
        Stop();
        UnsubscribeEvents();
    }
    private void FixedUpdate()
    {
        UpdateMovement(Time.fixedDeltaTime);
    }

    // 현재 스크립트에서 사용하는 이벤트들을 구독함
    private void SubscribeEvents()
    {
        // 로컬 이벤트 버스 관련 (= 인터페이스 구현체 간의 통신)
        owner.EventBus.Subscribe<float>(EnemyEventType.OnKnockback, PauseMovementForSeconds);
        owner.EventBus.Subscribe(EnemyEventType.OnEnterEvadeRange, StartEvade);
        owner.EventBus.Subscribe(EnemyEventType.OnExitEvadeRange, FinishEvade);
        owner.EventBus.Subscribe(EnemyEventType.OnEnterAttackRange, StartAttack);
        owner.EventBus.Subscribe(EnemyEventType.OnExitAttackRange, FinishAttack);

        // 글로벌 이벤트 버스 관련 (= 적과 외부 오브젝트 간의 통신)
        GameEvents.Subscribe(GameEventType.OnPlayerStartOverdrive, ChangeSpeed);
        GameEvents.Subscribe(GameEventType.OnPlayerFinishOverdrive, ChangeSpeed);
    }

    // 현재 스크립트에서 사용하는 이벤트들을 구독 해제함
    private void UnsubscribeEvents()
    {
        // 로컬 이벤트 버스 관련 (= 인터페이스 구현체 간의 통신)
        owner.EventBus.Unsubscribe<float>(EnemyEventType.OnKnockback, PauseMovementForSeconds);
        owner.EventBus.Unsubscribe(EnemyEventType.OnEnterEvadeRange, StartEvade);
        owner.EventBus.Unsubscribe(EnemyEventType.OnExitEvadeRange, FinishEvade);
        owner.EventBus.Unsubscribe(EnemyEventType.OnEnterAttackRange, StartAttack);
        owner.EventBus.Unsubscribe(EnemyEventType.OnExitAttackRange, FinishAttack);

        // 글로벌 이벤트 버스 관련 (= 적과 외부 오브젝트 간의 통신)
        GameEvents.Unsubscribe(GameEventType.OnPlayerStartOverdrive, ChangeSpeed);
        GameEvents.Unsubscribe(GameEventType.OnPlayerFinishOverdrive, ChangeSpeed);
    }

    // 공격할 대상을 설정하는 함수
    // EnemyManager 에서 호출됨
    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void ChangeSpeed()
    {
        var state = PlayerStateLogic.Instance.CurrentState;

        if (state == PlayerState.OverDrive)
            SetSpeedMultiplier(overdriveSpeedMultiplier);
        else
            SetSpeedMultiplier(1f);
    }

    private void StartEvade()
    {
        if (currentState == MovementState.Evading)
            return;

        currentState = MovementState.Evading;
    }

    private void FinishEvade()
    {
        if (currentState != MovementState.Evading)
            return;

        currentState = MovementState.Chase;
    }

    private void StartAttack()
    {
        if (currentState == MovementState.Attacking)
            return;

        currentState = MovementState.Attacking;
    }

    private void FinishAttack()
    {
        if (currentState != MovementState.Attacking)
            return;

        currentState = MovementState.Chase;
    }

    public void UpdateMovement(float deltaTime)
    {
        if (target == null) 
            return;

        switch (currentState)
        {
            case MovementState.Chase:
                UpdateChaseMovement(deltaTime);
                break;

            case MovementState.Attacking:
                UpdateAttackingMovement(deltaTime);
                break;

            case MovementState.Evading:
                UpdateEvadingMovement(deltaTime);
                break;
        }
    }

    private void UpdateChaseMovement(float deltaTime)
    {
        // 플레이어 방향으로 이동
        Vector3 direction = (target.position - transform.position).normalized;
        Vector2 velocity = direction * baseMoveSpeed * currentSpeedMultiplier;
        rb.linearVelocity = velocity;

        FlipSprite(direction.x);
    }

    private void UpdateAttackingMovement(float deltaTime)
    {
        // 공격 중에는 느리게 이동
        // 속도가 0 이라면 불필요한 연산을 하지 않음
        if (attackMoveSpeed > 0)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            Vector2 velocity = direction * attackMoveSpeed * currentSpeedMultiplier;
            rb.linearVelocity = velocity;

            FlipSprite(direction.x);
        }
    }

    private void UpdateEvadingMovement(float deltaTime)
    {
        // 플레이어 반대 방향으로 느리게 후퇴
        Vector3 direction = (transform.position - target.position).normalized;
        Vector2 velocity = direction * evadeMoveSpeed * currentSpeedMultiplier;
        rb.linearVelocity = velocity;

        FlipSprite(direction.x);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = multiplier;
    }

    public void PauseMovement()
    {
    }

    public void ResumeMovement()
    {
    }

    public void PauseMovementForSeconds(float seconds)
    {
        // 코루틴 시작하기 전에 비활성화 체크
        if (!gameObject.activeInHierarchy)
            return;

        StartCoroutine(PauseRoutine(seconds));
    }

    private IEnumerator PauseRoutine(float seconds)
    {
        float remainingTime = seconds;
        while (remainingTime > 0f)
        {
            // 코루틴 중에 사망하여 비활성화되면 중단
            if (!gameObject.activeInHierarchy)
                yield break;

            remainingTime -= Time.deltaTime;
            yield return null;
        }
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
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
}
