using UnityEngine;

public class GoldFollower : BaseCollectibleFollower
{
    protected override void Collect()
    {
        GoldDropObject drop = GetComponent<GoldDropObject>();
        drop?.OnCollected(); // 골드 증가 + 풀 복귀
    }
}