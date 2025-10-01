
public enum StatType
{
    // ================================
    // 공격 관련 스탯 (순수 전투력 ↑)
    // ================================

    /// <summary>기본 공격력 증가</summary>
    AttackPower,

    /// <summary>공격 속도 증가 (공격 간격 감소)</summary>
    AttackSpeed,

    /// <summary>사거리 증가 (자동 발사 기준)</summary>
    AttackRange,

    /// <summary>탄환/공격 관통력 증가</summary>
    Penetration,

    /// <summary>발사 탄 수 증가 (한 번에 발사되는 탄 수)</summary>
    ProjectileCount,

    // ================================
    // 유틸리티 관련 스탯 (저점/편의/안정성 ↑)
    // ================================

    /// <summary>이동 속도 증가</summary>
    MoveSpeed,

    /// <summary>최대 체력 증가</summary>
    MaxHealth,

    /// <summary>획득 경험치 증가</summary>
    ExpGain,

    /// <summary>자력 증가 (예: 회전력/회전 모멘텀)</summary>
    Magnetism,

    /// <summary>아이템 획득 확률 증가</summary>
    ItemDropRate,

    /// <summary>적 처치 시 오버드라이브 게이지 상승량 증가</summary>
    OverdriveGainOnKill,

    /// <summary>충돌 시 오버드라이브 게이지 감소량 감소</summary>
    OverdriveLossOnHitReduction,

    /// <summary>폭주 모드 지속 시간 증가</summary>
    BerserkDuration,

    /// <summary>오버드라이브 회전력 증가</summary>
    OverdriveRotationPower,

    /// <summary>적 처치 시 탑승 게이지 증가량 증가</summary>
    MountGainOnKill,

    /// <summary>탑승 게이지 보정량 증가</summary>
    MountCorrection,

    // ================================
    // 속성 강화 관련 스탯 (속성 증강 및 시너지)
    // ================================

    /// <summary>상태이상 효과 지속시간 증가 (중복 통합)</summary>
    StatusEffectDuration,

    /// <summary>지속형 스킬 지속시간 증가 (불길/물 잔상 등)</summary>
    PersistentSkillDuration,

    /// <summary>속성 공격 쿨타임 감소</summary>
    ElementalCooldownReduction,

    // ================================
    // 악마 증강 (인게임 특수 증강)
    // ================================

    /// <summary>폭주 모드 시 충돌 시 광역 피해, 잔상 공격 등 추가 효과 발동</summary>
    BerserkCollisionEffect,

    /// <summary>속도에 따른 보상 (속도가 빠를수록 공격/점수 증가)</summary>
    SpeedRewardBonus,

    /// <summary>폭주 종료 시 특수 효과 발동 (폭발, 넉백 등)</summary>
    BerserkEndEffect,

    /// <summary>회전 저항 완전 제거 (자유로운 컨트롤 가능)</summary>
    NoRotationResistance
}
