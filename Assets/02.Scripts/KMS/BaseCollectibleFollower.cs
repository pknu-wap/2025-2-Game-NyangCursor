using UnityEngine;

public abstract class BaseCollectibleFollower : MonoBehaviour
{
    protected Transform target;
    protected Collider2D targetCollider;   // 콜라이더 추적
    protected float speed;
    protected Vector3 offset = Vector3.zero;

    public virtual void SetTarget(Transform target, float speed)
    {
        this.target = target;
        this.speed = speed;

        // 플레이어 Collider 저장
        targetCollider = target.GetComponent<Collider2D>();
    }

    protected virtual void OnDisable()
    {
        // 풀로 돌아갈 때 target 초기화 필수!!
        target = null;
        targetCollider = null;
    }

    protected virtual void Update()
    {
        if (target == null || targetCollider == null) return;

        // Collider 중심 + offset
        Vector3 targetPos = targetCollider.bounds.center + offset;

        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }

    protected abstract void Collect();
}
