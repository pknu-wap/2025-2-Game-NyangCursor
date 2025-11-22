using UnityEngine;

public class NormalPot : FieldObject
{
    // 항아리가 파괴되었을 때 실행할 콜백 함수
    public override void Break()
    {
        PoolManager.instance.Despawn(this.gameObject);
    }
}
