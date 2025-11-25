using UnityEngine;

public class EvadeMoveController : NormalMoveController
{
    private enum MovementState
    {
        Chase,          // 추적
        Attacking,      // 공격
        Evading,        // 회피
    }

    [Header("상태 관리")]
    private MovementState currentState = MovementState.Chase;   // 현재 이동 상태

    [Header("Attacking 상태")]
    [SerializeField] private float attackMoveSpeed = 0.5f;   // 공격 상태일 때 이동 속도

    [Header("Evading 상태")]
    [SerializeField] private float evadeMoveSpeed = 5f;   // 회피 상태일 때 이동 속도

    protected override void SubscribeEvents()
    {
        // 부모 클래스의 기본 이벤트 구독
        base.SubscribeEvents();

        // EvadeMoveController 전용 이벤트 추가 구독
        owner.EventBus.Subscribe(EnemyEventType.OnEnterEvadeRange, StartEvade);
        owner.EventBus.Subscribe(EnemyEventType.OnExitEvadeRange, FinishEvade);
        owner.EventBus.Subscribe(EnemyEventType.OnEnterAttackRange, StartAttack);
        owner.EventBus.Subscribe(EnemyEventType.OnExitAttackRange, FinishAttack);
    }

    protected override void UnsubscribeEvents()
    {
        // EvadeMoveController 전용 이벤트 구독 해제
        owner.EventBus.Unsubscribe(EnemyEventType.OnEnterEvadeRange, StartEvade);
        owner.EventBus.Unsubscribe(EnemyEventType.OnExitEvadeRange, FinishEvade);
        owner.EventBus.Unsubscribe(EnemyEventType.OnEnterAttackRange, StartAttack);
        owner.EventBus.Unsubscribe(EnemyEventType.OnExitAttackRange, FinishAttack);

        // 부모 클래스의 기본 이벤트 구독 해제
        base.UnsubscribeEvents();
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

    public override void UpdateMovement(float deltaTime)
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
}