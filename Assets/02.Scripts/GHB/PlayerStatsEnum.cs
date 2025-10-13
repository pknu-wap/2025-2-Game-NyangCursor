public enum StatType
{
    // ================================
    // 자원 순환형 (Resource Loop Stat)
    // ================================

    /// <summary>적 처치 시 OD 게이지 획득량 증가</summary>
    ODGainOnKill,

    /// <summary>충돌 시 OD 게이지 감소량 완화</summary>
    ODLossOnHit,

    /// <summary>공격 시 탑승 게이지 획득량 증가</summary>
    RidingGaugeGainOnAttack,

    /// <summary>폭주 모드 지속 시간 증가</summary>
    BerserkModeDurationUp,

    /// <summary>오버드라이브 진입 시 초기 게이지 증가</summary>
    InitialODBoost,

    /// <summary>오버드라이브 자연 감소 속도 완화</summary>
    ODDrainRateDown,

    // ================================
    // 경제·성장형 (Economy & Growth Stat)
    // ================================

    /// <summary>경험치 획득량 증가</summary>
    ExpGainUp,

    /// <summary>코인 획득량 증가</summary>
    CoinGainUp,

    /// <summary>상자 드랍 확률 증가</summary>
    ChestDropRateUp,

    /// <summary>자력(아이템 흡입 범위) 증가</summary>
    MagnetRangeUp,

    /// <summary>최대 체력 증가</summary>
    MaxHealthUp,

    // ================================
    // 🏃 이동·기동형 (Movement & Control Stat)
    // ================================

    /// <summary>기본 이동속도 증가</summary>
    MoveSpeedUp,

    /// <summary>오버드라이브 이동속도 증가</summary>
    OverdriveMoveSpeedUp,

    /// <summary>회전력 증가</summary>
    RotationPowerUp,

    // ================================
    // 범용 속성강화 (Universal Elemental Buff)
    // ================================

    /// <summary>모든 속성 피해 증가</summary>
    AllElementDamageUp,

    /// <summary>모든 속성 범위 증가</summary>
    AllElementRangeUp,

    /// <summary>모든 속성 지속시간 증가</summary>
    AllElementDurationUp,

    /// <summary>모든 속성 쿨타임 감소</summary>
    AllElementCooldownDown,

    /// <summary>모든 속성 초당 공격 횟수 증가</summary>
    AllElementAttackSpeedUp,

    // ================================
    // 특정 속성강화 (Specific Elemental Buff)
    // ================================

    // ----- Fire (불의 고리)
    FireRingDamageUp,
    FireRingRangeUp,
    FireRingDurationUp,
    FireRingCooldownDown,
    FireRingAttackSpeedUp,

    // ----- Water (물의 잔상)
    WaterTrailDamageUp,
    WaterTrailRangeUp,
    WaterTrailDurationUp,
    WaterTrailCooldownDown,
    WaterTrailAttackSpeedUp,

    // ----- Lightning (번개의 질주)
    LightningDashDamageUp,
    LightningDashRangeUp,
    LightningDashDurationUp,
    LightningDashCooldownDown,
    LightningDashAttackSpeedUp,

    // ----- Wind (바람의 폭풍)
    WindStormDamageUp,
    WindStormRangeUp,
    WindStormDurationUp,
    WindStormCooldownDown,
    WindStormAttackSpeedUp,

    // ================================
    // 특정 속성강화 (Specific Elemental Buff)
    // ================================
    TestDemon1,
    TestDemon2, 
    TestDemon3


}
