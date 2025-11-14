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
        // 리스트 기반 반복 → OverlapCircleAll보다 훨씬 가벼움
        for (int i = targetsInZone.Count - 1; i >= 0; i--)
        {
            if (targetsInZone[i] == null || targetsInZone[i].IsDead)
            {
                targetsInZone.RemoveAt(i);
                continue;
            }

            targetsInZone[i].TakeDamage(damagePerTick);
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
