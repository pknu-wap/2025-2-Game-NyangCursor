using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("기본 정보")]
    public string enemyName;
    [TextArea]
    public string description;

    [Header("능력치")]
    public int hp = 5;
    public float speed = 2f;
    public int attackPower = 2;

    [Header("프리펩")]
    public GameObject prefab;
}
