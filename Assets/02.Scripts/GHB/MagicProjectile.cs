using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicProjectile : MonoBehaviour, IProjectile
{
    [Header("적 레이어 지정")]
    [SerializeField] private LayerMask targetLayer; // 적 Layer 지정

    private float damage;
    private float duration;
    private float speed;

    private Rigidbody rb;

    // 적과 닿아있는 동안의 목록
    private readonly HashSet<IDamageable> overlappingTargets = new HashSet<IDamageable>();
    private Coroutine damageLoop;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    public void SetDuration(float duration)
    {
        this.duration = duration;
        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsTargetLayer(collision.gameObject))
            return;

        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
        {
            overlappingTargets.Add(damageable);

            // 첫 타격 즉시 데미지 1회
            damageable.TakeDamage(damage);
            Debug.Log($"{collision.name} 에게 {damage} 데미지를 줌 (첫 타격)");

            // 지속 데미지 루프 시작
            if (damageLoop == null)
                damageLoop = StartCoroutine(DamageOverTime());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null && overlappingTargets.Contains(damageable))
        {
            overlappingTargets.Remove(damageable);

            // 아무도 닿아 있지 않으면 루프 중단
            if (overlappingTargets.Count == 0 && damageLoop != null)
            {
                StopCoroutine(damageLoop);
                damageLoop = null;
            }
        }
    }

    private IEnumerator DamageOverTime()
    {
        while (overlappingTargets.Count > 0)
        {
            // HashSet 복사본을 만들어 foreach 순회
            var targets = new List<IDamageable>(overlappingTargets);
            foreach (var target in targets)
            {
                if (target != null && !target.IsDead)
                {
                    target.TakeDamage(damage);
                    Debug.Log($"{target} 에게 {damage} 지속 데미지 (0.3초 주기)");
                }
            }

            yield return new WaitForSeconds(0.3f);
        }

        damageLoop = null;
    }


    private void OnDestroy()
    {
        if (damageLoop != null)
            StopCoroutine(damageLoop);

        overlappingTargets.Clear();
    }

    // LayerMask 체크 유틸
    private bool IsTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }
}
