using UnityEngine;
using System;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance; // 싱글톤 방식

    [SerializeField] private List<GameObject> initialPrefabs;  // Scene 에서 사용하는 모든 Prefab 를 저장하는 배열 
    [SerializeField] private int initialPoolSize = 10;  // Pool 을 초기화 하는데 사용되는 변수
    [SerializeField] private int maxPoolSize = 100; // 최대 Pool 크기

    // 여러 개의 pool 을 관리하는 Dictionary
    // {key: prefab, value: pool} 를 사용
    private Dictionary<GameObject, Stack<GameObject>> pools;
    // Instance 가 어느 Prefab 에서 생성되었는 지를 역추적하기 위한 Dictionary
    // {key: instance, value: prefab} 를 사용
    private Dictionary<GameObject, GameObject> instanceDict;

    void Awake()
    {
        // PoolManager 인스턴스, Dictionary 초기화
        if (instance == null) 
            instance = this;
        else
        {
            Debug.Log("Scene 에 기존의 PoolManager 가 존재합니다. 하나를 파괴합니다");
            Destroy(gameObject);
            return;
        }
        pools = new Dictionary<GameObject, Stack<GameObject>>();
        instanceDict = new Dictionary<GameObject, GameObject>();
        InitializePools();
    }

    void InitializePools()
    {
        // Scene 에서 사용하는 적 데이터가 없다면 Skip
        if (initialPrefabs.Count <= 0)
        {
            Debug.Log("현재 Scene 에서 사용하는 Prefab 이 없습니다. Inspector 창에서 추가해주세요");
            return;
        }

        // Scene 에서 사용하는 Prefab 에 대해서 Pool 생성
        foreach (GameObject prefab in initialPrefabs)
            CreatePool(prefab);
    }

    void CreatePool(GameObject prefab)
    {
        // 해당 데이터에 대해서 이미 Pool 이 존재한다면 Skip
        if (pools.ContainsKey(prefab))
        {
            Debug.Log("해당 Prefab 에 대해 이미 Pool 이 존재합니다");
            return;
        }

        // Pool 생성
        Stack<GameObject> pool = new Stack<GameObject>(maxPoolSize);

        // Pool 초기화
        for (int i = 0; i < initialPoolSize; ++i)
        {
            GameObject newObj = Instantiate(prefab, transform);
            newObj.SetActive(false);
            pool.Push(newObj);
        }
        // 해당 Pool 을 Dictionary 에 추가
        pools.Add(prefab, pool);
    }

    // Pool 에서 GameObject 를 꺼내오는 함수 
    public GameObject Spawn(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out Stack<GameObject> pool))
        {
            GameObject instance;

            // Pool 에 재고가 있을 경우 : 꺼내기
            if (pool.Count > 0)
                instance = pool.Pop();
            // 없을 경우 : prefab 인스턴스화
            else
                instance = Instantiate(prefab, transform);

            instance.SetActive(true); // 해당 instance 를 활성화
            instanceDict.Add(instance, prefab); // 역추적을 위해 instance 가 어떤 Prefab 에서 왔는지 기록
            return instance;
        }
        else
        {
            Debug.Log("해당 Prefab 에 대한 Pool 이 존재하지 않습니다");
            return null;
        }
    }

    // GameObject 를 다시 Pool 에 집어넣는 함수
    public void Despawn(GameObject instance)
    {
        // 역추적 Dictionary 를 활용해 해당 인스턴스의 prefab 을 찾음 
        if (instanceDict.TryGetValue(instance, out GameObject prefab))
        {
            instance.SetActive(false);
            pools[prefab].Push(instance);
            instanceDict.Remove(instance);
        }
        // 없다면 에러 출력 후 해당 인스턴스를 파괴
        else
        {
            Debug.Log("풀링되지 않은 Instance 를 반납 시도하였습니다");
            Destroy(instance);
            return;
        }
    }
}