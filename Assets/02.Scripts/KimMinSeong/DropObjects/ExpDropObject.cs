using UnityEngine;

public class ExpDropObject : DropObject
{
    [SerializeField] private int expAmount = 10;    // 경험치량
    [SerializeField] private LayerMask collectableLayer; // 충돌시 획득 가능한 레이어

    // 이 오브젝트를 먹었을때 동작을 정의 
    public override void OnCollected()
    {
        Debug.Log("경험치를 먹었습니다"); // 디버그용
        
        // 경험치량만큼 경험치 매니저에게 값을 전달
        /* 구현 예정 */

        // 먹었다면 풀로 복귀
        PoolManager.instance.Despawn(this.gameObject);
    }

    // 충돌 발생시 OnCollected 를 실행
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // LayerMaskHelper 확장 메서드 사용
        if (collectableLayer.Contains(collision.gameObject))
            OnCollected();
    }
}
