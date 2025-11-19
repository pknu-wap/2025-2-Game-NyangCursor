using System;
using UnityEngine;

public class BulletColliderManager : MonoBehaviour
{
    [Header("총알 데미지 설정")]
    [SerializeField] private float damage = 10f;

    [Header("총알 유지시간")]
    [SerializeField] private float lifeTime = 3f;

    [Header("적 레이어 지정")]
    [SerializeField] private LayerMask targetLayer;

    [Header("총알을 맞췄을 때 게이지 증가량")]
    [SerializeField] private float gaugeIncreaseAmount = 10f;

    [Header("충돌 이펙트 (Pool 등록 )")]
    public GameObject impactParticle;  // 풀 프리팹

    private Rigidbody2D rb;

    public static event Action<float> OnBulletHit;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();

        // 총알 수명 타이머
        CancelInvoke();
        Invoke(nameof(DespawnSelf), lifeTime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!targetLayer.Contains(collision.gameObject))
            return;

        HitTarget(collision.gameObject, collision.ClosestPoint(transform.position));
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!targetLayer.Contains(collision.gameObject))
            return;

        HitTarget(collision.gameObject, collision.contacts[0].point);
    }

    private void HitTarget(GameObject hitObj, Vector2 hitPoint)
    {
        IDamageable damageable = hitObj.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
        {
            damageable.TakeDamage(damage);
            OnBulletHit?.Invoke(gaugeIncreaseAmount);
        }

        PlayImpactEffect(hitPoint);
        DespawnSelf();
    }

    //피격이펙트
    private void PlayImpactEffect(Vector2 pos)
    {
        if (impactParticle == null) return;

        // impactParticle을 PoolManager에서 꺼낸다
        GameObject fx = PoolManager.instance.Spawn(impactParticle, pos);

        // 2초 뒤 풀로 반환
        PoolManager.instance.Despawn(fx,2f);
    }

    private void DespawnSelf()
    {
        PoolManager.instance.Despawn(gameObject);
    }

    //총알 발사방향 업데이트
    void FixedUpdate()
    {
        if (rb == null) return;

        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
