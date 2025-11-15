using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerColliderMove : MonoBehaviour
{
    private CapsuleCollider2D capsule;

    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider2D>();
    }

    private void OnEnable()
    {
        GaugeRidingLogic.OnOverDriveEvent += HandleOverDriveCollider;
        GaugeOverdriveLogic.OnNormalEvent += HandleNormalCollider;
    }

    private void OnDisable()
    {
        GaugeRidingLogic.OnOverDriveEvent -= HandleOverDriveCollider;
        GaugeOverdriveLogic.OnNormalEvent -= HandleNormalCollider;
    }

    // 노말 모드 = offset.y = -0.7
    private void HandleNormalCollider()
    {
        Vector2 off = capsule.offset;
        off.y = -0.7f;
        capsule.offset = off;
    }

    // 오버드라이브 모드 = offset.y = 0
    private void HandleOverDriveCollider()
    {
        Vector2 off = capsule.offset;
        off.y = 0f;
        capsule.offset = off;
    }
}
