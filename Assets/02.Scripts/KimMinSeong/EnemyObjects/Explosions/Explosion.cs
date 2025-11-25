using UnityEngine;

// 폭발 추상 클래스
public abstract class Explosion : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] protected LayerMask targetLayer;           // 타겟 레이어

    protected float damage;
    protected float explosionRadius;

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
    protected abstract void Explode();

    // 디버그용 기즈모
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}