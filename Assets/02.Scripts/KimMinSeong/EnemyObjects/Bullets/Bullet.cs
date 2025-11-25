using UnityEngine;

// 탄막 추상 클래스
public abstract class Bullet : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] protected float lifetime = 5f; // 지속 시간
    [SerializeField] protected LayerMask targetLayer;

    protected Rigidbody2D rb;
    protected float damage;
    protected float speed;
    protected Vector2 direction;
    protected float spawnTime;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;           // 중력 제거
        rb.linearDamping = 0f;          // 선형 저항 제거
        rb.angularDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;  // 회전 고정
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;    // 속도가 빨라 통과하는 것을 방지
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // 물리 연산 프레임과 렌더링 프레임 사이의 지연을 보정
    }

    protected virtual void OnEnable()
    {
        spawnTime = Time.time;
    }

    protected virtual void OnDisable()
    {
        damage = 0f;
        speed = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    // 총알을 발사할 때 방향, 속도, 데미지를 설정하는 메서드
    public virtual void Initialize(Vector3 direction, float speed, float damage)
    {
        this.damage = damage;
        this.speed = speed;
        this.direction = direction;
        rb.linearVelocity = direction * speed;
    }

    protected virtual void Update()
    {
        // 수명이 다 되었다면 풀로 복귀
        if (Time.time - spawnTime >= lifetime)
            PoolManager.instance.Despawn(gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // 레이어 체크 → LayerMaskHelper 확장 메서드 사용
        if (!targetLayer.Contains(collision.gameObject))
            return;

        // 충돌 처리
        Hit(collision.gameObject);
    }

    protected abstract void Hit(GameObject target);
}