using System;
using System.Collections.Generic;
using UnityEngine;

// GPT를 통한 예시 사용 스탯들
public enum SkillStatKey
{
    Damage,         // 데미지 배율 등
    Cooldown,       // 쿨타임 (초) — 비전서는 보통 이 값을 감소시킴
    Range,          // 범위 (거리)
    Duration,       // 지속시간 (초)
    ProjectileCount,// 발사체 수
    ProjectileSize, // 발사체 크기
    EffectZoneDuration, // 장판류 스킬의 유지 시간

    // 밑 3개는 레거시 예정
    Speed          // 투사체/이동 속도 
    // 필요하면 여기에 추가
}

public class PlayerSkillsManager : MonoBehaviour
{
    [Header("업그레이드 매니저")]
    [SerializeField] private UpgradeManager upgradeManager;

    [Header("플레이어 자식으로 둘 9개의 스킬 매니저")]
    [SerializeField] private List<SkillSlot> skillSlots = new();

    // 해금된 순서대로 저장
    private List<SkillSlot> unlockedSkills = new();
    public IReadOnlyList<SkillSlot> UnlockedSkills => unlockedSkills;

    void Awake()
    {
        foreach (var slot in skillSlots)
        {
            // 모든 스킬들에 대해 SO 값으로 클래스 값 여기에서 갱신
            slot.skillName = slot.linkedUpgradeSO.optionName;
            if (slot.skillManagerObject != null)
            {
                var tempMgr = slot.skillManagerObject.GetComponent<ISkill>();
                if (tempMgr != null)
                {
                    tempMgr.SetSkill(slot.skillName);
                }
            }
        }
        upgradeManager.ApplySelectedStartSkill();
    }

    public void UnlockSkill(string skillName, Sprite icon = null)
    {
        var slot = skillSlots.Find(s => s.skillName == skillName);
        if (slot == null || slot.isUnlocked) return;

        slot.isUnlocked = true;

        // 먼저 UI 생성
        if (icon != null)
            slot.icon = icon;
        if (slot.icon != null)
            SkillIconUIManager.Instance.AddSkillIcon(slot.skillName, slot.icon);

        // SetActive 후 TempSkillManager 활성화
        slot.skillManagerObject.SetActive(true);

        unlockedSkills.Add(slot);

        // 아이콘 순서 적용
        SkillIconUIManager.Instance.RefreshIcons(unlockedSkills);

        // 패시브 루프 시작 / 상태 반영
        ISkill skill = slot.skillManagerObject.GetComponent<ISkill>();
        skill.HandleStateChanged(PlayerStateLogic.Instance.CurrentState);
        skill.Activate();
    }



    public void LockSkill(string skillName)
    {
        var slot = skillSlots.Find(s => s.skillName == skillName);
        if (slot == null || !slot.isUnlocked) return;

        slot.isUnlocked = false;
        slot.skillManagerObject.SetActive(false);

        unlockedSkills.Remove(slot);

        SkillIconUIManager.Instance?.RemoveSkillIcon(slot.skillName);

        // 스탯 초기화
        var skillMgr = slot.skillManagerObject.GetComponent<ISkill>();
        if (skillMgr != null)
        {
            skillMgr.ResetSkill();
        }
    }

    public SkillSlot GetSkillSlot(string skillName)
    {
        return skillSlots.Find(s => s.skillName.Equals(skillName, StringComparison.OrdinalIgnoreCase));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            foreach (var ski in UnlockedSkills)
            {
                Debug.Log(ski.skillName);
            }
        }
    }
}
