using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("=== 공유 데이터 ===")]

    [Space(5)]
    [Tooltip("적 이름")]
    public string enemyName = "example";

    [Header("=== 개별 데이터 ===")]

    [Space(5)]
    [Tooltip("최대 체력")]
    public float maxHP = 50f;

    [Space(5)]
    [Tooltip("이동 속도")]
    public float moveSpeed = 3f;

    [Space(5)]
    [Tooltip("처치시 게이지 회복량")]
    public float overdriveGaugeReward = 10f;
}