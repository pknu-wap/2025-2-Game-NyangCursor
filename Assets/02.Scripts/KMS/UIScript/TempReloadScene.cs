using UnityEngine;
using UnityEngine.SceneManagement;

public class TempReloadScene : MonoBehaviour
{
    public void ReloadCurrentScene()
    {
        // 현재 활성화된 씬 가져오기
        Scene currentScene = SceneManager.GetActiveScene();

        // 현재 씬 다시 로드
        SceneManager.LoadScene(currentScene.name);
        Time.timeScale = 1.0f;
    }

    public void MoveLobby()
    {
        SceneManager.LoadScene("LobbySceneFinal");
        SceneManager.LoadScene("LobbySceneFinal");
        Time.timeScale = 1.0f;
    }
}