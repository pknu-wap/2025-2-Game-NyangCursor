using UnityEngine;

public class ExpFollower : MonoBehaviour
{
    private Transform target;
    private float speed = 10f;
    void OnEnable()
    {
        ExpAltar.expAltarEvent += SetTarget;
    }

    void OnDisable()
    {
        target = null;
        ExpAltar.expAltarEvent -= SetTarget;
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
