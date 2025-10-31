using System;
using System.Reflection;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("현재 적에 필요한 데이터")]
    [SerializeField] private EnemyData enemyData;

    [Header("인터페이스 구현체를 참조하는 변수")]
    [SerializeField] private InterfaceReference<IDamageable> damageable;
    [SerializeField] private InterfaceReference<IMoveable> moveable;
    [SerializeField] private InterfaceReference<IAttackable> attackable;
    [SerializeField] private InterfaceReference<ICollidable> collidable;
    [SerializeField] private InterfaceReference<IDroppable> droppable;


    // 외부에서 인터페이스 메서드를 쉽게 접근할 수 있도록 하기 위한 프로퍼티들
    public EnemyData Data => enemyData;
    public IDamageable Damageable => damageable?.TargetInterface;
    public IMoveable Moveable => moveable?.TargetInterface;
    public IAttackable Attackable => attackable?.TargetInterface;
    public ICollidable Collidable => collidable?.TargetInterface;
    public IDroppable Droppable => droppable?.TargetInterface;

    private void OnEnable()
    {
        SubscribeEvents();
        InitializeComponents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        CleanupComponents();
    }

    private void FixedUpdate()
    {
        Moveable.UpdateMovement(Time.fixedDeltaTime);
    }

    private void InitializeComponents()
    {
        // 각 인터페이스 구현체 초기화
        TryInvokeMethod(damageable, "Initialize", "Damageable");
        TryInvokeMethod(moveable, "Initialize", "Moveable");
        TryInvokeMethod(attackable, "Initialize", "Attackable");
        TryInvokeMethod(collidable, "Initialize", "Collidable");
        TryInvokeMethod(droppable, "Initialize", "IDroppable");
    }

    private void CleanupComponents()
    {
        // 정리 작업
        TryInvokeMethod(damageable, "Cleanup");
        TryInvokeMethod(moveable, "Cleanup");
        TryInvokeMethod(attackable, "Cleanup");
        TryInvokeMethod(collidable, "Cleanup");
        TryInvokeMethod(droppable, "Cleanup");
    }

    private void SubscribeEvents()
    {
        if (Damageable != null)
        {
            Damageable.OnDeath += HandleDeath;
        }
    }

    private void UnsubscribeEvents()
    {
        if (Damageable != null)
        {
            Damageable.OnDeath -= HandleDeath;
        }
    }

    // 적 사망 이벤트 핸들러
    private void HandleDeath()
    {
        // 드랍 처리
        Droppable?.Drop();

        // PoolManager를 통한 오브젝트 반환
        PoolManager.instance.Despawn(this.gameObject);
    }

    // 유효한 인터페이스 참조 변수에 대해서 주어진 인터페이스 메서드를 실행하는 함수
    private void TryInvokeMethod<TInterface>(
        InterfaceReference<TInterface> reference,
        string methodName,
        string componentName = null
    ) where TInterface : class
    {
        if (!IsValid(reference))
            return;

        try
        {
            // 컴파일 타임에 제네릭 타입인 TInterface 에 대해 해당 메서드가 있는지 확인할 수 없음
            // 따라서 런타임에 메서드 호출을 하도록 reflection 사용
            var component = reference.TargetInterface;
            var type = component.GetType();

            switch (methodName)
            {
                case "Initialize":
                    var initMethod = type.GetMethod("Initialize");
                        initMethod?.Invoke(component, new object[] { this });
                    break;
                case "Cleanup":
                    var cleanupMethod = type.GetMethod("Cleanup");
                        cleanupMethod?.Invoke(component, null);
                    break;
                default:
                    Debug.LogWarning($"알 수 없는 메서드 이름 '{methodName}'");
                    break;
            }
        }
        catch (System.Exception ex)
        {
            // Initialize 시에만 경고 출력
            if (componentName != null)
                Debug.LogWarning($"{componentName} 에 구현체가 할당되지 않았으므로 스킵: {ex.GetType().Name}");
        }
    }

    // 인터페이스 구현체가 할당되었는지를 검사하는 함수
    private bool IsValid<TInterface>(InterfaceReference<TInterface> reference) where TInterface : class
    {
        // null이거나 TargetObject가 파괴된 경우만 false
        return reference?.TargetObject != null;
    }
}