using UnityEngine;

// 이 스크립트는 1:1 충돌만을 지원함
public class EBasicCollisionController : MonoBehaviour, ICollidable
{
    private Enemy owner;

    [Header("충돌 관련")]
    private IDamageable currentTarget;
    private float lastDamageTime = -100f;
    [SerializeField] private float collisionDamage = 10f;
    [SerializeField] private float collisionCooldown = 0.5f;
    [SerializeField] private LayerMask damageableLayer;

    public void Initialize(Component owner)
    {
        // Enemy 타입만 허용
        if (owner is not Enemy enemy)
        {
            Debug.LogError($"EBasicCollisionController 는 Enemy 타입만 지원합니다. 현재 타입: {owner.GetType().Name}");
            return;
        }

        this.owner = enemy;
        currentTarget = null;
        lastDamageTime = -100f;
}

    public void Cleanup()
    {
        currentTarget = null;
        lastDamageTime = -100f;
    }

    private void HandleEnter(GameObject target)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
        {
            currentTarget = damageable;
            DealDamage();
        }
    }

    private void DealDamage()
    {
        if (currentTarget == null || currentTarget.IsDead)
        {
            currentTarget = null;
            return;
        }

        currentTarget.TakeDamage(collisionDamage);
        lastDamageTime = Time.time;
    }

    // === 콜라이더 전용 ===

    // 콜라이더 진입 시 즉시 데미지 처리
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (damageableLayer.Contains(collision.gameObject))
            HandleEnter(collision.gameObject);
    }

    // 콜라이더 내에 머무를 때 쿨타임 체크 후 데미지 처리
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (currentTarget == null || currentTarget.IsDead)
            return;

        if (Time.time - lastDamageTime >= collisionCooldown)
            DealDamage();
    }

    // 현재 타겟이 콜라이더에서 벗어날 때 타겟 초기화
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (damageableLayer.Contains(collision.gameObject))
            currentTarget = null;
    }

    // === 트리거 전용 ===

    //// 트리거 진입 시 즉시 데미지 처리
    //private void OnTriggerEnter2D(Collider2D collider)
    //{
    //    if (damageableLayer.Contains(collider.gameObject))
    //        HandleEnter(collider.gameObject);
    //}

    //// 트리거 내에 머무를 때 쿨타임 체크 후 데미지 처리
    //private void OnTriggerStay2D(Collider2D collider)
    //{
    //    if (currentTarget == null || currentTarget.IsDead)
    //        return;

    //    if (Time.time - lastDamageTime >= damageInterval)
    //        DealDamage();
    //}

    //// 현재 타겟이 트리거에서 벗어날 때 타겟 초기화
    //private void OnTriggerExit2D(Collider2D collider)
    //{
    //    if (damageableLayer.Contains(collider.gameObject))
    //        currentTarget = null;
    //}
}