using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("능력치")]
    public int maxHp = 5;
    public float speed = 2f;
    public int attackPower = 2;
}