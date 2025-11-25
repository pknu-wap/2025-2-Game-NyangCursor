using UnityEngine;

// 폭발 추상 클래스
public abstract class Explosion : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] protected float lifetime = 2f;             // 폭발 지속 시간
    [SerializeField] protected LayerMask targetLayer;           // 타겟 레이어

    protected float damage;
    protected float spawnTime;
    protected float explosionRadius;

    protected virtual void OnEnable()
    {
        spawnTime = Time.time;
    }

    protected virtual void OnDisable()
    {
        damage = 0f;
        explosionRadius = 0f;
    }

    // 폭발 이펙트를 소환할 때 데미지, 폭발 범위를 설정하는 메서드
    public virtual void Initialize(float damage, float radius)
    {
        this.damage = damage;
        this.explosionRadius = radius;
        Explode();
    }

    // 폭발 실행
    protected virtual void Explode()
    {
        // 폭발 범위 내의 타겟 감지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, targetLayer);

        foreach (var hit in hits)
        {
            if (targetLayer.Contains(hit.gameObject))
                Hit(hit.gameObject);
        }

        // 딜레이 이후 자동으로 풀로 반환
        PoolManager.instance.Despawn(this.gameObject, lifetime);
    }

    // 각 타겟에 대한 처리 (자식 클래스에서 구현)
    protected abstract void Hit(GameObject target);

    // 디버그용 기즈모
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}