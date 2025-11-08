using UnityEngine;

public class GoldFollower : MonoBehaviour
{
    private Transform target;
    private float speed = 10f;

    // 풀에서 되돌아갈 때, target 을 초기화
    private void OnDisable()
    {
        target = null;
    }

    public void SetTarget(Transform player, float moveSpeed)
    {
        target = player;
        speed = moveSpeed;
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }
}
