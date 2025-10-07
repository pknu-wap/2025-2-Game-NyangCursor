using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EBasicMoveController : MonoBehaviour, IMoveable
{
    private Enemy owner;
    private Transform target;
    private Rigidbody2D rb;
    private float moveSpeed;
    private bool isMoving = true;

    public void Initialize(Component owner)
    {
        // Enemy 타입만 허용
        if (owner is not Enemy enemy)
        {
            Debug.LogError($"EBasicMoveController 는 Enemy 타입만 지원합니다. 현재 타입: {owner.GetType().Name}");
            return;
        }

        // 필드 초기화
        this.owner = enemy;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = enemy.Data.moveSpeed;


        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            SetTarget(player.transform);

        else
            Debug.LogError($"{enemy.name}: Player 태그를 가진 오브젝트를 찾을 수 없습니다");
    }

    public void UpdateMovement(float fixedDeltaTime)
    {
        // 정지해있거나 타겟이 없다면 스킵
        if (!isMoving || target == null)
            return;

        Vector2 direction = (target.position - transform.position).normalized;  // 플레이어 방향 계산
        Vector2 distance = direction * moveSpeed * fixedDeltaTime;   // 이동해야할 거리 계산

        // 현재 위치에서 이동
        rb.MovePosition(rb.position + distance);

        // 이동 방향으로 스프라이트 플립
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
        isMoving = false;
        rb.linearVelocity = Vector2.zero;
    }

    public void Cleanup()
    {
        Stop();
    }
}