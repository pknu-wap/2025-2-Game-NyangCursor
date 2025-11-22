using UnityEngine;

public class RangeAttackController : MonoBehaviour, IAttackable
{
    private Enemy owner;
    private Transform target;   // 공격할 대상

    [Header("회피 / 공격 범위 설정")]
    [SerializeField] private float evadeRange = 2f;     // 회피 범위
    [SerializeField] private float attackRange = 8f;    // 공격 범위

    [Header("범위 마진 설정")]
    // 여유 범위가 없다면 경계선에서 프레임 단위로 진입/퇴장이 반복됨
    [SerializeField] private float evadeMargin = 0.5f; // 회피 범위 마진
    [SerializeField] private float attackMargin = 1f; // 공격 범위 마진


    [Header("공격 설정")]
    [SerializeField] private GameObject bulletPrefab;   // 탄막 프리팹
    [SerializeField] private float attackCooldown = 2f; // 공격 쿨타임
    [SerializeField] private float attackDamage = 10f;  // 공격 데미지
    [SerializeField] private float bulletSpeed = 5f;    // 탄막 속도

    private float lastAttackTime = -999f;

    // 범위를 판단하기 위한 상태 변수
    private bool isInEvadeRange = false;
    private bool isInAttackRange = false;

    public void Initialize(Component owner)
    {
        if (owner is not Enemy enemy)
        {
            Debug.LogError("RangedAttackController 는 Enemy 타입만 지원합니다");
            return;
        }

        this.owner = enemy;
    }

    public void Cleanup()
    {
        isInEvadeRange = false;
        isInAttackRange = false;
    }

    private void Update()
    {
        if (target == null) 
            return;

        // 현재 플레이어가 어느 범위에 있는지 체크
        CheckRanges();

        // 공격 범위 안에 있고 회피 범위 밖이면 공격
        if (isInAttackRange && !isInEvadeRange && CanAttack())
            Attack();
    }

    private void CheckRanges()
    {
        // 1. 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        // 2. 회피 범위 체크
        bool shouldEnterEvade = distanceToPlayer <= evadeRange;
        bool shouldExitEvade = distanceToPlayer > (evadeRange + evadeMargin);   // 나갈 때는 마진 적용

        // 이전에는 회피 범위 안에 없었으면서 지금은 회피 상태로 진입해야 할 때
        if (!isInEvadeRange && shouldEnterEvade)
        {
            owner.EventBus.Publish(EnemyEventType.OnEnterEvadeRange);   // 로컬 이벤트 버스 → 회피 범위 입장 이벤트 발행
            isInEvadeRange = true;
            isInAttackRange = true;             // 공격 범위가 회피 범위보다 크므로 자동으로 공격 범위로 간주
            return;
        }

        // 이전에 회피 범위 안에 있었으면서 지금은 회피 상태에서 벗어나야 할 때
        else if (isInEvadeRange && shouldExitEvade)
        {
            owner.EventBus.Publish(EnemyEventType.OnExitEvadeRange);    // 로컬 이벤트 버스 → 회피 범위 퇴장 이벤트 발행
            isInEvadeRange = false;
        }

        // 회피 범위 안에 있는 상태에서 CheckRanges 가 연속으로 2번 호출될 수 있기 때문에 불필요한 연산을 방지
        if (isInEvadeRange)
            return;

        // 3. 공격 범위 체크
        bool shouldEnterAttack = distanceToPlayer <= attackRange;
        bool shouldExitAttack = distanceToPlayer > (attackRange + attackMargin);

        // 이전에는 공격 범위 안에 없었으면서 지금은 공격 상태로 진입해야 할 때
        if (!isInAttackRange && shouldEnterAttack)
        {
            owner.EventBus.Publish(EnemyEventType.OnEnterAttackRange);  // 로컬 이벤트 버스 → 공격 범위 입장 이벤트 발행
            isInAttackRange = true;
        }

        // 이전에는 공격 범위 안에 있었으면서 지금은 공격 상태에서 벗어나야 할 때
        else if (isInAttackRange && shouldExitAttack)
        {
            owner.EventBus.Publish(EnemyEventType.OnExitEvadeRange);    // 로컬 이벤트 버스 → 공격 범위 퇴장 이벤트 발행
            isInAttackRange = false;
        }
    }

    // 쿨타임을 확인하여 현재 공격이 가능한지 체크하는 함수
    private bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown;
    }

    // 공격을 실행하는 함수
    public void Attack()
    {
        if (target == null || bulletPrefab == null)
            return;

        lastAttackTime = Time.time;

        // 탄막을 발사하기 위해 방향 벡터를 구함
        Vector3 direction = (target.position - transform.position).normalized;

        // 탄막 스폰
        GameObject bullet = PoolManager.instance.Spawn(bulletPrefab, transform.position);

        // 탄막 내부 변수 초기화
        Bullet bulletController = bullet.GetComponent<Bullet>();
        bulletController?.Initialize(direction, bulletSpeed, attackDamage);
    }

    // 공격할 대상을 설정하는 함수
    // EnemyManager 에서 호출됨
    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    // 디버그용 기즈모
    private void OnDrawGizmosSelected()
    {
        // 회피 범위 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, evadeRange);

        // 회피 범위 + 마진 (빨간색, 투명도 낮게)
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, evadeRange + evadeMargin);

        // 공격 범위 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 공격 범위 + 마진 (노란색, 투명도 낮게)
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRange + attackMargin);
    }
}