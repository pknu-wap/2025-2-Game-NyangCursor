using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillIconUIManager : MonoBehaviour
{
    public static SkillIconUIManager Instance { get; private set; }

    [SerializeField] private Transform iconParent;  // SkillIconPanel의 transform
    [SerializeField] private GameObject iconPrefab; // SkillIcon 프리팹

    private readonly List<Image> activeIcons = new List<Image>();
    private readonly Dictionary<string, SkillCooldownUI> cooldownUIs = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // 스킬 아이콘 추가 (획득)
    public void AddSkillIcon(string skillName, Sprite skillSprite)
    {
        // 중복 방지: 같은 이름의 아이콘이 이미 있으면 리턴
        if (activeIcons.Exists(i => i.name == skillName)) return;

        GameObject iconObj = Instantiate(iconPrefab, iconParent);
        iconObj.name = skillName;

        Image img = iconObj.GetComponent<Image>();
        img.sprite = skillSprite;
        img.color = Color.white;
        img.raycastTarget = false;

        activeIcons.Add(img);
        // 쿨타임 UI 연결
        SkillCooldownUI cdUI = iconObj.GetComponent<SkillCooldownUI>();
        if (cdUI != null)
            cooldownUIs[skillName] = cdUI;

    }

    // 스킬 아이콘 제거 (비해금 시)
    public void RemoveSkillIcon(string skillName)
    {
        var target = activeIcons.Find(i => i.name == skillName);
        if (target == null) return;

        activeIcons.Remove(target);
        Destroy(target.gameObject);
    }

    // 현재 순서를 unlockedSkills 기준으로 맞춰서 UI에 적용
    public void RefreshIcons(IReadOnlyList<SkillSlot> orderedSlots)
    {
        for (int i = 0; i < orderedSlots.Count; i++)
        {
            var slot = orderedSlots[i];
            var img = activeIcons.Find(x => x.name == slot.skillName);
            if (img != null)
            {
                img.transform.SetSiblingIndex(i);
            }
        }
    }

    public bool TryGetCooldownUI(string skillName, out SkillCooldownUI cdUI)
    {
        return cooldownUIs.TryGetValue(skillName, out cdUI);
    }
}
