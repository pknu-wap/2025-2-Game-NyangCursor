using System;
using UnityEngine;

public class PotDamageController : MonoBehaviour, IDamageable
{
    private FieldObject owner;
    private float currentHp;
    private float maxHp;
    private bool isDead;

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

    public void Initialize(Component owner)
    {
        // Pot 타입만 허용
        if (owner is not FieldObject pot)
        {
            Debug.LogError($"PotDamageController 는 FieldObject 타입만 지원합니다. 현재 타입: {owner.GetType().Name}");
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

        owner.EventBus.Publish(FieldObjectEventType.OnBreak);
    }

    public void TakeCollisionDamage(float amount)
    {
        //todo 오버드라이브 충돌 시 반응
    }
}