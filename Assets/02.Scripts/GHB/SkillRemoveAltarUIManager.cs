using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

// 제단 UI에서 실행될 모든 로직을 담습니다.
public class SkillRemoveAltarUIManager : MonoBehaviour
{
    [Header("현재 활성화된 스킬 버튼 4개")]
    [SerializeField] private List<Button> skillButtons = new();
    [Header("플레이어 스킬 관리자")]
    [SerializeField] private PlayerSkillsManager playerSkillsManager;
    // 선택 후 강제로 UI를 닫게 하고 싶다면(제단을 1회용으로 만들거나 한다면)
    // 버튼 콜백에 이벤트 달아서 발송(FlowManager가 구독한 후 play로 강제 이동)
    public static event Action OnAltarEvent;

    void OnEnable()
    {
        RefreshSkillButtons();
    }

    public void RefreshSkillButtons()
    {
        var unlocked = playerSkillsManager.UnlockedSkills;

        for (int i = 0; i < skillButtons.Count; i++)
        {
            if (i < unlocked.Count)
            {
                var slot = unlocked[i];
                skillButtons[i].image.sprite = slot.icon;
                skillButtons[i].interactable = true;

                int index = i; // 클릭 이벤트 캡처
                skillButtons[i].onClick.RemoveAllListeners();
                skillButtons[i].onClick.AddListener(
                    () => OnSkillButtonClicked(index)
                    );
            }
            else
            {
                skillButtons[i].image.sprite = null;
                skillButtons[i].interactable = false;
                skillButtons[i].onClick.RemoveAllListeners();
            }
        }
    }

    private void OnSkillButtonClicked(int index)
    {
        var skill = playerSkillsManager.UnlockedSkills[index];
        Debug.Log($"스킬 '{skill.skillName}' 초기화!");

        // 선택된 스킬 비해금
        playerSkillsManager.LockSkill(skill.skillName);

        // UI 갱신
        RefreshSkillButtons();

        // 1회용으로 쓰고싶다면 해당 부분 유지
        CloseAltarUI();
    }

    public void CloseAltarUI()
    {
        OnAltarEvent.Invoke();
        gameObject.SetActive(false);
    }
}
