using UnityEngine;

public class TestPCollisionController : MonoBehaviour, ICollidable
{
    private Component owner;

    [Header("충돌 데이터")]
    [SerializeField] private float collisionDamage = 10f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("연동 컴포넌트")]
    [SerializeField] private OverDriveModController overdriveController;


    public void Initialize(Component owner)
    {
        this.owner = owner;
    }


    public void OnCollisionDetected(Collision2D collision)
    {
        if (enemyLayer.Contains(collision.gameObject))
        {
            // 1) 충돌 데미지 전달 (쿨타임은 적이 처리함)
            ApplyCollisionDamage(collision.gameObject);

            // 3) 감속(기존 로직)
            //overdriveController.ApplyCollisionSlow(0.5f);
        }
    }

    //유체화 사용 시 (플레이어 trigger)
    public void OnTriggerDetected(Collider2D collider)
    {
        if (enemyLayer.Contains(collider.gameObject))
        {
            // 트리거 충돌도 데미지는 줄 수 있음
            ApplyCollisionDamage(collider.gameObject);
        }
    }

    // ============================
    //  충돌 데미지 및 넉백 적용
    // ============================
    private void ApplyCollisionDamage(GameObject target)
    {
        // 플레이어가 오버드라이브 상태가 아닐 경우 충돌 데미지 없음
        if (PlayerStateLogic.Instance.CurrentState != PlayerStateLogic.PlayerState.OverDrive)
            return;

        // ===== 데미지 처리 =====
        IDamageable dmg = target.GetComponent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeCollisionDamage(collisionDamage);
        }

        // ===== 넉백 처리 =====
        IKnockbackable knock = target.GetComponent<IKnockbackable>();
        if (knock != null)
        {
            knock.ApplyKnockback(
                transform.position,        // 충돌 주체(플레이어) 위치
                0,            // speed에 값을 넣으면 power필요없음
                overdriveController.speed   // 플레이어 현재 속도 기반 보정도 가능
            );
        }
    }

    // Unity 이벤트 전달
    private void OnCollisionEnter2D(Collision2D collision) => OnCollisionDetected(collision);
    private void OnCollisionStay2D(Collision2D collision) => OnCollisionDetected(collision);

    private void OnTriggerEnter2D(Collider2D collider) => OnTriggerDetected(collider);
    private void OnTriggerStay2D(Collider2D collider) => OnTriggerDetected(collider);

    public void Cleanup() { }
}
