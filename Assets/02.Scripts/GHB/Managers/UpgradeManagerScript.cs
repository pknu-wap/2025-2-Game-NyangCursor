using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    [Header("UI 슬롯 4개")]
    [SerializeField] private List<GameObject> slotObjects = new();

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

        // 중복 없는 4개 랜덤 선택
        for (int i = 0; i < 4; i++)
        {
            int index = UnityEngine.Random.Range(0, tempPool.Count);
            UpgradeOptionSO chosen = tempPool[index];
            tempPool.RemoveAt(index);
            currentSelection.Add(chosen);
            SetupSlot(slotObjects[i], chosen);
        }
    }

    private void SetupSlot(GameObject slotObj, UpgradeOptionSO data)
    {
        if (slotObj == null || data == null) return;

        TMP_Text text = slotObj.GetComponentInChildren<TMP_Text>();
        Button button = slotObj.GetComponentInChildren<Button>();

        float ratio = UnityEngine.Random.Range(data.minUpgradeRatio, data.maxUpgradeRatio);
        string percentText = $"{ratio * 100f:F1}%";

        SkillStatKey chosenStatKey = data.affectedStat;

        bool isUnlocked = true;

        if (data.isSkill)
        {
            var slot = playerSkillsManager.GetSkillSlot(data.optionName);
            isUnlocked = slot != null && slot.isUnlocked;

            ISkillUpgradable skillMgr = GetSkillManager(data.optionName);
            if (skillMgr != null && skillMgr.UsedStats.Count > 0)
            {
                int r = UnityEngine.Random.Range(0, skillMgr.UsedStats.Count);
                chosenStatKey = skillMgr.UsedStats[r];
            }
        }

        // UI 텍스트
        if (text != null)
        {
            string typeText = data.isSkill ? "[스킬]" : "[비전서]";
            string statText = chosenStatKey.ToString();

            if (data.isSkill && !isUnlocked)
            {
                text.text = $"{typeText} {data.optionName}\n<size=80%>해금되지 않은 스킬입니다.\n클릭 시 스킬 해금</size>";
            }
            else
            {
                text.text = $"{typeText} {data.optionName}\n<size=80%>{data.description}\n({statText} +{percentText})</size>";
            }
        }

        // 버튼 동작
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
    {
        if (data.isSkill && !isUnlocked)
        {
            // 해금 처리
            playerSkillsManager.UnlockSkill(data.optionName);

            // UI 닫기 위해 UpgradeEvent 발동 (dummy 값)
            var dummyEvent = new UpgradeEventData
            {
                isSkillUpgrade = true,
                skillName = data.optionName,
                statKey = chosenStatKey,
                upgradeRatio = 0f // 실제 값은 의미 없음
            };
            Debug.Log($"선택됨: {data.optionName}");
            OnUpgradeSelected?.Invoke(dummyEvent);
        }
        else
        {
            // 기존 업그레이드 이벤트
            var eventData = new UpgradeEventData
            {
                isSkillUpgrade = data.isSkill,
                skillName = data.isSkill ? data.optionName : null,
                statKey = chosenStatKey,
                upgradeRatio = ratio
            };
            Debug.Log($"선택됨: {(data.isSkill ? "스킬" : "비전서")} - {data.optionName} ({chosenStatKey}, +{percentText})");
            OnUpgradeSelected?.Invoke(eventData);
        }
    });

        }
    }


    // 인터페이스 기반으로 스킬 참조 가져오기
    private ISkillUpgradable GetSkillManager(string skillName)
    {
        if (playerSkillsManager == null)
        {
            Debug.LogError("PlayerSkillsManager 참조가 없습니다!");
            return null;
        }

        var slot = playerSkillsManager.GetSkillSlot(skillName);
        if (slot != null && slot.skillManagerObject != null)
        {
            // ISkillUpgradable 구현체를 가져옴
            return slot.skillManagerObject.GetComponent<ISkillUpgradable>();
        }

        return null;
    }
}
