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
        rb = GetComponent<Rigidbody2D>();
        lastKnockbackTime = -999f;
    }

    public void Cleanup()
    {
        //rb = null;
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

    public void ApplyKnockbackWithScatter(Vector2 sourcePosition, float power, float scatterAmount = 0.3f, float playerSpeed = 0f)
    {
        if (Time.time - lastKnockbackTime < knockbackCooldown)
            return;

        lastKnockbackTime = Time.time;

        if (rb == null)
            return;

        // 기본 넉백 방향 (총알 → 적 기준)
        Vector2 dir = (rb.position - sourcePosition).normalized;

        // ============================
        //  🔥 방향에 '흩뿌림(Scatter)' 추가
        // ============================
        // scatterAmount 예) 0.3 => 최대 ±30도 흔들기
        float randomAngle = Random.Range(-scatterAmount, scatterAmount) * 90f;
        dir = Quaternion.Euler(0, 0, randomAngle) * dir;

        // ============================
        //  힘 계산
        // ============================
        float finalPower = (playerSpeed > 0f) ? playerSpeed : power;

        rb.AddForce(dir * finalPower, ForceMode2D.Impulse);

        GetComponent<IMoveable>()?.PauseMovementForSeconds(stopDuration);
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
