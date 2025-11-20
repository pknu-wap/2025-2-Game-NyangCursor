using System;
using UnityEngine;

public class BulletColliderManager : MonoBehaviour
{
    [Header("총알 데미지 설정")]
    [SerializeField] private float damage = 10f;

    [Header("총알 유지시간")]
    [SerializeField] private float lifeTime = 3f;
    
    [Header("넉백 파워")]
    [SerializeField] private float knockbackPower = 5f;

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

        // ==========================
        // 🔥 전방 사각형 넉백
        // ==========================
        float boxWidth = 6f;     // 좌우 폭
        float boxHeight = 3f;    // 앞쪽 범위
        LayerMask enemyLayer = LayerMask.GetMask("Enemy");

        // 총알 방향
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 bulletDir = rb.linearVelocity.normalized;

        // 박스 중심 = 맞은 적 위치 + 총알 방향 * (boxHeight / 2)
        Vector2 boxCenter = hitPoint + bulletDir * (boxHeight * 0.5f);

        // 회전값: 박스가 총알 방향을 따라가도록 회전
        float angle = Mathf.Atan2(bulletDir.y, bulletDir.x) * Mathf.Rad2Deg;

        // 사각형 영역 내 적들 찾기
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            boxCenter,
            new Vector2(boxWidth, boxHeight),
            angle,
            enemyLayer
        );

        // fakeSource는 여전히 총알 뒤쪽
        Vector2 fakeSource = (Vector2)transform.position - bulletDir * 999f;

        // 감지된 적들에게 넉백
        foreach (var col in hits)
        {
            var enemyKnock = col.GetComponent<IKnockbackable>();
            if (enemyKnock != null)
                enemyKnock.ApplyKnockbackWithScatter(fakeSource, knockbackPower, 0.4f,0);


            PlayImpactEffect(hitPoint);
           
        }
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
