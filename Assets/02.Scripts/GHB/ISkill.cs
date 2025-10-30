using System.Collections.Generic;

public enum SkillType
{
    Active, // 액티브형
    Passive // 패시브형
}

public interface ISkill
{
    // 스킬들의 현재 레벨
    int CurrentLevel { get; set; }
    // 스킬 타입 읽기 전용 속성
    SkillType SkillType { get; }
    // 스킬이 사용하는 스탯들
    List<SkillStatKey> UsedStats { get; }
    // 스킬 발동 함수
    void Activate();
    // 선택지 선택 시 발송되는 이벤트에 구독되는 함수
    void ApplyUpgrade(UpgradeEventData data);
    // 스킬명 설정 (playerskillmanager 인스펙터에서 스킬명 할당 -> 각 스킬 스크립트에 자동 할당)
    void SetSkill(string skillName);
    // 스킬 버릴 시 스탯 초기화 함수
    void ResetSkill();
    // 라이딩 / 일반 상태 감지 후 제어하는 로직. tempskillmanager에서 바로 가져다 쓰시면 됩니다
    void HandleStateChanged(PlayerStateLogic.PlayerState newState);
    // 스킬 사용 가능 상태인지 체크하는 함수. tempskillmanager에서 바로 가져다 쓰시면 됩니다
    bool IsSkillAllowed();
}
