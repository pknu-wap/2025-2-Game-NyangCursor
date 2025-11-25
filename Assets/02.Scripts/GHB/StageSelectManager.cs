using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    [Header("맵 데이터 리스트")]
    public List<StageDataSO> stageList = new List<StageDataSO>();

    [Header("UI 참조")]
    [SerializeField] private GameObject stageSelectUI;
    [SerializeField] private Transform slotParent;       // 슬롯들이 들어갈 부모
    [SerializeField] private GameObject slotPrefab;      // 슬롯 프리팹

    [Header("설명창 오브젝트")]
    [SerializeField] private GameObject descriptionField;
    [SerializeField] private Image stageIcon;
    [SerializeField] private TextMeshProUGUI stageName;
    [SerializeField] private TextMeshProUGUI bestRecord;
    [SerializeField] private TextMeshProUGUI stageMonster;
    [SerializeField] private TextMeshProUGUI stageDifficult;


    [Header("시작 버튼")]
    [SerializeField] private Button startButton;



    private void Start()
    {
        stageSelectUI.SetActive(false); // 기본은 비활성화
        descriptionField.SetActive(false);
    }

    public void OpenUI()
    {
        stageSelectUI.SetActive(true);
        PopulateMapSlots();
    }

    public void CloseUI()
    {
        stageSelectUI.SetActive(false);
    }

    // 캐릭터 목록 표시
    private void PopulateMapSlots()
    {
        // 이미 있던 슬롯 제거
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        // 새 슬롯 생성
        for (int i = 0; i < stageList.Count; i++)
        {
            StageDataSO data = stageList[i];
            GameObject slot = Instantiate(slotPrefab, slotParent);

            var iconButton = slot.transform.Find("Icon").GetComponent<Button>();
            var iconImage = slot.transform.Find("Icon/MapIcon").GetComponent<Image>();
            var nameText = slot.transform.Find("Icon/MapName").GetComponent<TMP_Text>();
            var descText = slot.transform.Find("Icon/MapDescription").GetComponent<TMP_Text>();

            iconImage.sprite = data.stageImage;
            nameText.text = data.stageName;

            // 해금 여부 판별
            bool isUnlocked = (i == 0) || PlayerPrefs.GetInt($"StageCleared_{i}", 0) == 1;

            if (isUnlocked)
            {
                // 해금된 스테이지
                descText.text = data.stageDescription;
                iconButton.interactable = true;
                iconButton.onClick.AddListener(() => OnStageSelected(data));
            }
            else
            {
                // 비해금 스테이지
                descText.text = $"이전 스테이지 ({i}) 클리어 필요";
                iconButton.interactable = false;
            }
        }
    }




    private void OnStageSelected(StageDataSO selected)
    {
        descriptionField.SetActive(true);
        stageIcon.sprite = selected.stageImage;
        stageName.text = selected.stageName;

        string monsterPrefix = "등장 몬스터 : ";
        string difficultPrefix = "난이도 : ";

        if (!stageMonster.text.StartsWith(monsterPrefix))
            stageMonster.text = monsterPrefix;
        if (!stageDifficult.text.StartsWith(difficultPrefix))
            stageDifficult.text = difficultPrefix;

        stageMonster.text = monsterPrefix + string.Join(", ", selected.monsterList);

        stageDifficult.text = difficultPrefix + new string('●', selected.difficulty);

        // 버튼 리스너 갱신
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() => StageConfirm(selected));
    }


    private void StageConfirm(StageDataSO selected)
    {
        SceneManager.LoadScene(selected.sceneName);
    }
}
