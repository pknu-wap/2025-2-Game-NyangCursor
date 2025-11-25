using System;
using System.Collections;
using UnityEngine;

public class NormalHpController : MonoBehaviour, IDamageable
{
    protected Enemy owner;
    protected float currentHp;
    protected float maxHp;
    protected bool isDead;
    protected float overdriveGaugeReward;

    [Header("피격 시 시각 효과")]
    protected Color originalColor = Color.white;
    protected Coroutine flashRoutine;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Color hitColor = Color.red;
    [SerializeField] protected float hitFlashDuration = 0.15f;

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

    public virtual void Initialize(Component owner)
    {
        if (owner is not Enemy enemy)
        {
            Debug.LogError($"NormalHpController는 Enemy 타입만 지원합니다. 현재 타입: {owner.GetType().Name}");
            return;
        }

        this.owner = enemy;
        maxHp = enemy.Data.maxHP;
        currentHp = maxHp;
        isDead = false;
        overdriveGaugeReward = enemy.Data.overdriveGaugeReward;
        spriteRenderer.color = originalColor;
    }

    public virtual void Cleanup()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHp -= damage;
        currentHp = Mathf.Max(0, currentHp);

        GameEvents.Publish<(float, Transform)>(GameEventType.OnEnemyTakeDamage, (damage, transform));
        TriggerHitFlash();

        if (currentHp <= 0)
            Die();
    }

    protected void TriggerHitFlash()
    {
        if (spriteRenderer != null)
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashHitEffect());
        }
    }

    protected IEnumerator FlashHitEffect()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // 로컬 이벤트 버스로 적 사망 이벤트 발행
        owner.EventBus.Publish(EnemyEventType.OnDeath);

        // 글로벌 이벤트 버스로 적 사망 이벤트 발행
        GameEvents.Publish<float>(GameEventType.OnEnemyDeath, overdriveGaugeReward);
    }
}