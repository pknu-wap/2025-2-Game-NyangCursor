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

    private void Start()
    {
        characterSelectUI.SetActive(false); // 기본은 비활성화
    }

    // 🔹 시작 버튼에서 이 함수 호출
    public void OpenUI()
    {
        characterSelectUI.SetActive(true);
        PopulateCharacterSlots();
    }

    // 🔹 캐릭터 목록 표시
    private void PopulateCharacterSlots()
    {
        foreach (var data in characterList)
        {
            GameObject slot = Instantiate(slotPrefab, slotParent);

            // 슬롯 내부 컴포넌트 찾기 (예: TMP_Text, Image)
            var nameText = slot.transform.Find("NameText").GetComponent<TMP_Text>();
            var iconImage = slot.transform.Find("Icon").GetComponent<Image>();
            var selectButton = slot.transform.Find("SelectButton").GetComponent<Button>();

            nameText.text = data.characterName;
            iconImage.sprite = data.characterImage;

            // 선택 버튼 클릭 시 해당 캐릭터 선택 처리
            selectButton.onClick.AddListener(() => OnCharacterSelected(data));
        }
    }

    // 🔹 캐릭터 선택 시 호출
    private void OnCharacterSelected(CharacterDataSO selected)
    {
        Debug.Log($"선택된 캐릭터: {selected.characterName}");
        // TODO: 선택 정보 저장 or 다음 씬으로 전달
        // e.g. GameManager.Instance.SetSelectedCharacter(selected);
    }
}
