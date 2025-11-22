using System.Collections;
using UnityEngine;

public class NormalBullet : Bullet
{
    [Header("파티클 관련")]
    [SerializeField] private GameObject impactParticle; // 폭발 파티클
    [SerializeField] private GameObject projectileParticle; // 탄막 파티클
    [SerializeField] private GameObject muzzleParticle; // 발사 파티클

    // 폭발 파티클을 생성할 때 파묻히는 현상을 방지하기 위한 오프셋
    [SerializeField] private float impactParticleOffset = 0.15f;

    public override void Initialize(Vector3 direction, float speed, float damage)
    {
        base.Initialize(direction, speed, damage);

        // 1. 발사 파티클 생성
        GameObject muzzle = PoolManager.instance.Spawn(muzzleParticle, transform.position);
        StartCoroutine(DespawnParticle(muzzle));  // 파티클 시스템이 끝난 후 풀로 복귀

        // 2. 탄막 파티클 생성
        GameObject projectile = PoolManager.instance.Spawn(projectileParticle, transform.position);
        projectile.transform.SetParent(transform);         // 탄막 프리팹의 자식으로 설정
        projectile.transform.localPosition = Vector3.zero; // 탄막 중심에 위치하도록 설정
    }

    public override void Cleanup()
    {
        base.Cleanup();
    }

    protected override void Hit(GameObject target)
    {
        // 1. 폭발 방향을 계산 → 적 방향과 반대 방향
        Vector3 hitPoint = (transform.position - target.transform.position).normalized;

        // 2. 폭발 파티클 생성 위치 및 회전을 계산
        Vector3 impactPosition = transform.position + (hitPoint * impactParticleOffset);
        Quaternion impactRotation = Quaternion.FromToRotation(Vector3.up, hitPoint);

        // 3. 폭발 파티클 생성
        GameObject impact = PoolManager.instance.Spawn(impactParticle, impactPosition);
        impact.transform.rotation = impactRotation;
        StartCoroutine(DespawnParticle(impact));  // 파티클 시스템이 끝난 후 풀로 복귀

        // 4. 데미지 처리
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null || damageable.IsDead)
            return;

        damageable.TakeDamage(damage);

        // 내부 변수 초기화 및 풀로 복귀
        Cleanup();
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
