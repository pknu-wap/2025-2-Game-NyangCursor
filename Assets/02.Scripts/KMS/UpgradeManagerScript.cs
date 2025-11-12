using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum UpgradeRarity { Normal, Rare, Legendary }

public class UpgradeManager : MonoBehaviour
{
    [Header("업그레이드 슬롯 4개")]
    [SerializeField] private List<UpgradeSlotUI> slotPrefabObjects = new();

    [Header("업그레이드 풀 (스킬 + 비전서)")]
    [SerializeField] private List<UpgradeOptionSO> upgradePool = new();

    [Header("플레이어 스킬 관리자")]
    [SerializeField] private PlayerSkillsManager playerSkillsManager;

    [Header("등급 확률 (총합 100%)")]
    [Range(0, 100)] public float normalRate = 70f;
    [Range(0, 100)] public float rareRate = 25f;
    [Range(0, 100)] public float legendaryRate = 5f;

    public static event Action OnUpgradeFinished;

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

        // 스킬/비전서 등급 결정
        UpgradeRarity rarity = GetRandomRarity();

        // 스킬이 잠금 상태면 등급 무조건 Normal
        var slot = playerSkillsManager.GetSkillSlot(data.optionName);
        bool isUnlocked = slot != null && slot.isUnlocked;
        if (data.isSkill && !isUnlocked)
            rarity = UpgradeRarity.Normal;

        // 기본 강화 비율 계산
        float ratio = UnityEngine.Random.Range(data.minUpgradeRatio, data.maxUpgradeRatio);

        // 비전서라면 등급 배수 적용
        if (!data.isSkill)
        {
            float multiplier = rarity == UpgradeRarity.Rare ? 2f :
                               rarity == UpgradeRarity.Legendary ? 3f : 1f;
            ratio *= multiplier;
        }

        // 스탯 키 선정
        List<SkillStatKey> chosenStats = new();
        if (data.isSkill)
        {
            ISkill skillMgr = GetSkillManager(data.optionName);
            if (skillMgr != null && skillMgr.UsedStats.Count > 0)
            {
                int statCount = rarity switch
                {
                    UpgradeRarity.Rare => 2,
                    UpgradeRarity.Legendary => 3,
                    _ => 1
                };
                statCount = Mathf.Min(statCount, skillMgr.UsedStats.Count);

                // 중복 없는 랜덤 선택
                var shuffled = skillMgr.UsedStats.OrderBy(_ => UnityEngine.Random.value).ToList();
                chosenStats = shuffled.Take(statCount).ToList();
            }
        }
        else
        {
            chosenStats.Add(data.affectedStat);
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
            slotUI.rarityText.text = $"[{rarity}]";
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
            slotUI.statText.text = string.Join("\n", chosenStats.Select(s => $"{s} +{ratio * 100f:F1}%"));
        }

        // 버튼 클릭 이벤트
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    for (int i = 0; i < chosenStats.Count; i++)
                    {
                        bool levelUp = i == 0; // 첫 번째 이벤트만 레벨 증가
                        var eventData = CreateUpgradeEvent(data, chosenStats[i], ratio, isUnlocked, levelUp);
                        Debug.Log($"[Upgrade] 선택됨: {data.optionName} (등급: {rarity})");
                        OnUpgradeSelected1?.Invoke(eventData);
                    }
                    OnUpgradeFinished?.Invoke();
                });
            }

        }
    }

    private UpgradeRarity GetRandomRarity()
    {
        float roll = UnityEngine.Random.Range(0f, 100f);
        if (roll < normalRate)
            return UpgradeRarity.Normal;
        else if (roll < normalRate + rareRate)
            return UpgradeRarity.Rare;
        else
            return UpgradeRarity.Legendary;
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

    private UpgradeEventData CreateUpgradeEvent(UpgradeOptionSO data, SkillStatKey chosenStatKey, float ratio, bool isUnlocked, bool applyLevelUp = false)
    {
        if (data.isSkill && !isUnlocked)
        {
            playerSkillsManager.UnlockSkill(data.optionName, data.icon);
            return new UpgradeEventData
            {
                isSkillUpgrade = true,
                skillName = data.optionName,
                statKey = chosenStatKey,
                upgradeRatio = 0f,
                applyLevelUp = applyLevelUp
            };
        }

        return new UpgradeEventData
        {
            isSkillUpgrade = data.isSkill,
            skillName = data.isSkill ? data.optionName : null,
            statKey = chosenStatKey,
            upgradeRatio = ratio,
            applyLevelUp = applyLevelUp
        };
    }


    public void ApplySelectedStartSkill()
    {
        var selectedDataManager = SelectedChararcterDataManager.instance;
        if (selectedDataManager == null || selectedDataManager.selectedStartData == null)
            return;

        UpgradeOptionSO startData = selectedDataManager.selectedStartData;

        // 스킬 Unlock
        playerSkillsManager.UnlockSkill(startData.optionName, startData.icon);

        // UpgradeEventData 생성 후 이벤트 발생 (레벨업 포함)
        var skillMgr = GetSkillManager(startData.optionName);
        if (skillMgr != null && skillMgr.UsedStats.Count > 0)
        {
            var firstStat = skillMgr.UsedStats[0]; // 첫 번째 Stat만 적용
            var eventData = CreateUpgradeEvent(startData, firstStat, 0f, true, true);
            OnUpgradeSelected1?.Invoke(eventData);
        }
        // 중복 방지
        selectedDataManager.selectedStartData = null;
    }

}
