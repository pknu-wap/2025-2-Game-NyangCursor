using System;
using UnityEngine;

public class EBasicHpController : MonoBehaviour, IDamageable
{
    private Enemy owner;
    private float currentHp;
    private float maxHp;
    private bool isDead;

    [Header("피격 시 시각 효과")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.15f;

    private Color originalColor = Color.white;
    private Coroutine flashRoutine;

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

    public event Action OnDeath;
    public event Action<float, float> OnInitializeHp;
    public event Action<float, float> OnTakeDamage;

    public static event Action OnTakeCollisionDamage;

    // ------------------------
    // 몸박 전용 추가 변수
    // ------------------------
    [Header("충돌 데미지 설정")]
    [SerializeField] private float collisionDamageInterval = 0.5f; // 몸박 무적시간
    private float lastCollisionDamageTime = -999f;


    private void OnEnable()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    public void Cleanup() { }

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

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // ----------------------------------
    // 스킬/총알 데미지 = 즉시 반영
    // ----------------------------------
    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHp -= damage;
        currentHp = Mathf.Max(0, currentHp);
        OnTakeDamage?.Invoke(currentHp, damage);
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
        OnDeath?.Invoke();
    }


    // -----------------------------------------
    // 몸박 데미지 = 간격(쿨타임) 있는 데미지
    // -----------------------------------------
    public void TakeCollisionDamage(float amount)
    {
        if (isDead)
            return;

        // 몸박 무적시간 적용
        if (Time.time - lastCollisionDamageTime < collisionDamageInterval)
            return;

        lastCollisionDamageTime = Time.time;

        currentHp -= amount;
        currentHp = Mathf.Max(0, currentHp);

        TriggerHitFlash();

        OnTakeCollisionDamage?.Invoke();//카메라 흔들림 이벤트

        if (currentHp <= 0)
            Die();
    }
}
