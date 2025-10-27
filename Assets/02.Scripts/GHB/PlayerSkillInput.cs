using System.Collections.Generic; // <- 추가
using UnityEngine;

public class PlayerSkillInput : MonoBehaviour
{
    [SerializeField] private PlayerSkillsManager playerSkillsManager;

    void Update()
    {
        var skills = playerSkillsManager.UnlockedSkills;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            TryActivateSkill(skills, 0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            TryActivateSkill(skills, 1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            TryActivateSkill(skills, 2);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            TryActivateSkill(skills, 3);
    }

    private void TryActivateSkill(IReadOnlyList<SkillSlot> list, int index)
    {
        Debug.Log($"Trying to activate skill at index {index}, total unlocked: {list.Count}");

        if (index >= list.Count)
        {
            Debug.Log("Index out of range.");
            return;
        }

        var slot = list[index];
        if (slot == null)
        {
            Debug.Log("Slot is null.");
            return;
        }

        if (slot.skillManagerObject == null)
        {
            Debug.Log($"SkillManagerObject for {slot.skillName} is null.");
            return;
        }

        var skill = slot.skillManagerObject.GetComponent<ISkill>();
        if (skill == null)
        {
            Debug.Log($"ISkill component missing on {slot.skillName}");
            return;
        }

        Debug.Log($"Skill found: {slot.skillName}, type: {skill.SkillType}");

        // 액티브형만 키 입력으로 발동
        if (skill.SkillType == SkillType.Active)
        {
            Debug.Log($"Activating skill: {slot.skillName}");
            skill.Activate();
        }
        else
        {
            Debug.Log($"Skill {slot.skillName} is not active type, skipping.");
        }
    }

}
