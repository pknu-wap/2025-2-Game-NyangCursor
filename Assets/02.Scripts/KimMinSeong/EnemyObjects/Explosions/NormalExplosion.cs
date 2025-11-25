using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class NormalExplosion : Explosion
{
    [Header("파티클 관련")]
    [SerializeField] private GameObject explosionParticle; // 폭발 파티클

    protected override void Explode()
    {
        // 폭발 파티클 생성
        GameObject explosion = PoolManager.instance.Spawn(explosionParticle, transform.position);
        StartCoroutine(DespawnParticle(explosion));  // 파티클 시스템이 끝난 후 풀로 복귀

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
    private IEnumerator DespawnParticle(GameObject particleObject)
    {
        ParticleSystem ps = particleObject.GetComponent<ParticleSystem>();

        // ParticleSystem 이 재생 중일 때까지 대기
        while (ps != null && ps.isPlaying)
            yield return null;

        // 모든 파티클이 완전히 사라질 때까지 추가적으로 대기
        yield return new WaitForSeconds(ps.main.startLifetime.constantMax);

        // 풀로 복귀
        PoolManager.instance.Despawn(particleObject);
    }
}