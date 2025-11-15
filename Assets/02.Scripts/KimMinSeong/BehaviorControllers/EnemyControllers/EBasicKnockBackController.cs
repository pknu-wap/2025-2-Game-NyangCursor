using UnityEngine;

public class EBasicKnockBackController : MonoBehaviour, IKnockbackable
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float stopDuration = 0.4f; // 넉백 중 이동 멈춤
    [SerializeField] private float knockbackCooldown = 0.5f; // 넉백 쿨타임

    private float lastKnockbackTime = -999f;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyKnockback(Vector2 sourcePosition, float power, float playerSpeed = 0f)
    {
        // 넉백 쿨타임 중이면 무시
        if (Time.time - lastKnockbackTime < knockbackCooldown)
            return;

        lastKnockbackTime = Time.time;

        if (rb == null)
            return;

        // 넉백 방향 = 적 → 플레이어 반대
        Vector2 dir = (rb.position - sourcePosition).normalized;

        // ============================
        //   power 결정 로직
        //   playerSpeed > 0이면 speed 기반 넉백
        // ============================
        float finalPower = (playerSpeed > 0f) ? playerSpeed : power;

        // 물리 넉백
        rb.AddForce(dir * finalPower, ForceMode2D.Impulse);

        // Moveable 인터페이스 가져오기 
        var mover = GetComponent<IMoveable>();
        mover?.PauseMovementForSeconds(stopDuration);
    }
}
