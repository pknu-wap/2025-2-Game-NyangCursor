using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemySpawnEntity
{
    public GameObject enemyPrefab;
    [Range(0f, 100f)] public int spawnWeight = 1;
}

[CreateAssetMenu(fileName = "DifficultyData", menuName = "Scriptable Objects/DifficultyData")]
public class DifficultyData : ScriptableObject
{
    [Header("이 난이도가 적용되는 시작 시간")]
    public float startTimeMinutes;

    [Header("이 난이도에서 생성 가능한 적 리스트")]
    public List<EnemySpawnEntity> spawnableEnemies;

    [Header("이 난이도에서 생성 관련 설정")]
    public float spawnPeriod = 1f;
    public int maxEnemies = 50;

    // 유틸 프로퍼티
    public float StartTimeSeconds => startTimeMinutes * 60f;
}
