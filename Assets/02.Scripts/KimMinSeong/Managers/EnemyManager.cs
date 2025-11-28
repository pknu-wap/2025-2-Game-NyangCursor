using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

/*
현재는 DifficultyManager 와 협업이 이루어지지 않은 상태입니다.
나중에는 DifficultyManager 로부터 spawnPeriod, maxEnemies, enemyDatasToSpawn 을 
OnIncreaseDifficulty 이벤트를 통해 전달받을 예정입니다.
*/

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    [Header("참조하는 오브젝트들")]
    [SerializeField] private DifficultyManager difficultyManager;

    [Header("현재 난이도 관련 변수")]
    private float spawnPeriod = 1f; // 적 생성 요청 주기
    private int currentEnemies; // 현재 생성된 적 개수
    private int maxEnemies = 50; // 최대 적 개수
    private int totalWeight = 0; // 적 리스트의 가중치 총합
    private Coroutine spawnCoroutine; // 적 생성 코루틴
    private List<EnemySpawnEntity> currentSpawnableEnemies = new List<EnemySpawnEntity>();  // 현재 난이도에서 사용하는 적 리스트

    [Header("생성 위치 관련")]
    [SerializeField] private Transform playerTransform;  // 생성한 적에게 주입할 플레이어 좌표
    [SerializeField] private CircleCollider2D spawnZoneCollider;  // 스폰 영역
    [SerializeField] private CircleCollider2D combatZoneCollider; // 적이 활동하는 영역

    void Awake()
    {
        // EnemyManager 인스턴스 설정
        if (instance == null)
            instance = this;
        else
        {
            Debug.Log("Scene 에 기존의 EnemyManager instance 가 존재합니다. 하나를 파괴합니다");
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        CombatZoneTrigger.onEnemyExited += RepositionEnemy;
    }

    void OnDisable()
    {
        CombatZoneTrigger.onEnemyExited -= RepositionEnemy;
    }

    void Start()
    {
        // 초기 난이도를 difficultyManager 로부터 받아온 후 적용
        DifficultyData currentDifficulty = difficultyManager.CurrentDifficulty;
        ApplyDifficulty(currentDifficulty);

        // 코루틴 시작
        StartCoroutine(SpawnCoroutine());
    }

    public void ApplyDifficulty(DifficultyData data)
    {
        spawnPeriod = data.spawnPeriod;
        maxEnemies = data.maxEnemies;
        currentSpawnableEnemies = new List<EnemySpawnEntity>(data.spawnableEnemies);

        // 가중치 총합을 계산
        totalWeight = 0;
        foreach (var entity in currentSpawnableEnemies)
            totalWeight += entity.spawnWeight;

        Debug.Log($"ApplyDifficulty 발동: 주기: {spawnPeriod}s, 최대 적 개수: {maxEnemies}");

        // 코루틴 재시작
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(SpawnCoroutine());
        }
    }

    // spawnPeriod 마다 PoolManager 을 통해 적을 생성하는 함수
    IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnPeriod);

            // 현재 적 개수가 최대치보다 작을 때에만 요청
            if (currentEnemies < maxEnemies)
            {
                GameObject randomEnemyPrefab = GetRandomEnemy();
                Vector2 spawnPosition = GetRandomSpawnPosition();

                if (randomEnemyPrefab == null)
                {
                    Debug.Log("SpawnCoroutine: 생성할 적이 없습니다");
                    continue;
                }

                GameObject enemy = PoolManager.instance.Spawn(randomEnemyPrefab, spawnPosition);

                IMoveable moveable = enemy.GetComponent<IMoveable>();   // 적의 타깃으로 쓸 플레이어 좌표 주입
                moveable?.SetTarget(playerTransform);
                moveable?.ChangeSpeed();

                IAttackable attackable = enemy.GetComponent<IAttackable>();
                attackable?.SetTarget(playerTransform); // 적의 타깃으로 쓸 플레이어 좌표 주입

                if (enemy != null)
                    currentEnemies++;
                else
                    Debug.Log("SpawnCoroutine: 해당 Prefab 이 Pool 에 등록되어있지 않습니다");
            }
        }
    }

    // 현재 생성 가능한 적 리스트에서 랜덤하게 하나를 선택하는 함수
    private GameObject GetRandomEnemy()
    {
        if (currentSpawnableEnemies.Count == 0)
        {
            Debug.Log("GetRandomEnemy: 현재 생성 가능한 적이 없습니다!");
            return null;
        }

        // 0 부터 totalWeight 사이의 랜덤한 값을 먼저 선택
        float random = Random.Range(0f, totalWeight);
        int cumulative = 0;

        foreach (var entity in currentSpawnableEnemies)
        {
            cumulative += entity.spawnWeight;

            // 가중치 구간에 해당하면 해당 적을 반환
            if (random <= cumulative)
                return entity.enemyPrefab;
        }

        // 이론상 실행되지 않는 부분
        return currentSpawnableEnemies[0].enemyPrefab;
    }

    // SpawnZone 경계의 랜덤한 위치를 계산하는 함수
    Vector2 GetRandomSpawnPosition()
    {
        // 0 ~ 360도 사이의 랜덤한 각도를 선택
        float randomAngle = Random.Range(0f, 2f * Mathf.PI);

        // 월드 기준 Collider 의 중심 좌표와 반지름을 계산
        Vector2 center = (Vector2)spawnZoneCollider.transform.position + spawnZoneCollider.offset;

        // 스케일에 따라 반지름도 영향을 받으므로 정확한 계산을 위해 보정
        Vector3 scale = spawnZoneCollider.transform.localScale;
        float radius = spawnZoneCollider.radius * Mathf.Max(scale.x, scale.y);

        // 경계 위의 위치를 구하기 위해 삼각함수 사용
        float x = center.x + radius * Mathf.Cos(randomAngle);
        float y = center.y + radius * Mathf.Sin(randomAngle);

        return new Vector2(x, y);
    }

    // OnEnemyExited 의 콜백 함수로 전투 영역에서 벗어난 적을 재배치하는 함수
    void RepositionEnemy(Transform enemyTransform)
    {
        // 현재 적 위치에서 제일 가까운 CombatZone 경계 위의 위치를 구함
        Vector2 closestPosition = combatZoneCollider.bounds.ClosestPoint(enemyTransform.position);

        // 해당 위치에서 약간 안쪽 영역에 배치
        Vector2 center = combatZoneCollider.bounds.center;
        Vector2 direction = (center - closestPosition).normalized;
        Vector2 adjustedPosition = closestPosition + direction * 0.2f;

        enemyTransform.position = adjustedPosition;
    }

    public void DecreaseCurrentEnemies()
    {
        currentEnemies -= 1;
        if (currentEnemies < 0)
            currentEnemies = 0;

        // 디버그용
        Debug.Log($"현재 적 개수: {currentEnemies}");
    }
}
