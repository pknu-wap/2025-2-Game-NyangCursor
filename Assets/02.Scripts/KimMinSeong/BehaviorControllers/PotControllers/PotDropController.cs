using System.Collections.Generic;
using UnityEngine;

public class PotDropController : MonoBehaviour, IDroppable
{
    private Pot owner;
    private Transform dropTransform; // 드랍하고자 하는 위치

    [Header("드랍 횟수")]
    [SerializeField, Range(1, 50)] private int minDropCount = 1;
    [SerializeField, Range(1, 50)] private int maxDropCount = 10;

    [Header("흩뿌리기 설정값")]
    [SerializeField, Range(1f, 10f)] private float minScatterForce = 1f;  // 흩뿌리는 힘의 최소값
    [SerializeField, Range(1f, 10f)] private float maxScatterForce = 10f;    // 흩뿌리는 힘의 최대값

    [Header("드랍할 객체들")]
    [SerializeField] private List<DropEntity> dropEntities;    // 드랍할 객체들을 저장하는 리스트

    public void Initialize(Component owner)
    {
        // Pot 타입만 허용
        if (owner is not Pot pot)
        {
            Debug.LogError($"PotDropController 는 Pot 타입만 지원합니다. 현재 타입: {owner.GetType().Name}");
            return;
        }

        this.owner = pot;
        dropTransform = transform;
    }
    public void Cleanup()
    {
    }

    public void Drop()
    {
        // 드랍 횟수를 잘못 설정했다면 Switch 
        if (minDropCount > maxDropCount)
            (minDropCount, maxDropCount) = (maxDropCount, minDropCount);

        // 랜덤한 횟수로 드랍
        int dropCount = Random.Range(minDropCount, maxDropCount + 1);
        for (int i = 0; i < dropCount; ++i)
        {
            DropObject<ExpDropObject>();
            DropObject<GoldDropObject>();
        }
    }

    private void DropObject<T>() where T : DropObject
    {
        foreach (var entity in dropEntities)
        {
            // 현재 리스트에서 드랍하고자 하는 타입을 필터링
            if (entity.DropPrefab.TryGetComponent<T>(out T dropComponent))
            {
                // 무작위 확률로 드랍
                float randomValue = UnityEngine.Random.Range(0f, 100f);
                if (randomValue <= entity.DropRate)
                {
                    GameObject droppedObj = PoolManager.instance.Spawn(entity.DropPrefab, dropTransform.position);

                    // 드랍한 오브젝트에 RigidBody2D 컴포넌트가 있다면 흩뿌리게 스폰하도록 설정
                    if (droppedObj.TryGetComponent<Rigidbody2D>(out var rb))
                        ScatterObject(rb);
                }
            }
        }
    }

    // 드랍한 오브젝트를 랜덤하게 흩뿌리는 함수
    private void ScatterObject(Rigidbody2D rb)
    {
        // 흩뿌리기 설정을 잘못 했다면 Switch 
        if (minScatterForce > maxScatterForce)
            (minScatterForce, maxScatterForce) = (maxScatterForce, minScatterForce);

        // 단위원 (반지름이 1인 원) 안에서 랜덤한 벡터를 찾음
        // 해당 벡터를 정규화하여 방향 벡터를 얻음
        // 해당 방향으로 랜덤한 힘의 크기를 곱하여 최종 벡터를 구함
        Vector2 randomForce = Random.insideUnitCircle.normalized * Random.Range(minScatterForce, maxScatterForce);

        // 오브젝트에 위에서 구한 힘만큼의 순간 충격을 적용
        rb.AddForce(randomForce, ForceMode2D.Impulse);
    }
}