using UnityEngine;

public class SuicideHpController : NormalHpController
{
    public override void Initialize(Component owner)
    {
        base.Initialize(owner);
        SubscribeEvents();
    }

    public override void Cleanup()
    {
        UnsubscribeEvents();
        base.Cleanup();
    }

    private void SubscribeEvents()
    {
        owner.EventBus.Subscribe(EnemyEventType.OnSuicide, HandleSuicide);
    }

    private void UnsubscribeEvents()
    {
        owner.EventBus.Unsubscribe(EnemyEventType.OnSuicide, HandleSuicide);
    }

    private void HandleSuicide()
    {
        Suicide();
    }

    private void Suicide()
    {
        if (isDead)
            return;

        isDead = true;

        // 로컬 이벤트 버스만 발행 → 풀링, 드랍 처리
        owner.EventBus.Publish(EnemyEventType.OnDeath);

        // 글로벌 이벤트는 발행 안함 → 오버드라이브 게이지 증가는 안함
    }
}
