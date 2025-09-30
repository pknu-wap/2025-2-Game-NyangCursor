using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TempStageSelectManager : MonoBehaviour
{
    [Header("스테이지 버튼")]
    [SerializeField] private List<Button> stageButtons; // 0: stage1, 1: stage2, ...

    private void OnEnable()
    {
        for (int i = 0; i < stageButtons.Count; i++)
        {
            if (i == 0)
            {
                // 첫 스테이지는 무조건 열려 있음
                stageButtons[i].interactable = true;
            }
            else
            {
                // 그 이후 스테이지는 이전 스테이지의 클리어 여부에 따라 해금
                stageButtons[i].interactable = PlayerPrefs.GetInt($"StageCleared_{i}", 0) == 1;
            }
        }
    }

}
