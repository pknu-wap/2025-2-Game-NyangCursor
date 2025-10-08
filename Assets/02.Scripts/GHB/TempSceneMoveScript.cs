using UnityEngine;
using UnityEngine.SceneManagement;

// 임시 스크립트
public class TempSceneMoveScript : MonoBehaviour
{
    public void LoadPlayScene()
    {
        SceneManager.LoadScene("StageManagerMakeScene");
    }
}
