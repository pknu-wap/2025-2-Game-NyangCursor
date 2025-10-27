using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Data.Common;

public class UpgradeManager : MonoBehaviour
{
    [Header("업그레이드 슬롯 4개")]
    [SerializeField] private List<UpgradeSlotUI> slotPrefabObjects = new();

    [Header("업그레이드 풀 (스킬 + 비전서)")]
    [SerializeField] private List<UpgradeOptionSO> upgradePool = new();

    [Header("플레이어 스킬 관리자")]
    [SerializeField] private PlayerSkillsManager playerSkillsManager;

    private List<UpgradeOptionSO> currentSelection = new();

    public static event Action<UpgradeEventData> OnUpgradeSelected;

    private void OnEnable()
    {
        GenerateRandomOptions();
    }

    private void GenerateRandomOptions()
    {
        currentSelection.Clear();

        if (upgradePool.Count < 4)
        {
            Debug.LogWarning("풀에 아이템이 4개 미만입니다!");
            return;
        }

        List<UpgradeOptionSO> tempPool = new(upgradePool);

        // 해금된 스킬이 4개 이상이면 새로운 스킬은 제외하고, 기존 스킬의 업그레이드만 표시됨
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
            currentSelection.Add(chosen);
            SetupSlot(slotPrefabObjects[i], chosen);
        }
    }


    private void SetupSlot(UpgradeSlotUI slotPrefab, UpgradeOptionSO data)
    {
        if (slotPrefab.slotObj == null || data == null) return;

        TMP_Text text = slotPrefab.slotObj.GetComponentInChildren<TMP_Text>();
        Button button = slotPrefab.slotObj.GetComponentInChildren<Button>();

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

        // 아이콘 배치
        if (data.icon != null && slotPrefab.icon != null)
        {
            slotPrefab.icon.sprite = data.icon;
        }

        // 배경색 설정(임시로 스킬은 하늘색, 비전서는 보라색)
        if (slotPrefab.background != null)
        {
            if (data.isSkill)
                slotPrefab.background.color = new Color(0.53f, 0.81f, 0.98f); // 하늘색
            else
                slotPrefab.background.color = new Color(0.6f, 0.4f, 0.8f); // 보라색
        }


        // UI 텍스트
        if (text != null)
        {
            string typeText = data.isSkill ? "[스킬]" : "[비전서]";
            string statText = chosenStatKey.ToString();
            string levelText = "";

            // 스킬인 경우 레벨 정보 표시
            if (data.isSkill)
            {
                ISkill skillMgr = GetSkillManager(data.optionName);
                if (skillMgr != null && isUnlocked)
                {
                    int currentLv = skillMgr.CurrentLevel;
                    levelText = $"\n<size=60%>{currentLv}Lv → {currentLv + 1}Lv</size>";
                }
            }

            if (data.isSkill && !isUnlocked)
            {
                text.text = $"{typeText} {data.optionName}\n<size=80%>해금되지 않은 스킬입니다.\n클릭 시 스킬 해금</size>";
            }
            else
            {
                text.text = $"{typeText} {data.optionName}{levelText}<size=80%>{data.description}\n({statText} +{percentText})</size>";
            }
        }


        // 버튼 동작
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                var eventData = CreateUpgradeEvent(data, chosenStatKey, ratio, isUnlocked);
                Debug.Log($"선택됨: {data.optionName}");
                OnUpgradeSelected?.Invoke(eventData);
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
