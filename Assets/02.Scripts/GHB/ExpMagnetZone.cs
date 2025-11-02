using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ExpMagnetZone : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private LayerMask expLayer;
    [SerializeField] private Transform player;
    [SerializeField] private float pullSpeed = 10f;

    [Header("자기장 범위")]
    [SerializeField] private float radius = 2f; // 인스펙터에서 조정 가능
    [SerializeField] private CircleCollider2D circleCollider;

    private void Start()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = radius;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!expLayer.Contains(other.gameObject)) return;

        ExpFollower exp = other.GetComponent<ExpFollower>();
        if (exp != null)
            exp.SetTarget(player, pullSpeed);
    }
}
