using System.Collections;
using UnityEngine;

public class ElectricLine : MonoBehaviour
{
    private float damage;
    private float duration;
    private LayerMask targetLayer;

    private GameObject hitEffectPrefab;
    public void Init(float dmg, float dur, LayerMask layer,GameObject hitEffectPrefab)
    {
        damage = dmg;
        duration = dur;
        targetLayer = layer;
        this.hitEffectPrefab = hitEffectPrefab;
        StartCoroutine(LineLife());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        {
            if ((targetLayer.value & (1 << collision.gameObject.layer)) == 0)
                return;

            IDamageable dmgable = collision.GetComponent<IDamageable>();
            if (dmgable != null && !dmgable.IsDead)
            {
                // 구체적 로그
                Debug.Log($"[ElectricLine] '{gameObject.name}' 전류가 '{collision.gameObject.name}'에게 {damage} 피해 적용");

                dmgable.TakeDamage(damage);
                PoolManager.instance.Spawn(hitEffectPrefab, collision.transform.position);
            }
        }
    }


    private IEnumerator LineLife()
    {
        yield return new WaitForSeconds(duration);
        PoolManager.instance.Despawn(gameObject);
    }
}
