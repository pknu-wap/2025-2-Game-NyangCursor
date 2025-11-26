using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] Image progressbar;

    // 최소 로딩 화면 표시 시간 (초). 필요하면 인스펙터에서 조정.
    [SerializeField] float minDisplayTime = 1.0f;

    // 진행바가 목표값으로 따라갈 때의 속도. 작을수록 더 느리게 채움.
    [SerializeField] float fillSpeed = 0.8f;

    static string NextScene;
    float loadedAtTime = -1f; // op.progress가 0.9에 최초 도달한 시점(유니티 시간)

    public static void Nextloading(string _NextScene)
    {
        NextScene = _NextScene;
        SceneManager.LoadScene("LoadScene");
    }

    void Start()
    {
        // 안전장치: progressbar가 null이면 경고
        if (progressbar == null)
        {
            Debug.LogWarning("Progressbar Image is not assigned in the inspector.");
        }

        StartCoroutine(Loadsceneprocess());
    }

    IEnumerator Loadsceneprocess()
    {
        // 다음 씬을 비동기 로드
        AsyncOperation op = SceneManager.LoadSceneAsync(NextScene);
        op.allowSceneActivation = false;

        // 초기값
        progressbar.fillAmount = 0f;
        loadedAtTime = -1f;

        while (!op.isDone)
        {
            yield return null;

            // op.progress 는 0 ~ 0.9 까지 값이 옴. 0.9는 거의 로드 완료 상태 (씬 활성화 대기)
            if (op.progress < 0.9f)
            {
                // 목표 퍼센트: op.progress / 0.9 를 사용하면 0~1 범위로 정규화 가능(옵션)
                float target = op.progress / 0.9f;

                // 부드럽게 증가시키기 (언스케일드 시간 사용)
                progressbar.fillAmount = Mathf.MoveTowards(progressbar.fillAmount, target, fillSpeed * Time.unscaledDeltaTime);
            }
            else
            {
                // op.progress >= 0.9: 실제 로드는 끝났으나 allowSceneActivation == false 상태
                if (loadedAtTime < 0f) loadedAtTime = Time.unscaledTime; // 최초 도달 시점 기록

                // 경과 비율: 0 -> minDisplayTime 동안 0.9 -> 1.0 으로 보간
                float t = Mathf.Clamp01((Time.unscaledTime - loadedAtTime) / Mathf.Max(0.0001f, minDisplayTime));
                float value = Mathf.Lerp(0.9f, 1f, t);
                progressbar.fillAmount = Mathf.MoveTowards(progressbar.fillAmount, value, fillSpeed * Time.unscaledDeltaTime);

                // 둘 다 만족하면 씬 활성화
                if (t >= 1f && progressbar.fillAmount >= 1f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}
