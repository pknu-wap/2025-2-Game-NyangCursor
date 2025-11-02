using UnityEngine;

public class TestPCollisionController : MonoBehaviour, ICollidable
{
    private Component owner;

    [Header("충돌 데이터")]
    [SerializeField] private float collisionDamage = 10f;
    [SerializeField] private float damageCooltime = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    private float lastAttackTime;

    public void Initialize(Component owner)
    {
        this.owner = owner;
        lastAttackTime = -damageCooltime; // 시작 시 즉시 공격 가능
    }

    public void OnCollisionDetected(Collision2D collision)
    {
        // Enemy 레이어 체크
        if (enemyLayer.Contains(collision.gameObject))
        {
            TryAttackEnemy(collision.gameObject);
        }
    }

    public void OnTriggerDetected(Collider2D collider)
    {
        // Enemy 레이어 체크
        if (enemyLayer.Contains(collider.gameObject))
        {
            TryAttackEnemy(collider.gameObject);
        }
    }

    private void TryAttackEnemy(GameObject target)
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerStateLogic.PlayerState.OverDrive)
            return;

        // 공격 쿨타임 체크
        if (Time.time - lastAttackTime < damageCooltime)
            return;

        // IDamageable 인터페이스를 구현한 컴포넌트 찾기
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
        {
            damageable.TakeDamage(collisionDamage);
            lastAttackTime = Time.time;
            Debug.Log($"{owner.name} 이(가) {target.name} 에게 {collisionDamage} 데미지를 입혔습니다.");
        }
    }

    public void Cleanup()
    {
        // 필요한 정리 작업
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