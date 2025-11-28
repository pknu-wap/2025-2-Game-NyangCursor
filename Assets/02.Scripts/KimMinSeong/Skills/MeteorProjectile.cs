using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorProjectile : MonoBehaviour
{
    [Header("적 레이어 지정")]
    [SerializeField] private LayerMask targetLayer;

    [Header("메테오 설정")]
    [SerializeField] private float explosionRadius = 2f; // 폭발 반경

    [Header("파티클 관련")]
    [SerializeField] private GameObject explosionParticle; // 폭발 파티클

    private float damage;

    public void Initialize(float damage, Vector3 targetPos)
    {
        this.damage = damage;
        Explode();
    }

    private void OnDisable()
    {
        damage = 0;
    }

    private void Explode()
    {
        // 폭발 파티클 생성
        GameObject explosion = PoolManager.instance.Spawn(explosionParticle, transform.position);
        StartCoroutine(DespawnParticle(explosion));

        // 폭발 범위 내의 타겟 감지
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, explosionRadius, targetLayer);

        foreach (var target in targets)
        {
            if (targetLayer.Contains(target.gameObject))
            {
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable != null && !damageable.IsDead)
                    damageable.TakeDamage(damage);
            }
        }

        // 풀로 복귀
        PoolManager.instance.Despawn(this.gameObject);
    }

    // 파티클 시스템이 끝날 때까지 대기한 후 풀로 복귀하는 코루틴
    private IEnumerator DespawnParticle(GameObject particle)
    {
        ParticleSystem ps = particle.GetComponent<ParticleSystem>();

        // ParticleSystem이 재생 중일 때까지 대기
        while (ps != null && ps.isPlaying)
            yield return null;

        // 모든 파티클이 완전히 사라질 때까지 추가적으로 대기
        yield return new WaitForSeconds(ps.main.startLifetime.constantMax);

        // 풀로 복귀
        PoolManager.instance.Despawn(particle);
    }

    // 디버그용 기즈모
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}