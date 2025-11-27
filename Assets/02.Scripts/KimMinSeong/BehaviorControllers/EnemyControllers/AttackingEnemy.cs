using Unity.VisualScripting;
using UnityEngine;

public class AttackingEnemy : Enemy
{
    [Header("인터페이스 구현체를 참조하는 변수")]
    [SerializeField] private InterfaceReference<IAttackable> attackable;

    // 외부에서 인터페이스 메서드를 쉽게 접근할 수 있도록 하기 위한 프로퍼티들
    public IAttackable Attackable => attackable?.TargetInterface;

    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        Attackable.Initialize(this);
    }

    protected override void CleanupComponents()
    {
        base.CleanupComponents();
        Attackable.Cleanup();
    }
}