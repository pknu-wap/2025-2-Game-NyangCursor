using UnityEngine;

public abstract class FieldObject : MonoBehaviour
{
    [Header("인터페이스 구현체를 참조하는 변수")]
    [SerializeField] private InterfaceReference<IDamageable> damageable;
    [SerializeField] private InterfaceReference<IDroppable> droppable;

    [Header("이 객체에 사용되는 로컬 이벤트 버스")]
    public FieldObjectEvents EventBus;

    private void Awake()
    {
        EventBus = new FieldObjectEvents();
    }

    public IDamageable Damageable => damageable?.TargetInterface;
    public IDroppable Droppable => droppable?.TargetInterface;

    private void OnEnable()
    {
        // 메모리 누수를 방지하기 위해 이벤트 버스 초기화
        EventBus?.Reset();

        // 이벤트 구독
        EventBus.Subscribe(FieldObjectEventType.OnBreak, Break);

        Damageable.Initialize(this);
        Droppable.Initialize(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(FieldObjectEventType.OnBreak, Break);

        Damageable.Cleanup();
        Droppable.Cleanup();

        // 메모리 누수를 방지하기 위해 이벤트 버스 초기화
        EventBus?.Reset();
    }

    // 필드 오브젝트가 깨졌을때 실행하는 함수
    public abstract void Break();
}
