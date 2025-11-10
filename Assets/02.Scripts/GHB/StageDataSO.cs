using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game Data/Stage Data")]
public class StageDataSO : ScriptableObject
{
    [Header("스테이지 기본 정보")]
    public string stageName;                  // 스테이지 이름
    [TextArea] public string stageDescription; // 설명

    [Header("대표 이미지")]
    public Sprite stageImage;                 // UI에 표시할 스테이지 대표 이미지

    [Header("이동할 씬 이름")]
    public string sceneName;                  // 버튼 클릭 시 이동할 씬 이름

    // 등장 몬스터 이름. 지금은 인스펙터에서 string으로 해뒀지만, 추후 몬스터 SO가 정립되면 인스펙터에서 SO를 가져와서 할당해 NAME만 가져오기로 가능
    [Header("출현 몬스터 목록")]
    public List<string> monsterList = new List<string>();

    [Header("난이도")]
    [Range(1, 10)]
    public int difficulty = 1;                // 난이도 (1~10 등급)
}
