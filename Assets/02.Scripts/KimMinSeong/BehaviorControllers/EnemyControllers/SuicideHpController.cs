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

        // 자폭 죽음 이벤트 발행 → 풀링만 처리하고 드랍은 안함
        // 글로벌 이벤트는 발행 안함 → 오버드라이브 게이지 증가는 안함
        owner.EventBus.Publish(EnemyEventType.OnSuicideDeath);
    }
}
