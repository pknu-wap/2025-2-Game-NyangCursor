using UnityEngine;

public class FollowTargetNoRotation : MonoBehaviour
{
    [SerializeField] private Transform target; // 따라갈 대상

    void Update()
    {
        if (target == null) return;

        // 회전은 유지하고 위치만 즉시 따라감
        transform.position = target.position;
        // transform.rotation 은 그대로 두므로 회전 고정됨
    }
}
