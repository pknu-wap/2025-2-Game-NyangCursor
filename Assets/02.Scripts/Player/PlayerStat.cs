using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    // 일반모드 계열
    [Header("이동계열")]
    public float walkSpeed;
    public float attackDamage;


    //연료 계열
    [Header("연료계열")]
    [Tooltip("연료 자연충전 속도 (초당). 20초 풀충전이면 5")]
    public float fuelRechargePerSec = 5f;
    [Tooltip("연료 소모 속도 (초당)")]
    public float fuelConsumePerSec = 10f;


    // 오버드라이브 계열
    [Header("Overdrive계열")]
    public float speed = 6f; //기본값 6
    [Range(1, 10)] public float turnRateDeg = 3f; //(1= 완전묵직, 3 = 시작값, 10 = 경쾌)
    [Tooltip("연료 1 소모당 오버드라이브 증가량 (기본 0.7)")]
    public float odGainPerFuel = 0.7f;
    [Tooltip("키를 뗀 뒤 감소 시작까지 지연(초)")]
    public float odDecayDelay = 3f;
    [Tooltip("감소 시작 후 초당 감소량")]
    public float odDecayPerSec = 8f;
    [Tooltip("순간 부스트 오브젝트가 켜져 있을 시간(초)")]
    public float boostOnDuration = 1f;
    [Tooltip("순간 부스트 추가 가속량")]
    public float boostExtraSpeed = 1.3f;


    // 생존 계열
    public int maxHP = 3;
    public float shield = 0f;

    // 메타 계열
    public float goldMultiplier = 1f; //골드획득량
    public float expMultiplier = 1f;  //경험치획득량

    // 나중에 확장 가능
}
