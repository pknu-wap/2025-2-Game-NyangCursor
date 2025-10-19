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
    }

    [Header("플레이어 자식으로 둘 9개의 스킬 매니저")]
    [SerializeField] private List<SkillSlot> skillSlots = new();

    void Awake()
    {
        foreach (var slot in skillSlots)
        {
            if (slot.skillManagerObject != null)
            {
                var tempMgr = slot.skillManagerObject.GetComponent<ISkillUpgradable>();
                if (tempMgr != null)
                {
                    tempMgr.SetSkillName(slot.skillName);
                }
            }
        }
    }

    public void UnlockSkill(string skillName)
    {
        var slot = skillSlots.Find(s => s.skillName == skillName);
        if (slot == null)
        {
            Debug.LogWarning($"스킬 '{skillName}'을(를) 찾을 수 없습니다.");
            return;
        }

        if (slot.isUnlocked)
        {
            Debug.Log($"스킬 '{skillName}'은(는) 이미 해금됨.");
            return;
        }

        slot.isUnlocked = true;
        slot.skillManagerObject.SetActive(true);

        Debug.Log($"스킬 '{skillName}' 활성화됨!");
    }

    public string GetRandomLockedSkill()
    {
        var locked = skillSlots.FindAll(s => !s.isUnlocked);
        if (locked.Count == 0)
        {
            Debug.Log("모든 스킬이 이미 해금됨!");
            return null;
        }

        int rand = UnityEngine.Random.Range(0, locked.Count);
        return locked[rand].skillName;
    }

    public SkillSlot GetSkillSlot(string skillName)
    {
        return skillSlots.Find(s => s.skillName.Equals(skillName, StringComparison.OrdinalIgnoreCase));
    }

}
