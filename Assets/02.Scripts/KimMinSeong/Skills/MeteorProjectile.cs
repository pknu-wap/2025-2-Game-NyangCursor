using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorProjectile : MonoBehaviour, IProjectile
{
    [Header("적 레이어 지정")]
    [SerializeField] private LayerMask targetLayer;

    [Header("메테오 설정")]
    [SerializeField] private float explosionDuration = 1f; // 폭발 지속 시간

    private float damage;
    private Coroutine explosionCoroutine;
    private HashSet<GameObject> damagedEnemies = new HashSet<GameObject>(); // 중복 피해 방지

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    public void SetDuration(float duration)
    {
    }

    public void Initialize(float damage, Vector3 targetPos)
    {
        // 데미지 설정
        SetDamage(damage);

        // 중복 피해 방지용 리스트 초기화
        damagedEnemies.Clear();

        // 폭발 코루틴 시작
        if (explosionCoroutine != null)
        {
            StopCoroutine(explosionCoroutine);
        }
        explosionCoroutine = StartCoroutine(ExplodeCoroutine());
    }

    private void OnDisable()
    {
        damage = 0;
        damagedEnemies.Clear();

        if (explosionCoroutine != null)
        {
            StopCoroutine(explosionCoroutine);
            explosionCoroutine = null;
        }
    }

    private IEnumerator ExplodeCoroutine()
    {
        // 폭발 이펙트가 재생되는 동안 대기
        yield return new WaitForSeconds(explosionDuration);

        // 풀로 반환
        PoolManager.instance.Despawn(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // LayerMaskHelper를 사용하여 targetLayer에 포함되는지 확인
        if (!targetLayer.Contains(other.gameObject.layer))
            return;

        // 이미 피해를 입힌 적이라면 스킵
        if (damagedEnemies.Contains(other.gameObject))
            return;

        // IDamageable 인터페이스로 데미지 처리
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
        {
            damageable.TakeDamage(damage);
            damagedEnemies.Add(other.gameObject);

            Debug.Log($"메테오 폭발! 대상: {other.name}, 데미지: {damage}");
        }
    }
}