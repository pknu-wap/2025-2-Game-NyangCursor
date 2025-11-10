using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EBasicMoveController2 : MonoBehaviour, IMoveable
{
    private Transform target;
    private Enemy owner;
    private Rigidbody2D rb;
    private float moveSpeed;
    private bool isMoving = true;

    public void Initialize(Component owner)
    {
        // Enemy 타입만 허용
        if (owner is not Enemy enemy)
        {
            Debug.LogError($"[EBasicMoveController2]  Enemy 타입만 지원합니다. 현재 타입: {owner.GetType().Name}");
            return;
        }

        this.owner = enemy;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = enemy.Data.moveSpeed;

        // Rigidbody 설정
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void UpdateMovement(float fixedDeltaTime)
    {
        // 정지 상태 or 타겟 없음 → 바로 종료
        if (!isMoving || target == null)
        {
            string reason = !isMoving ? "isMoving=false" : "target==null";
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 타겟 방향 계산
        Vector2 direction = (target.position - transform.position).normalized;
        float speedMagnitude = moveSpeed;
        Vector2 velocity = direction * speedMagnitude;

        // 실제 속도 적용
        rb.linearVelocity = velocity;

        // 이동 방향으로 스프라이트 반전
        FlipSprite(direction.x);
    }

    private void FlipSprite(float directionX)
    {
        if (directionX != 0)
        {
            transform.localScale = new Vector3(
                Mathf.Sign(directionX) * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
        isMoving = true;
       
    }

    public void Stop()
    {
        //isMoving = false;
        rb.linearVelocity = Vector2.zero;
    }

    public void Cleanup()
    {
        Stop();
    }
}
