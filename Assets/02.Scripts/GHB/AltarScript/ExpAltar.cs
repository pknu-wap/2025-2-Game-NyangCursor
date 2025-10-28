using UnityEngine;

public class ExpAltar : AltarBase
{
    [Header("설정")]
    [SerializeField] private float pullRadius = 5f;
    [SerializeField] private LayerMask expLayer;
    [SerializeField] private float pullSpeed = 10f;

    public override void Execute(Transform player)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pullRadius, expLayer);

        foreach (Collider2D hit in hits)
        {
            //EXP 추적 스크립트 추가/활성화
            ExpFollower follower = hit.GetComponent<ExpFollower>();
            if (follower == null)
            {
                follower = hit.gameObject.AddComponent<ExpFollower>();
            }
            follower.SetTarget(player, pullSpeed);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }
}
