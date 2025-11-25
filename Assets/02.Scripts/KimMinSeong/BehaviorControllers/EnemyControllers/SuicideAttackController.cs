using System.Collections;
using UnityEngine;

public class SuicideAttackController : MonoBehaviour, IAttackable
{
    private Enemy owner;
    private Transform target;

    [Header("자폭 설정")]
    [SerializeField] private float suicideRange = 1.5f;     // 자폭 감지 범위 (주황색)
    [SerializeField] private float suicideDelay = 1.0f;     // 자폭까지 대기 시간
    [SerializeField] private float suicideDamage = 50f;     // 자폭 데미지
    [SerializeField] private float suicideRadius = 2f;      // 자폭 피해 범위 (빨간색)
    [SerializeField] private GameObject explosionPrefab;    // 폭발 프리팹

    private bool isSuiciding = false;                       // 자폭 준비 중인지 여부

    public void Initialize(Component owner)
    {
        if (owner is not Enemy enemy)
        {
            Debug.LogError("SuicideAttackController 는 Enemy 타입만 지원합니다");
            return;
        }

        this.owner = enemy;
    }

    public void Cleanup()
    {
        isSuiciding = false;
    }

    private void Update()
    {
        if (target == null || isSuiciding)
            return;

        CheckSuicideRange();
    }

    private void CheckSuicideRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        // 자폭 범위 진입 시 자폭 시작 (취소 불가)
        if (distanceToPlayer <= suicideRange)
        {
            // 자폭 범위 입장 이벤트 발행 → 이동 컨트롤러에서 처리
            owner.EventBus.Publish(EnemyEventType.OnEnterSuicideRange);
            StartSuicide();
        }
    }

    private void StartSuicide()
    {
        if (isSuiciding)
            return;

        isSuiciding = true;
        StartCoroutine(SuicideRoutine());
    }

    private IEnumerator SuicideRoutine()
    {
        // 자폭까지 대기
        yield return new WaitForSeconds(suicideDelay);

        // 자폭 실행
        Attack();
    }

    public void Attack()
    {
        if (target == null || explosionPrefab == null)
            return;

        // 폭발 이펙트 생성
        GameObject explosion = PoolManager.instance.Spawn(explosionPrefab, transform.position);

        // 폭발 이펙트 정보 초기화
        Explosion explosionController = explosion.GetComponent<Explosion>();
        explosionController.Initialize(suicideDamage, suicideRadius);

        // 자폭 이벤트 발행 → 체력 컨트롤러에서 처리
        owner.EventBus.Publish(EnemyEventType.OnSuicide);
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    // 디버그용 기즈모
    private void OnDrawGizmosSelected()
    {
        // 자폭 피해 범위 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, suicideRadius);

        // 자폭 감지 범위 (주황색)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, suicideRange);
    }
}