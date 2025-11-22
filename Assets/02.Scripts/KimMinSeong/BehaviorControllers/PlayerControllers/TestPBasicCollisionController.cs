using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 이 스크립트는 1:N 충돌을 지원함
public class TestPCollisionController : MonoBehaviour, ICollidable
{
    private Component owner;

    [Header("충돌 관련")]
    private Dictionary<GameObject, Coroutine> collisionCooldowns = new Dictionary<GameObject, Coroutine>();     // 충돌 쿨타임 관리용 Dictionary
    [SerializeField] private float collisionDamage = 10f;
    [SerializeField] private float collisionCooldown = 0.5f; // 충돌 쿨타임
    [SerializeField] private LayerMask enemyLayer;

    [Header("연동 컴포넌트")]
    [SerializeField] private OverDriveModController overdriveController;


    public void Initialize(Component owner)
    {
        this.owner = owner;
    }

    public void Cleanup() 
    {
        // 모든 코루틴 정리
        foreach (var coroutine in collisionCooldowns.Values)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        collisionCooldowns.Clear();
    }

    // Unity 이벤트 사용
    private void OnCollisionEnter2D(Collision2D collision) => ApplyCollision(collision.gameObject);
    private void OnCollisionStay2D(Collision2D collision) => ApplyCollision(collision.gameObject);
    //private void OnTriggerEnter2D(Collider2D collider) => ApplyCollision(collider.gameObject);
    //private void OnTriggerStay2D(Collider2D collider) => ApplyCollision(collider.gameObject);

    private void ApplyCollision(GameObject target)
    {
        // 1. 레이어 체크
        if (!enemyLayer.Contains(target)) 
            return;

        // 2. 오버드라이브 상태 체크
        if (PlayerStateLogic.Instance.CurrentState != PlayerStateLogic.PlayerState.OverDrive)
            return;

        // 3. 넉백 처리
        // 넉백은 데미지와 상관없이 항상 적용
        IKnockbackable knock = target.GetComponent<IKnockbackable>();
        if (knock != null)
        {
            knock.ApplyKnockback(
                transform.position, // 충돌 주체 위치 (플레이어)
                0,  // speed 에 값을 넣으면 power 필요없음
                overdriveController.speed   // 플레이어 현재 속도 기반 보정도 가능
            );
        }

        // 4. 쿨타임 체크 
        // 해당 적의 쿨타임이 아직 안끝났으면 스킵
        if (collisionCooldowns.ContainsKey(target))
            return;

        // 5. 데미지 처리
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(collisionDamage);

        // 6. 쿨타임 시작
        Coroutine cooldownCoroutine = StartCoroutine(CollisionCooldownCoroutine(target));
        collisionCooldowns.Add(target, cooldownCoroutine);
    }

    private IEnumerator CollisionCooldownCoroutine(GameObject target)
    {
        yield return new WaitForSeconds(collisionCooldown);

        // 쿨타임 종료 후 Dictionary에서 제거
        if (collisionCooldowns.ContainsKey(target))
            collisionCooldowns.Remove(target);
    }
}
