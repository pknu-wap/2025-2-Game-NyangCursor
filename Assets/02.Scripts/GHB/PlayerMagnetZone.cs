using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PlayerMagnetZone : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private List<LayerMask> collectableLayers; // 자력이 적용되는 레이어
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
        bool isCollectable = false;
        foreach (LayerMask layer in collectableLayers)
        {
            if (layer.Contains(other.gameObject))
            {
                isCollectable = true;
                break;
            }
        }

        // 하나도 포함된 레이어가 없었다면 중지
        if (!isCollectable) 
            return;

        ExpFollower exp = other.GetComponent<ExpFollower>();
        GoldFollower gold = other.GetComponent<GoldFollower>();

        exp?.SetTarget(player, pullSpeed);
        gold?.SetTarget(player, pullSpeed);
    }
}
