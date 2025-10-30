using UnityEngine;

public class BulletColliderManager : MonoBehaviour
{
    [Header("총알 데미지 설정")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifeTime = 3f; // 일정 시간 후 자동 제거
    [SerializeField] private LayerMask targetLayer; // 적 Layer 지정

    private void OnEnable()
    {
        // 총알이 활성화될 때 수명 타이머 시작
        CancelInvoke();
        Invoke(nameof(DespawnSelf), lifeTime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 대상 레이어가 맞지 않으면 무시
        if (!targetLayer.Contains(collision.gameObject))
            return;

        // IDamageable 인터페이스 찾기
        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
        {
            damageable.TakeDamage(damage);
            Debug.Log($"{collision.name} 에게 {damage} 데미지를 줌");
        }

        // 맞추면 풀로 반환
        DespawnSelf();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 혹시 Trigger가 아닌 Collider로 충돌했을 때도 처리
        if (!targetLayer.Contains(collision.gameObject))
            return;

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
        {
            damageable.TakeDamage(damage);
            Debug.Log($"{collision.gameObject.name} 에게 {damage} 데미지를 줌");
        }

        DespawnSelf();
    }

    private void DespawnSelf()
    {
        PoolManager.instance.Despawn(gameObject);
    }
}
