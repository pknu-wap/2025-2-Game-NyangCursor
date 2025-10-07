using UnityEngine;
using System;

[RequireComponent(typeof(CircleCollider2D))]
public class CombatZoneTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private CircleCollider2D combatZoneCollider;

    // 적이 전투 영역을 벗어났을 때 발생하는 이벤트
    public static event Action<Transform> onEnemyExited;

    private void OnTriggerExit2D(Collider2D other)
    {
        if (enemyLayer.Contains(other.gameObject))
        {
            Debug.Log($"{other.name}이(가) 전투 영역을 벗어났습니다");
            onEnemyExited?.Invoke(other.transform);
        }
    }

    // 디버그용 - Combat Zone 시각화
    private void OnDrawGizmos()
    {
        if (combatZoneCollider == null)
            combatZoneCollider = GetComponent<CircleCollider2D>();

        if (combatZoneCollider != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // 빨간색 반투명

            Vector3 center = transform.position + (Vector3)combatZoneCollider.offset;
            float radius = combatZoneCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);

            Gizmos.DrawWireSphere(center, radius);
        }
    }
}