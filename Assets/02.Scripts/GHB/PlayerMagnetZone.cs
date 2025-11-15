using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PlayerMagnetZone : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float pullSpeed = 10f;
    [SerializeField] private float radius = 2f;

    private CircleCollider2D circleCollider;

    private void Start()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = radius;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 공통 follower 컴포넌트 찾기
        BaseCollectibleFollower follower = other.GetComponent<BaseCollectibleFollower>();
        if (follower == null) return;

        follower.SetTarget(player, pullSpeed);
    }
}
