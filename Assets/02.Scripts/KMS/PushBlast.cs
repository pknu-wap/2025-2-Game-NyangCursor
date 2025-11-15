using UnityEngine;

public class PushBlast : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float pushPower = 15f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Cooldown Settings")]
    [SerializeField] private float triggerCooldown = 0.5f;
    private float lastTriggerTime = -999f;

    private void OnEnable()
    {
        GaugeOverdriveLogic.OnBoostEvent += Trigger;
        GaugeOverdriveLogic.OnNormalEvent += Trigger;
    }

    private void OnDisable()
    {
        GaugeOverdriveLogic.OnBoostEvent -= Trigger;
        GaugeOverdriveLogic.OnNormalEvent -= Trigger;
    }

    // ------------------------------------------------------
    // Main blast trigger
    // ------------------------------------------------------
    public void Trigger()
    {
        if (Time.time - lastTriggerTime < triggerCooldown)
            return;

        lastTriggerTime = Time.time;

        float power = GetPowerByPlayerState();

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);

        foreach (var col in enemies)
        {
            var knockback = col.GetComponent<IKnockbackable>();
            if (knockback != null)
            {
                knockback.ApplyKnockback(transform.position, power);
            }
        }
    }

    // ------------------------------------------------------
    //  Player mode → pushPower 적용 방식
    // ------------------------------------------------------
    private float GetPowerByPlayerState()
    {
        switch (PlayerStateLogic.Instance.CurrentState)
        {
            case PlayerStateLogic.PlayerState.Normal:
                return pushPower * 0.3f;

            case PlayerStateLogic.PlayerState.OverDrive:
                return pushPower;

            default:
                return pushPower;
        }
    }

    // ------------------------------------------------------
    // Scene View에서만 반경 표시 (Game View에서는 안 보임)
    // ------------------------------------------------------
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // 주황 반투명
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
