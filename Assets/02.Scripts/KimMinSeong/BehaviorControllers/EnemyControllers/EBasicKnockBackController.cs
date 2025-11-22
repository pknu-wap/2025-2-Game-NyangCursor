using UnityEngine;

public class EBasicKnockBackController : MonoBehaviour, IKnockbackable
{
    private Enemy owner;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float stopDuration = 0.4f; // 넉백 중 이동 멈춤
    [SerializeField] private float knockbackCooldown = 0.5f; // 넉백 쿨타임

    private float lastKnockbackTime = -999f;

    public void Initialize(Component owner)
    {
        // Enemy 타입만 허용
        if (owner is not Enemy enemy)
        {
            Debug.LogError($"EBasicKnockBackController 는 Enemy 타입만 지원합니다. 현재 타입: {owner.GetType().Name}");
            return;
        }

        this.owner = enemy;
        rb = owner.GetComponent<Rigidbody2D>();
        lastKnockbackTime = -999f;
    }

    public void Cleanup()
    {
        rb = null;
    }

    public void ApplyKnockback(Vector2 sourcePosition, float power, float playerSpeed = 0f)
    {
        // 넉백 쿨타임 중이면 무시
        if (Time.time - lastKnockbackTime < knockbackCooldown)
            return;

        lastKnockbackTime = Time.time;

        // 넉백 방향 = 적 → 플레이어 반대
        Vector2 dir = (rb.position - sourcePosition).normalized;

        // ============================
        //   power 결정 로직
        //   playerSpeed > 0이면 speed 기반 넉백
        // ============================
        float finalPower = (playerSpeed > 0f) ? playerSpeed : power;

        // 물리 넉백
        rb.AddForce(dir * finalPower, ForceMode2D.Impulse);

        // 로컬 이벤트 버스 사용 → 이동 멈춤 처리
        owner.EventBus.Publish<float>(EnemyEventType.OnKnockback, stopDuration);
    }

    public void ApplyPull(Vector2 targetPosition, float power, float playerSpeed = 0f)
    {
        // 끌림 쿨타임 (넉백과 같은 쿨타임 사용)
        if (Time.time - lastKnockbackTime < knockbackCooldown)
            return;

        lastKnockbackTime = Time.time;

        if (rb == null)
            return;

        // 끌림 방향 = 현재 위치 → targetPosition 방향
        Vector2 dir = (targetPosition - rb.position).normalized;

        // power 결정 로직 동일
        float finalPower = (playerSpeed > 0f) ? playerSpeed : power;

        // 물리 적용 (중심으로 빨려 들어감)
        rb.AddForce(dir * finalPower, ForceMode2D.Impulse);

        // 로컬 이벤트 버스 사용 → 이동 멈춤 처리
        owner.EventBus.Publish<float>(EnemyEventType.OnKnockback, stopDuration);
    }

}
