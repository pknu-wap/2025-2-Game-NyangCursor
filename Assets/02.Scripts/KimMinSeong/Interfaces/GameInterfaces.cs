using UnityEngine;
using System;

// 데미지 처리 인터페이스
public interface IDamageable
{
    void Initialize(Component owner);
    void TakeDamage(float damage);
    void TakeCollisionDamage(float amount); // 몸박 전용
    void Cleanup();
    float CurrentHp { get; }
    float MaxHp { get; }
    bool IsDead { get; }
    event Action OnDeath;
}

// 이동 처리 인터페이스
public interface IMoveable
{
    void Initialize(Component owner);
    void UpdateMovement(float deltaTime);

    void SetTarget(Transform target);

    // 이동 정지/재개(여러 시스템이 동시에 정지 시켜도 안전)
    void PauseMovement();
    void ResumeMovement();

    // 특정 시간 정지(스턴, 얼음 등)
    void PauseMovementForSeconds(float seconds);

    void Stop();
    void Cleanup();
}

// 공격 처리 인터페이스
public interface IAttackable
{
    void Initialize(Component owner);
    void TryAttack(Transform target);
    bool CanAttack();
    void Cleanup();
}

// 충돌 처리 인터페이스
public interface ICollidable
{
    void Initialize(Component owner);
    void OnCollisionDetected(Collision2D collision);
    void OnTriggerDetected(Collider2D collider);
    void Cleanup();
}

// 드랍 처리 인터페이스
public interface IDroppable
{
    void Initialize(Component owner);
    void Drop();
    void Cleanup();
}

public interface IKnockbackable
{
    void ApplyKnockback(Vector2 sourcePosition, float power, float playerSpeed = 0f);
}
