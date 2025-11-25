using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LoadingScene : MonoBehaviour
{
    [SerializeField]
    Image progressbar;
    static string NextScene;

    public static void Nextloading(string _NextScene)
    {
        NextScene = _NextScene;
        SceneManager.LoadScene("LoadScene");

    }


    void Start()
    {
        StartCoroutine(Loadsceneprocess());
    }

    IEnumerator Loadsceneprocess()
    {
        //다음씬을 로딩씬에서 미리 로드함 
        AsyncOperation op = SceneManager.LoadSceneAsync(NextScene);

        op.allowSceneActivation = false;
        float timer = 0f;
        while (!op.isDone)
        {
            yield return null;

            if (op.progress < 0.9f)
            {
                progressbar.fillAmount = op.progress;
            }
            else
            {
                timer += Time.unscaledDeltaTime * 0.5f;
                //진행바가 1초동안 90프로에서 100프로로 보여지기위함
                progressbar.fillAmount = Mathf.Lerp(0.9f, 1, timer);
                if (progressbar.fillAmount >= 1f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }

        }


    }







}
