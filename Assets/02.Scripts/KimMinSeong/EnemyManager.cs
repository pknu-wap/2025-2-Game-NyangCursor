using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Collections;

/*
현재는 DifficultyManager 와 협업이 이루어지지 않은 상태입니다.
나중에는 DifficultyManager 로부터 spawnPeriod, maxEnemies, enemyDatasToSpawn 을 
OnIncreaseDifficulty 이벤트를 통해 전달받을 예정입니다.
*/

public class EnemyManager : MonoBehaviour
{
    // 외부 (DifficultyManager) 에서 변경될 멤버 변수
    [SerializeField] private float spawnPeriod; // 적 생성 요청 주기
    [SerializeField] private int maxEnemies;    // 최대 적 개수

    // 내부에서 관리하는 멤버 변수
    private int currentEnemies;
    [SerializeField] private List<EnemyData> enemyDatasToSpawn; // 스폰할 적 데이터 리스트
    [SerializeField] private CircleCollider2D spawnZoneCollider;  // 스폰 영역
    [SerializeField] private CircleCollider2D combatZoneCollider; // 적이 활동하는 영역

    // 발행하는 이벤트
    public static event Action<EnemyData> OnRequestEnemy;  // PoolManager 에게 적 스폰을 요청

    // 멤버 초기화
    void Awake()
    {
        spawnPeriod = 2f;
        maxEnemies = 50;
        currentEnemies = 0;
        spawnZoneCollider = GetComponent<CircleCollider2D>();
        combatZoneCollider = GetComponent<CircleCollider2D>();
    }

    // 이벤트 구독
    void OnEnable()
    {
        // DifficultyManager.OnIncreaseDifficulty += HandleDifficulty;
        // PoolManager.OnEnemyReady += DeployEnemy;
        // CombatZoneTrigger.onEnemyExited += RepositionEnemy;
    }

    // 이벤트 구독 해제
    void OnDisable()
    {
        // DifficultyManager.OnIncreaseDifficulty -= HandleDifficulty;
        // PoolManager.OnEnemyReady -= DeployEnemy;
        // CombatZoneTrigger.onEnemyExited -= RepositionEnemy;
    }

    // spawnPeriod 마다 coroutine 실행
    void Start()
    {
        StartCoroutine(SpawnRequestCoroutine());
    }

    // spawnPeriod 마다 PoolManager 에게 적 스폰 요청을 하는 함수
    IEnumerator SpawnRequestCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnPeriod);

            // 현재 적 개수가 최대치보다 작을 때에만 요청
            if (currentEnemies < maxEnemies)
            {
                EnemyData randomEnemy = enemyDatasToSpawn[Random.Range(0, enemyDatasToSpawn.Count)];
                OnRequestEnemy?.Invoke(randomEnemy);
                currentEnemies++;
            }
        }
    }

    // DifficultyManager 와 협업 후에 구현할 예정입니다.
    //void HandleDifficulty() {}

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

    // OnEnemyReady 의 콜백 함수로 스폰 영역에 적을 배치하는 함수
    void DeployEnemy(GameObject enemy)
    {
        Vector2 spawnPosition = GetRandomSpawnPosition();
        enemy.transform.position = spawnPosition;
    }

    // OnEnemyExited 의 콜백 함수로 전투 영역에서 벗어난 적을 재배치하는 함수
    void RepositionEnemy(Transform enemyTransform)
    {
        Vector2 closestPosition = combatZoneCollider.bounds.ClosestPoint(enemyTransform.position);
        enemyTransform.position = closestPosition;
    }
}
