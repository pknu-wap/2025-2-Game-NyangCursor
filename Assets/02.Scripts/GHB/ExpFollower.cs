using UnityEngine;

public class ExpFollower : MonoBehaviour
{
    private Transform target;
    private float speed;

    public void SetTarget(Transform player, float moveSpeed)
    {
        target = player;
        speed = moveSpeed;
    }

    private void Update()
    {
        if (target == null) return;

        // 플레이어를 향해 이동
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }
}
