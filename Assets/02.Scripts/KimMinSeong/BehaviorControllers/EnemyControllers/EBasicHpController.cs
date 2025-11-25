using System;
using UnityEngine;

public class EBasicHpController : MonoBehaviour, IDamageable
{
    private Enemy owner;
    private float currentHp;
    private float maxHp;
    private bool isDead;
    private float overdriveGaugeReward;

    [Header("피격 시 시각 효과")]
    private Color originalColor = Color.white;
    private Coroutine flashRoutine;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.15f;

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

    public void Initialize(Component owner)
    {
        if (owner is not Enemy enemy)
        {
            Debug.LogError($"EBasicHpController 는 Enemy 타입만 지원합니다. 현재 타입: {owner.GetType().Name}");
            return;
        }

        this.owner = enemy;
        maxHp = enemy.Data.maxHP;
        currentHp = maxHp;
        isDead = false;
        overdriveGaugeReward = enemy.Data.overdriveGaugeReward;

        spriteRenderer.color = originalColor;
    }

    public void Cleanup() {
        // 필요시 정리 작업
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

        // 글로벌 이벤트 버스로 이벤트 발행 → 데미지 텍스트 시스템에서 사용
        GameEvents.Publish<(float, Transform)>(GameEventType.OnEnemyTakeDamage, (damage, transform));

        TriggerHitFlash();

        if (currentHp <= 0)
            Die();
    }

    private void TriggerHitFlash()
    {
        if (spriteRenderer != null)
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashHitEffect());
        }
    }

    private System.Collections.IEnumerator FlashHitEffect()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // 로컬 이벤트 버스로 적 사망 이벤트 발행 → 풀링, 드랍 처리
        owner.EventBus.Publish(EnemyEventType.OnDeath);

        // 글로벌 이벤트 버스로 적 사망 이벤트 발행 → GauageOverdriveLogic 에서 게이지 증가 처리
        GameEvents.Publish<float>(GameEventType.OnEnemyDeath, overdriveGaugeReward);
    }

    #region 레거시 코드
    // 

    //// 몸통박치기 전용 데미지 처리 메서드
    //public void TakeCollisionDamage(float damage)
    //{
    //    if (isDead)
    //        return;

    //    // 몸박 무적시간 적용
    //    if (Time.time - lastCollisionDamageTime < collisionDamageInterval)
    //        return;

    //    // 쿨타임 존재
    //    lastCollisionDamageTime = Time.time;

    //    currentHp -= damage;
    //    currentHp = Mathf.Max(0, currentHp);

    //    TriggerHitFlash();

    //    // 글로벌 이벤트 버스로 이벤트 발행 → 데미지 텍스트 시스템에서 사용
    //    GameEvents.Publish<(float, Transform)>(GameEventType.OnEnemyTakeDamage, (damage, transform));

    //    if (currentHp <= 0)
    //        Die();
    //}
    #endregion
}