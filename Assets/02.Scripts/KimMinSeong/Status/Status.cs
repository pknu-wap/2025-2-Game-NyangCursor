using UnityEngine;
using System;

// 상태 종류
public enum StatusType
{
    Knockback,
    Stun,
    /*
     * 필요시 추가
     */
}

// 상태 효과 데이터 클래스
[Serializable]
public class StatusEffectData
{
    [Header("필수 데이터")]
    public StatusType statusType;
    public float duration;

    [Header("선택 데이터 / 확장 가능")]
    public bool blocksMovement = false;
    /*
     * 필요시 추가
     */

    // 기본 생성자
    public StatusEffectData(StatusType type, float duration)
    {
        this.statusType = type;
        this.duration = duration;
    }

    public static StatusEffectData Knockback(float duration)
    {
        // 1. 생성자를 호출하여 필수 데이터 설정하여 새 객체 생성
        return new StatusEffectData(StatusType.Knockback, duration)
        {
            // 2. 선택 데이터를 초기화
            blocksMovement = true
        };
    }

    /*
     * 필요시 추가
     */
}
