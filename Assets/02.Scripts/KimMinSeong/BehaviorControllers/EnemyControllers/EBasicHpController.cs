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


    private void OnEnable()
    {
        // 풀에서 꺼낼 때 색 초기화
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    public void Cleanup()
    {
        // 필요시 수정
    }

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

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHp -= damage;
        currentHp = Mathf.Max(0, currentHp);

        // 깜빡임 시작
        if (spriteRenderer != null)
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashHitEffect());
        }

        if (currentHp <= 0)
            Die();
    }

    private System.Collections.IEnumerator FlashHitEffect()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        OnDeath?.Invoke();
    }
}
