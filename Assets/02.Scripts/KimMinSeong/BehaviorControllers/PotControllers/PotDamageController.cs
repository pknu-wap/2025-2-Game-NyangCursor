using System;
using UnityEngine;

public class PotDamageController : MonoBehaviour, IDamageable
{
    private Pot owner;
    private float currentHp;
    private float maxHp;
    private bool isDead;

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

    public event Action OnDeath;

    public void Initialize(Component owner)
    {
        // Pot 타입만 허용
        if (owner is not Pot pot)
        {
            Debug.LogError($"PotDamageController 는 Pot 타입만 지원합니다. 현재 타입: {owner.GetType().Name}");
            return;
        }

        this.owner = pot;
        maxHp = 0;
        currentHp = maxHp;
        isDead = false;
    }

    public void Cleanup()
    {
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        // 항아리 오브젝트는 체력과 상관 없이 깨지도록 설정
        Die();
    }

    private void Die()
    {
        isDead = true;
        /*
         * 추후에 추가적인 작업들을 여기에 구현 (ex. 이펙트, 사운드...)
        */

        OnDeath?.Invoke();
    }
}