using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("캐릭터 데이터 리스트")]
    public List<CharacterDataSO> characterList = new List<CharacterDataSO>();

    [Header("UI 참조")]
    [SerializeField] private GameObject characterSelectPanel;
    [SerializeField] private Transform slotParent;       // 슬롯들이 들어갈 부모
    [SerializeField] private GameObject slotPrefab;      // 슬롯 프리팹

    [Header("설명창 오브젝트")]
    [SerializeField] private GameObject descriptionFieldPanel;
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

    public static event Action OnLobbyRidingStart;


    private void Start()
    {
        characterSelectPanel.SetActive(false); // 기본은 비활성화
    }

    // 시작 버튼에서 이 함수 호출
    public void OpenUI()
    {
        characterSelectPanel.SetActive(true); //패널켜기

        PopulateCharacterSlots(); //캐릭터 슬롯에 프리팹 추가
    }

    public void CloseUI()
    {
        characterSelectPanel.SetActive(false);
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
        foreach (var data in characterList) //so 만큼 생성
        {
            GameObject slot = Instantiate(slotPrefab, slotParent);
            var button = slot.transform.Find("Icon").GetComponent<Button>();
            var iconImage = slot.transform.Find("Icon").GetComponent<Image>();

            //프리팹 데이터 매핑
            iconImage.sprite = data.characterImage;
            iconImage.SetNativeSize();
            button.onClick.AddListener(() => OnCharacterSelected(data));
        }
    }




    // 캐릭터 선택 시 호출
    private void OnCharacterSelected(CharacterDataSO selected)
    {
        descriptionFieldPanel.SetActive(true); //캐릭터 설명창 켜기
       
        selectedCharacterIcon.sprite = selected.characterImage;
        selectedCharacterIcon.SetNativeSize();

        selectedCharacterName.text = selected.characterName;
        selectedCharacterDescription.text = selected.characterDescription;

        selectedSkillIcon.sprite = selected.startingSkill.icon;
        selectedSkillIcon.SetNativeSize();
        selectedSkillName.text = selected.startingSkill.optionName;
        selectedSkillDescription.text = selected.startingSkill.description;

        /* 추후 자료구조를 어떻게 할지에 따라 갱신 로직 바뀔 수 있음
        selectedPassiveIcon.sprite

        */

        selectButton.onClick.RemoveAllListeners();
        //selectButton.onClick.AddListener(() => CharacterConfirm(selected));
        selectButton.onClick.AddListener(() => GameStart(selected));
    }

    //나중에 스테이지 추가 시 해당 함수사용
    private void CharacterConfirm(CharacterDataSO selected)
    {
       
        SelectedChararcterDataManager.instance.selectedStartData = selected.startingSkill;
        stageSelectUI.SetActive(true);//스테이지 선택창 켜기
        characterSelectPanel.SetActive(false);//캐릭터 선택창 끄기
    }

    private void GameStart(CharacterDataSO selected)
    {

        SelectedChararcterDataManager.instance.selectedStartData = selected.startingSkill;
        characterSelectPanel.SetActive(false);//캐릭터 선택창 끄기
        //고양이 커서 타는 연출 후
         OnLobbyRidingStart?.Invoke();
        //로드 씬 이동
       
    }


}
