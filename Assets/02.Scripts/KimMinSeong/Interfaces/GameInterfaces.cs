using UnityEngine;
using System;

// 데미지 처리 인터페이스
public interface IDamageable
{
    void Initialize(Component owner);
    void TakeDamage(float damage);
    //void TakeCollisionDamage(float amount); // 몸통박치기 전용 데미지 처리 메서드
    void Cleanup();
    float CurrentHp { get; }
    float MaxHp { get; }
    bool IsDead { get; }
}

// 이동 처리 인터페이스
public interface IMoveable
{
    void Initialize(Component owner);
    void UpdateMovement(float deltaTime);

    void SetTarget(Transform target);

    void ChangeSpeed();

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
    void Attack();
    void SetTarget(Transform target);
    void Cleanup();
}

// 충돌 처리 인터페이스
public interface ICollidable
{
    void Initialize(Component owner);
    void Cleanup();
}

// 드랍 처리 인터페이스
public interface IDroppable
{
    void Initialize(Component owner);
    void Drop();
    void Cleanup();
}

// 넉백 처리 인터페이스
public interface IKnockbackable
{
    void Initialize(Component owner);
    void ApplyKnockback(Vector2 sourcePosition, float power, float playerSpeed = 0f);
    void ApplyPull(Vector2 targetPosition, float power, float playerSpeed = 0f);
    void Cleanup();
}
