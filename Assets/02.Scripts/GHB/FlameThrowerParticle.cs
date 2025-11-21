using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDamageParticle : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("0.1이면 0.1초마다 데미지 적용")]
    public float tickRate = 0.1f;                 // Tick 간격 (초)
    [Tooltip("틱당 데미지")]
    public float damagePerTick = 3f;              // 틱당 데미지
    public float enemyExitDelay = 0.5f;           // 적이 충돌 안 되고 빠진 것으로 판단하는 시간

    private ParticleSystem particleSystemRef;
    private Dictionary<GameObject, float> enemyHitTimer = new Dictionary<GameObject, float>();

    private Coroutine damageCoroutine = null;

    void Awake()
    {
        particleSystemRef = GetComponent<ParticleSystem>();
    }

    void OnEnable()
    {
        if(damageCoroutine == null)
        {
            damageCoroutine = StartCoroutine(DamageLoop());
        }
        else
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = StartCoroutine(DamageLoop());
        }
    }

    public void SetDeactive()
    {
        enemyHitTimer.Clear();
        StopAllCoroutines();
    }

    // 파티클 충돌 이벤트
    void OnParticleCollision(GameObject other)
    {

        var enemy = other.GetComponent<IDamageable>();
        if (enemy == null)
        {
            return;
        }

        // 통합된 Add/Update
        enemyHitTimer[other] = enemyExitDelay;
    }

    IEnumerator DamageLoop()
    {

        while (true)
        {

            List<GameObject> toRemove = new List<GameObject>();

            // Dictionary 키 복사본으로 안전하게 순회
            var keys = new List<GameObject>(enemyHitTimer.Keys);

            foreach (var enemyObj in keys)
            {
                float timer = enemyHitTimer[enemyObj];

                if (enemyObj == null)
                {
                    toRemove.Add(enemyObj);
                    continue;
                }

                IDamageable enemy = enemyObj.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerTick);
                }

                timer -= tickRate;

                if (timer <= 0f)
                {
                    toRemove.Add(enemyObj);
                }
                else
                {
                    enemyHitTimer[enemyObj] = timer;
                }
            }

            // 삭제 처리
            foreach (var enemyObj in toRemove)
            {
                enemyHitTimer.Remove(enemyObj);
            }

            yield return new WaitForSeconds(tickRate);
        }
    }
}
