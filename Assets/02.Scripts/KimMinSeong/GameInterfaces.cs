using UnityEngine;

// 데미지 처리 인터페이스
public interface IDamageable
{
    void Initialize(Component owner);
    void TakeDamage(float damage);
    void Die();
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