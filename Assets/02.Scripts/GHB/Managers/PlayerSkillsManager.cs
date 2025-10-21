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
    ProcChance,     // 상태이상 발동확률
    Speed,          // 투사체/이동 속도
    ManaCost,       // 소모 자원
    // 필요하면 여기에 추가
}


public class PlayerSkillsManager : MonoBehaviour
{
    [Serializable]
    public class SkillSlot
    {
        public string skillName;
        public GameObject skillManagerObject;
        [HideInInspector] public bool isUnlocked = false;
        [HideInInspector] public Sprite icon; // UI용 아이콘
    }

    [Header("플레이어 자식으로 둘 9개의 스킬 매니저")]
    [SerializeField] private List<SkillSlot> skillSlots = new();

    // 해금된 순서대로 저장
    private List<SkillSlot> unlockedSkills = new();
    public IReadOnlyList<SkillSlot> UnlockedSkills => unlockedSkills;

    void Awake()
    {
        foreach (var slot in skillSlots)
        {
            if (slot.skillManagerObject != null)
            {
                var tempMgr = slot.skillManagerObject.GetComponent<ISkill>();
                if (tempMgr != null)
                {
                    tempMgr.SetSkillName(slot.skillName);
                }
            }
        }
    }

    public void UnlockSkill(string skillName, Sprite icon = null)
    {
        var slot = skillSlots.Find(s => s.skillName == skillName);
        if (slot == null) return;

        if (slot.isUnlocked) return;

        slot.isUnlocked = true;
        slot.skillManagerObject.SetActive(true);

        if (icon != null)
            slot.icon = icon; // 여기서 아이콘 주입

        unlockedSkills.Add(slot);
    }

    public void LockSkill(string skillName)
    {
        var slot = skillSlots.Find(s => s.skillName == skillName);
        if (slot == null || !slot.isUnlocked) return;

        slot.isUnlocked = false;
        slot.skillManagerObject.SetActive(false);

        unlockedSkills.Remove(slot);

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

}
