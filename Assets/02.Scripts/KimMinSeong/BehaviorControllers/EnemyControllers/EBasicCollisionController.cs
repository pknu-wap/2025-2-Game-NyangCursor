using UnityEngine;

public class EBasicCollisionController : MonoBehaviour, ICollidable
{
    [Header("=== 충돌 처리에 필요한 데이터 ===")]
    [SerializeField] private float collisionDamage = 10f;
    [SerializeField] private float damageCoolTime = 0.5f;
    [SerializeField] private LayerMask damageableLayer;

    // 내부적으로 사용하는 변수
    private Enemy owner;
    private float lastDamageTime;

    public void Initialize(Component owner)
    {
        // Enemy 타입만 허용
        if (owner is not Enemy enemy)
        {
            Debug.LogError($"EBasicCollisionController 는 Enemy 타입만 지원합니다. 현재 타입: {owner.GetType().Name}");
            return;
        }

        this.owner = enemy;
        lastDamageTime = -damageCoolTime; // 시작 시 즉시 데미지 가능
    }

    public void OnCollisionDetected(Collision2D collision)
    {
        // LayerMaskHelper 확장 메서드 사용
        if (damageableLayer.Contains(collision.gameObject))
            TryDealDamage(collision.gameObject);
    }

    public void OnTriggerDetected(Collider2D collider)
    {
        // LayerMaskHelper 확장 메서드 사용
        if (damageableLayer.Contains(collider.gameObject))
            TryDealDamage(collider.gameObject);
    }

    private void TryDealDamage(GameObject target)
    {
        // 쿨타임 체크
        if (Time.time - lastDamageTime < damageCoolTime)
            return;

        // IDamageable 인터페이스를 구현한 컴포넌트 찾기
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
        {
            damageable.TakeDamage(collisionDamage);
            lastDamageTime = Time.time;
            Debug.Log($"{owner.name} 이(가) {target.name} 에게 {collisionDamage} 데미지를 입혔습니다.");
        }
    }

    public void Cleanup()
    {
        // 필요시 수정
    }

    // Unity의 충돌 이벤트를 ICollidable로 전달
    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionDetected(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        OnCollisionDetected(collision);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        OnTriggerDetected(collider);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        OnTriggerDetected(collider);
    }
}