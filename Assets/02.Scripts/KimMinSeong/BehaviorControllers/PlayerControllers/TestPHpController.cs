using System;
using UnityEngine;

public class PlayerHpController : MonoBehaviour, IDamageable
{
    private Component owner;
    private float currentHp;
    private float maxHp;
    private bool isDead;

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

    public event Action OnDeath;
    public static event Action<float, float> OnInitializeHp;
    public static event Action<float, float> OnTakeDamage; 

    public void Initialize(Component owner)
    {
        this.owner = owner;
        maxHp = PlayerStatsManager.instance.GetStat(StatType.MaxHealthUp);
        currentHp = maxHp;
        isDead = false;

        OnInitializeHp?.Invoke(currentHp, maxHp);
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHp -= damage;
        currentHp = Mathf.Max(0, currentHp);

        OnTakeDamage?.Invoke(currentHp, maxHp);

        if (currentHp <= 0)
            Die();
    }

    public void Die()
    {
        if (isDead) 
            return;

        isDead = true;

        // 플레이어 사망시 게임 종료 요청
        StageFlowManager.instance.SetStateToEnd();        
    }

    public void Cleanup()
    {
        // 필요시 수정
    }
}