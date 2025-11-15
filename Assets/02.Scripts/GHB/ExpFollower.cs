using UnityEngine;

public class ExpFollower : BaseCollectibleFollower
{
    void OnEnable()
    {
        ExpAltar.expAltarEvent += SetTarget;
    }
    protected override void OnDisable()
    {
        base.OnDisable();  // 부모 로직 먼저 실행
        ExpAltar.expAltarEvent -= SetTarget;
    }

    protected override void Collect()
    {
        ExpDropObject drop = GetComponent<ExpDropObject>();
        drop?.OnCollected(); // 경험치 증가 + 풀 복귀
    }
}