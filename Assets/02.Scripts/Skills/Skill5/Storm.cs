using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storm : MonoBehaviour
{
    private float damagePerTick;
    private float duration;

    private ParticleSystem ps;
    [SerializeField] private float tickInterval;
    [SerializeField] private LayerMask targetLayer;

    private CircleCollider2D circleCollider;

    private readonly List<IDamageable> targetsInZone = new List<IDamageable>();

    private void Awake()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
    }


    public void Init(float damage, float duration, float size)
    {
        this.damagePerTick = damage;
        this.duration = duration;

        ApplySize(size);
        SetupParticleSystem(duration);

        // 초기 리스트 정리
        targetsInZone.Clear();


        StartCoroutine(StormRoutine());
    }

    private void ApplySize(float size)
    {
        Vector3 prefabScale = Vector3.one * 0.5f; // prefab 원래 크기
        transform.localScale = prefabScale * size;

        if (ps != null)
        {
            var shape = ps.shape;
            shape.radius = size * 0.5f; // shape 모듈도 맞춰줌
        }
    }

    private void SetupParticleSystem(float duration)
    {
        if (ps == null) return;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = duration;
        ps.Play();
    }

    private IEnumerator StormRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            TickEffect();
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        // duration 끝나면 pool로 반납
        PoolManager.instance.Despawn(gameObject);
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

    private void TickEffect()
    {
        for (int i = targetsInZone.Count - 1; i >= 0; i--)
        {
            var dmg = targetsInZone[i];
            if (dmg == null || dmg.IsDead)
            {
                targetsInZone.RemoveAt(i);
                continue;
            }

            dmg.TakeDamage(damagePerTick);

            if (dmg is Component comp)
            {
                // 이동 거리 = 속도 * tickInterval
                float pullPower = 5f; // 원하는 끌어오는 속도
                var pullable = comp.GetComponent<IKnockbackable>();
                if (pullable != null)
                {
                    pullable.ApplyPull(transform.position, pullPower);
                }


                Debug.Log($"TickEffect: {comp.name} pulled towards Storm!");
            }
        }
    }






    private bool IsTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }
}
