using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("=== 공유 데이터 ===")]

    [Space(5)]
    [Tooltip("적 이름")]
    public string enemyName = "example";

    [Space(5)]
    [Tooltip("적 아이콘")]
    public Sprite icon;

    [Space(5)]
    [Tooltip("처치 시 드롭되는 경험치량")]
    public int exp = 1;

    [Header("=== 개별 데이터 ===")]

    [Space(5)]
    [Tooltip("최대 체력")]
    public float maxHP = 50f;

    [Space(5)]
    [Tooltip("이동 속도")]
    public float moveSpeed = 3f;

    [Space(5)]
    [Tooltip("공격 데미지")]
    public float attackDamage = 5f;

    [Space(5)]
    [Tooltip("공격 쿨타임 (초 단위)")]
    public float attackRate = 0.5f;
}