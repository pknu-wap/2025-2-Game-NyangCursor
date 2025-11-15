using System;
using UnityEngine;

public class GoldDropObject : DropObject
{
    [SerializeField] private int goldAmount = 10;
    [SerializeField] private LayerMask collectableLayer;    // 이 오브젝트와 충돌 가능한 레이어

    public static event Action<int> OnGoldCollected;    // 이 오브젝트를 먹었을 때 발행하는 이벤트

    // 이 오브젝트를 먹었을 때 동작을 정의
    public override void OnCollected()
    {

        // 재화량을 담은 이벤트 발행
        OnGoldCollected?.Invoke(goldAmount);

        // 먹었다면 풀로 복귀
        PoolManager.instance.Despawn(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collectableLayer.Contains(collision.gameObject))
            OnCollected();
    }
}
