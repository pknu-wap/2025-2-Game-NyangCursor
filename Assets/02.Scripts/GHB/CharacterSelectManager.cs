using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("캐릭터 데이터 리스트")]
    public List<CharacterDataSO> characterList = new List<CharacterDataSO>();

    [Header("UI 참조")]
    [SerializeField] private GameObject characterSelectUI;
    [SerializeField] private Transform slotParent;       // 슬롯들이 들어갈 부모
    [SerializeField] private GameObject slotPrefab;      // 슬롯 프리팹

    [Header("설명창 오브젝트")]
    [SerializeField] private Image selectedCharacterIcon;
    [SerializeField] private TextMeshProUGUI selectedCharacterName;
    [SerializeField] private TextMeshProUGUI selectedCharacterDescription;
    [SerializeField] private Image selectedSkillIcon;
    [SerializeField] private TextMeshProUGUI selectedSkillName;
    [SerializeField] private TextMeshProUGUI selectedSkillDescription;
    [SerializeField] private Image selectedPassiveIcon;
    [SerializeField] private TextMeshProUGUI selectedPassiveName;
    [SerializeField] private TextMeshProUGUI selectedPassiveDescription;

    [Header("선택 버튼")]
    [SerializeField] private Button selectButton;

    [Header("스테이지 선택 UI")]
    [SerializeField] private GameObject stageSelectUI;

    [Header("로비 UI")]
    [SerializeField] private GameObject lobbyUI;

    private void Start()
    {
        characterSelectUI.SetActive(false); // 기본은 비활성화
    }

    // 시작 버튼에서 이 함수 호출
    public void OpenUI()
    {
        characterSelectUI.SetActive(true);
        lobbyUI.SetActive(false);
        PopulateCharacterSlots();
    }

    public void CloseUI()
    {
        characterSelectUI.SetActive(false);
        lobbyUI.SetActive(true);
    }

    // 캐릭터 목록 표시
    private void PopulateCharacterSlots()
    {
        // 이미 있던 슬롯 제거
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        // 새 슬롯 생성
        foreach (var data in characterList)
        {
            GameObject slot = Instantiate(slotPrefab, slotParent);
            var button = slot.transform.Find("Icon").GetComponent<Button>();
            var iconImage = slot.transform.Find("Icon").GetComponent<Image>();

            iconImage.sprite = data.characterImage;

            button.onClick.AddListener(() => OnCharacterSelected(data));
        }

        // 첫번째 캐릭터 자동 선택
        if (characterList.Count > 0)
            OnCharacterSelected(characterList[0]);
    }




    // 캐릭터 선택 시 호출
    private void OnCharacterSelected(CharacterDataSO selected)
    {
        selectedCharacterIcon.sprite = selected.characterImage;
        selectedCharacterName.text = selected.characterName;
        selectedCharacterDescription.text = selected.characterDescription;

        selectedSkillIcon.sprite = selected.startingSkill.icon;
        selectedSkillName.text = selected.startingSkill.optionName;
        selectedSkillDescription.text = selected.startingSkill.description;

        /* 추후 자료구조를 어떻게 할지에 따라 갱신 로직 바뀔 수 있음
        selectedPassiveIcon.sprite

        */

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => CharacterConfirm(selected));
    }

    private void CharacterConfirm(CharacterDataSO selected)
    {
        SelectedChararcterDataManager.instance.selectedStartData = selected.startingSkill;
        stageSelectUI.SetActive(true);
        characterSelectUI.SetActive(false);
    }
}
