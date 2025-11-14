using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricZone : MonoBehaviour
{
    [Header("Tick 설정")]
    [SerializeField] private float tickInterval = 0.5f;

    private float damagePerTick;
    private float duration;

    private ParticleSystem ps;
    private CircleCollider2D circleCollider;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private GameObject hitEffectPrefab;

    private readonly List<IDamageable> targetsInZone = new List<IDamageable>();

    private void Awake()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
    }

    public void Init(float damage, float duration, float size)
    {
        damagePerTick = damage;
        this.duration = duration;

        ApplySize(size);
        SetupParticleSystem(duration);

        // 초기 리스트 정리
        targetsInZone.Clear();

        StartCoroutine(ZoneRoutine());
    }

    private void ApplySize(float size)
    {
        Vector3 prefabScale = Vector3.one * 0.5f; // prefab 원래 크기
        transform.localScale = prefabScale * size;
    }


    private void SetupParticleSystem(float duration)
    {
        if (ps == null) return;
        var main = ps.main;
        main.duration = duration;
        ps.Play();
    }

    private IEnumerator ZoneRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            DealTickDamage();
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        PoolManager.instance.Despawn(gameObject);
    }

    private void DealTickDamage()
    {
        // 리스트 복사본 생성
        var snapshot = new List<IDamageable>(targetsInZone);

        foreach (var dmg in snapshot)
        {
            if (dmg == null || dmg.IsDead)
            {
                targetsInZone.Remove(dmg);
                continue;
            }

            dmg.TakeDamage(damagePerTick);

            if (dmg is Component comp)
            {
                PoolManager.instance.Spawn(hitEffectPrefab, comp.transform.position);
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsTargetLayer(collision.gameObject))
            return;
        if (collision.TryGetComponent(out IDamageable dmg))
        {
            if (!dmg.IsDead && !targetsInZone.Contains(dmg))
            {
                targetsInZone.Add(dmg);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsTargetLayer(collision.gameObject))
            return;
        if (collision.TryGetComponent(out IDamageable dmg))
        {
            if (targetsInZone.Contains(dmg))
                targetsInZone.Remove(dmg);
        }
    }

    private bool IsTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }
}
