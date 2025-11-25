using System.Collections;
using UnityEngine;
using static PlayerStateLogic;

public class NormalMoveController : MonoBehaviour, IMoveable
{
    protected Transform target;
    protected Enemy owner;
    protected Rigidbody2D rb;

    [Header("이동 관련")]
    protected float baseMoveSpeed;    // 기본 이동 속도, EnemyData에서 가져옴
    protected float currentSpeedMultiplier = 1f;    //  현재 속도 배율 
    [SerializeField] protected float overdriveSpeedMultiplier = 3f; // 오버드라이브 모드일 때 추가 속도 배율

    // 구버전 이동 일시정지 시스템 (리팩토링 예정)
    protected int pauseCount = 0;
    public bool IsPaused => pauseCount > 0;

    public virtual void Initialize(Component owner)
    {
        if (owner is not Enemy enemy)
        {
            Debug.LogError("MoveController 는 Enemy 타입만 지원합니다");
            return;
        }

        this.owner = enemy;
        baseMoveSpeed = enemy.Data.moveSpeed;
        rb = GetComponent<Rigidbody2D>();

        pauseCount = 0;

        // Rigidbody 설정
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        SubscribeEvents();
    }

    public virtual void Cleanup()
    {
        Stop();
        UnsubscribeEvents();
    }

    protected virtual void FixedUpdate()
    {
        UpdateMovement(Time.fixedDeltaTime);
    }

    // 현재 스크립트에서 사용하는 이벤트들을 구독함
    protected virtual void SubscribeEvents()
    {
        // 로컬 이벤트 버스 관련 (= 인터페이스 구현체 간의 통신)
        owner.EventBus.Subscribe<float>(EnemyEventType.OnKnockback, PauseMovementForSeconds);

        // 글로벌 이벤트 버스 관련 (= 적과 외부 오브젝트 간의 통신)
        GameEvents.Subscribe(GameEventType.OnPlayerStartOverdrive, ChangeSpeed);
        GameEvents.Subscribe(GameEventType.OnPlayerFinishOverdrive, ChangeSpeed);
    }

    // 현재 스크립트에서 사용하는 이벤트들을 구독 해제함
    protected virtual void UnsubscribeEvents()
    {
        // 로컬 이벤트 버스 관련 (= 인터페이스 구현체 간의 통신)
        owner.EventBus.Unsubscribe<float>(EnemyEventType.OnKnockback, PauseMovementForSeconds);

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

    public virtual void UpdateMovement(float fixedDeltaTime)
    {
        if (target == null || IsPaused)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, fixedDeltaTime * 5f);
            return;
        }

        // 플레이어를 향해 이동
        Vector3 direction = (target.position - transform.position).normalized;
        Vector2 velocity = direction * baseMoveSpeed * currentSpeedMultiplier;
        rb.linearVelocity = velocity;

        FlipSprite(direction.x);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = multiplier;
    }

    public void PauseMovement()
    {
        pauseCount++;
    }

    public void ResumeMovement()
    {
        pauseCount = Mathf.Max(0, pauseCount - 1);
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
        PauseMovement();

        float remainingTime = seconds;
        while (remainingTime > 0f)
        {
            // 코루틴 중에 사망하여 비활성화되면 중단
            if (!gameObject.activeInHierarchy)
                yield break;

            remainingTime -= Time.deltaTime;
            yield return null;
        }

        ResumeMovement();
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }

    protected void FlipSprite(float dirX)
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