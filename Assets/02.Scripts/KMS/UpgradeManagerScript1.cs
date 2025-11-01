using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UpgradeManager1 : MonoBehaviour
{
    [Header("업그레이드 슬롯 4개")]
    [SerializeField] private List<UpgradeSlotUI> slotPrefabObjects = new();

    [Header("업그레이드 풀 (스킬 + 비전서)")]
    [SerializeField] private List<UpgradeOptionSO> upgradePool = new();

    [Header("플레이어 스킬 관리자")]
    [SerializeField] private PlayerSkillsManager playerSkillsManager;

    public static event Action<UpgradeEventData> OnUpgradeSelected1;

    private void OnEnable()
    {
        GenerateRandomOptions();
    }

    private void GenerateRandomOptions()
    {
        if (upgradePool.Count < 4)
        {
            Debug.LogWarning("풀에 아이템이 4개 미만입니다!");
            return;
        }

        List<UpgradeOptionSO> tempPool = new(upgradePool);

        // 해금된 스킬이 4개 이상이면 새로운 스킬 제외
        if (playerSkillsManager.UnlockedSkills.Count >= 4)
        {
            tempPool.RemoveAll(x => x.isSkill &&
                                    !playerSkillsManager.UnlockedSkills.Any(s => s.skillName == x.optionName));
        }

        // 중복 없는 4개 랜덤 선택
        for (int i = 0; i < 4; i++)
        {
            if (tempPool.Count == 0) break;

            int index = UnityEngine.Random.Range(0, tempPool.Count);
            UpgradeOptionSO chosen = tempPool[index];
            tempPool.RemoveAt(index);
            SetupSlot(slotPrefabObjects[i], chosen);
        }
    }

    private void SetupSlot(UpgradeSlotUI slotUI, UpgradeOptionSO data)
    {
        if (slotUI.slotObj == null || data == null) return;

        // 버튼은 자식에서 찾기 (기존 방식 유지)
        Button button = slotUI.slotObj.GetComponentInChildren<Button>();

        // 강화 수치 계산
        float ratio = UnityEngine.Random.Range(data.minUpgradeRatio, data.maxUpgradeRatio);
        string percentText = $"{ratio * 100f:F1}%";

        SkillStatKey chosenStatKey = data.affectedStat;
        bool isUnlocked = true;

        if (data.isSkill)
        {
            var slot = playerSkillsManager.GetSkillSlot(data.optionName);
            isUnlocked = slot != null && slot.isUnlocked;

            ISkill skillMgr = GetSkillManager(data.optionName);
            if (skillMgr != null && skillMgr.UsedStats.Count > 0)
            {
                int r = UnityEngine.Random.Range(0, skillMgr.UsedStats.Count);
                chosenStatKey = skillMgr.UsedStats[r];
            }
        }

        // 아이콘 설정
        if (data.icon != null && slotUI.icon != null)
        {
            slotUI.icon.sprite = data.icon;
        }

        // 이름 텍스트 설정
        if (slotUI.skillnameText != null)
        {
            string extension = data.isSkill ? ".exe" : ".dll";
            string displayName = $"[{data.optionName}{extension}]";
            slotUI.skillnameText.text = displayName;
        }

        // 레벨 텍스트 설정
        if (slotUI.levelText != null)
        {
            if (data.isSkill)
            {
                ISkill skillMgr = GetSkillManager(data.optionName);
                if (skillMgr != null && isUnlocked)
                {
                    slotUI.levelText.text = $"{skillMgr.CurrentLevel}Lv";
                }
                else
                {
                    slotUI.levelText.text = "Locked";
                }
            }
            else
            {
                slotUI.levelText.text = "-";
            }
        }

        // 스탯 텍스트 설정
        if (slotUI.statText != null)
        {
            string statKeyText = chosenStatKey.ToString();
            slotUI.statText.text = $"{statKeyText} +{percentText}";
        }

        // 버튼 클릭 이벤트
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                var eventData = CreateUpgradeEvent(data, chosenStatKey, ratio, isUnlocked);
                Debug.Log($"[Upgrade] 선택됨: {data.optionName}");
                OnUpgradeSelected1?.Invoke(eventData);
            });
        }
    }

    private ISkill GetSkillManager(string skillName)
    {
        if (playerSkillsManager == null)
        {
            Debug.LogError("PlayerSkillsManager 참조가 없습니다!");
            return null;
        }

        var slot = playerSkillsManager.GetSkillSlot(skillName);
        if (slot != null && slot.skillManagerObject != null)
        {
            return slot.skillManagerObject.GetComponent<ISkill>();
        }

        return null;
    }

    private UpgradeEventData CreateUpgradeEvent(UpgradeOptionSO data, SkillStatKey chosenStatKey, float ratio, bool isUnlocked)
    {
        if (data.isSkill && !isUnlocked)
        {
            playerSkillsManager.UnlockSkill(data.optionName, data.icon);
            return new UpgradeEventData
            {
                isSkillUpgrade = true,
                skillName = data.optionName,
                statKey = chosenStatKey,
                upgradeRatio = 0f
            };
        }

        return new UpgradeEventData
        {
            isSkillUpgrade = data.isSkill,
            skillName = data.isSkill ? data.optionName : null,
            statKey = chosenStatKey,
            upgradeRatio = ratio
        };
    }
}
