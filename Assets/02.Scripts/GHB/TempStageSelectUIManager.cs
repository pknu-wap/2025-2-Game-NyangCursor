using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// 임시 UI ON/OFF용 스크립트, 나중에 LOBBYUI 매니저로 한번에 병합해야 함
public class TempStageSelectUIManager : MonoBehaviour
{
    [SerializeField] private GameObject stageUI;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            stageUI.SetActive(!stageUI.activeSelf);
        }
    }

    // 테스트용 pref 삭제 함수
    public void ResetPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    // 임시 씬전환 함수
    public void SceneChange()
    {
        SceneManager.LoadScene("StageManagerMakeScene");
    }
}
