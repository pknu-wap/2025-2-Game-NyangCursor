using UnityEngine;
using System;

// 드랍 가능한 오브젝트의 추상 클래스
public abstract class DropObject : MonoBehaviour
{
    // 오브젝트를 먹었을때 실행하는 함수
    public abstract void OnCollected();
}

// 드랍 확률과 실제 프리펩을 묶어서 관리하는 클래스
[Serializable]
public class DropEntity
{
    [SerializeField] private GameObject dropPrefab; // 드랍하고자 하는 오브젝트의 프리펩
    [SerializeField][Range(0f, 100f)] private float dropRate;    // 드랍 확률

    // 외부에서 사용하기 위한 프로퍼티
    public GameObject DropPrefab => dropPrefab;
    public float DropRate => dropRate;
}