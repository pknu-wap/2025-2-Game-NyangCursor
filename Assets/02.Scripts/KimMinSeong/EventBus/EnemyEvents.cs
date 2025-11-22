using System;
using System.Collections.Generic;

public enum EnemyEventType
{
    OnDeath,
    OnTakeDamage,
    OnKnockback,
    OnEnterEvadeRange,
    OnExitEvadeRange,
    OnEnterAttackRange,
    OnExitAttackRange,
    // 필요시 추가
}


// 적과 관련된 이벤트를 관리하는 이벤트 버스 클래스
public class EnemyEvents
{
    // Delegate 타입은 메서드를 참조할 수 있는 타입임. 즉, 함수 포인터 역할을 수행함
    // (이벤트 타입, 매개변수 타입) 을 키로 사용
    // 따라서 Dictionary 를 사용해 해당되는 키에 콜백 함수들을 매핑함
    private Dictionary<(EnemyEventType, Type), Delegate> events = new();

    // 매개변수가 있는 이벤트를 구독할 때 사용
    public void Subscribe<T>(EnemyEventType eventType, Action<T> handler)
    {
        var key = (eventType, typeof(T));
        if (!events.ContainsKey(key))
            events[key] = null;

        events[key] = (Action<T>)events[key] + handler;
    }

    // 매개변수가 없는 이벤트를 구독할 때 사용
    public void Subscribe(EnemyEventType eventType, Action handler)
    {
        var key = (eventType, typeof(void));
        if (!events.ContainsKey(key))
            events[key] = null;

        events[key] = (Action)events[key] + handler;
    }

    // 매개변수가 있는 이벤트를 구독 해제할 때 사용
    // ※ 메모리 누수를 방지하려면 꼭 Subscribe 를 호출한 객체에서 Unsubscribe 도 호출해주어야 함
    public void Unsubscribe<T>(EnemyEventType eventType, Action<T> handler)
    {
        var key = (eventType, typeof(T));

        // 해당 키에 대한 Value 가 없으면 스킵
        if (!events.TryGetValue(key, out var existing))
            return;

        // - 연산자로 handler 제거
        events[key] = (Action<T>)existing - handler;

        // 모든 구독자가 제거되었으면 키도 삭제
        if (events[key] == null)
            events.Remove(key);
    }

    // 매개변수가 없는 이벤트를 구독 해제할 때 사용
    // ※ 메모리 누수를 방지하려면 꼭 Subscribe 를 호출한 객체에서 Unsubscribe 도 호출해주어야 함
    public void Unsubscribe(EnemyEventType eventType, Action handler)
    {
        var key = (eventType, typeof(void));

        // 해당 키에 대한 Value 가 없으면 스킵
        if (!events.TryGetValue(key, out var existing))
            return;

        events[key] = (Action)existing - handler;

        // 모든 구독자가 제거되었으면 키도 삭제
        if (events[key] == null)
            events.Remove(key);
    }

    // 매개변수가 있는 이벤트를 발행할 때 사용
    public void Publish<T>(EnemyEventType eventType, T data)
    {
        var key = (eventType, typeof(T));
        if (events.TryGetValue(key, out var handler))
            (handler as Action<T>)?.Invoke(data);   // 해당 키에 저장된 Delegate 를 캐스팅 후 Invoke 호출. 등록된 모든 콜백 함수를 실행함
    }

    // 매개변수가 없는 이벤트를 발행할 때 사용
    public void Publish(EnemyEventType eventType)
    {
        var key = (eventType, typeof(void));
        if (events.TryGetValue(key, out var handler))
            (handler as Action)?.Invoke();  // 해당 키에 저장된 Delegate 를 캐스팅 후 Invoke 호출. 등록된 모든 콜백 함수를 실행함
    }

    // Dictionary 를 초기화하는 함수
    public void Reset()
    {
        events.Clear();
    }
}