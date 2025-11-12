using UnityEngine;

public class NormalPot : Pot
{
    [Header("인터페이스 구현체를 참조하는 변수")]
    [SerializeField] private InterfaceReference<IDamageable> damageable;
    [SerializeField] private InterfaceReference<IDroppable> droppable;

    public IDamageable Damageable => damageable?.TargetInterface;
    public IDroppable Droppable => droppable?.TargetInterface;

    private void OnEnable()
    {
        Damageable.OnDeath += Break;
        Damageable.Initialize(this);
        Droppable.Initialize(this);
    }

    private void OnDisable()
    {
        Damageable.OnDeath -= Break;
    }

    // 항아리가 파괴되었을 때 실행할 콜백 함수
    public override void Break()
    {
        Droppable?.Drop();
        PoolManager.instance.Despawn(this.gameObject);
    }
}
