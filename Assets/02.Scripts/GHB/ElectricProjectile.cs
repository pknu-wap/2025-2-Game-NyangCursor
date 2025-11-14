using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ElectricProjectile : MonoBehaviour, IProjectile
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject electricZone;
    [SerializeField] private LayerMask targetLayer;

    private float damage;
    private float duration;
    private float size;
    public float effectZoneDuration;

    private Coroutine durationCoroutine;

    private void OnEnable()
    {
        if (durationCoroutine != null)
        {
            StopCoroutine(durationCoroutine);
        }
    }

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    public void SetDuration(float d)
    {
        duration = d;

        // 지속시간 끝나면 폭발
        durationCoroutine = StartCoroutine(DespawnAfterDuration());
    }

    public void SetSize(float s)
    {
        size = s;

        // Transform 크기
        transform.localScale = Vector3.one * size;
    }

    private IEnumerator DespawnAfterDuration()
    {
        yield return new WaitForSeconds(duration);
        Explode();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsTargetLayer(collision.gameObject))
            return;

        IDamageable damageable = collision.GetComponent<IDamageable>();

        if (damageable != null && !damageable.IsDead)
        {
            damageable.TakeDamage(damage);
        }

        Explode();
    }

    private bool IsTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }
    private void Explode()
    {
        // 폭발 prefab 생성 (실제 인스턴스)
        PoolManager.instance.Spawn(explosionPrefab, transform.position);
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, size * 4f, targetLayer);
        foreach (var enemy in enemies)
        {
            IDamageable dmg = enemy.GetComponent<IDamageable>();
            if (dmg != null && !dmg.IsDead)
            {
                Debug.Log("범위공격으로 데미지");
                dmg.TakeDamage(damage);
            }
        }
        GameObject zoneObj = PoolManager.instance.Spawn(electricZone, transform.position);
        ElectricZone zone = zoneObj.GetComponent<ElectricZone>();
        zone.Init(damage / 5, effectZoneDuration, size);

        // projectile 반납
        PoolManager.instance.Despawn(gameObject);
    }
}
