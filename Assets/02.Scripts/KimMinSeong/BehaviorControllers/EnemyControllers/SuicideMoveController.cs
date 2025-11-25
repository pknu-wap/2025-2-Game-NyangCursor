using UnityEngine;

public class SuicideMoveController : NormalMoveController
{
    private bool isStopped = false;

    public override void Initialize(Component owner)
    {
        base.Initialize(owner);
        isStopped = false;  // 풀에서 생성될 때 플래그 초기화
    }

    protected override void SubscribeEvents()
    {
        base.SubscribeEvents();

        // 자폭 관련 이벤트 추가 구독
        owner.EventBus.Subscribe(EnemyEventType.OnEnterSuicideRange, StopForSuicide);
    }

    protected override void UnsubscribeEvents()
    {
        // 자폭 관련 이벤트 구독 해제
        owner.EventBus.Unsubscribe(EnemyEventType.OnEnterSuicideRange, StopForSuicide);

        base.UnsubscribeEvents();
    }

    private void StopForSuicide()
    {
        isStopped = true;
        Stop();
    }

    public override void UpdateMovement(float fixedDeltaTime)
    {
        // 자폭 준비 중이면 이동하지 않음
        if (isStopped)
        {
            Stop();
            return;
        }

        // 아니라면 기본 이동 로직 수행
        base.UpdateMovement(fixedDeltaTime);
    }
}