using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingSceneManager : MonoBehaviour
{
    public static string nextSceneName = "Level_1"; 

    [Header("⚙️ 로딩 설정")]
    [Tooltip("로딩이 아무리 빨라도 이 시간만큼은 대기합니다 (초 단위)")]
    public float minLoadingTime = 2.0f; 

    [Header("🖼️ UI 연결")]
    public Slider loadingBar;
    public TextMeshProUGUI loadingText;

    private void Start()
    {
        StartCoroutine(LoadSceneProcess());
    }

    IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
        op.allowSceneActivation = false; // 로딩이 끝나도 바로 넘어가지 않게 막음

        float timer = 0f;

        // 로딩이 완료되지 않았거나(op.progress < 0.9), 
        // 설정한 최소 시간이 지나지 않았다면(timer < minLoadingTime) 계속 반복
        while (!op.isDone)
        {
            yield return null; // 1프레임 대기

            timer += Time.deltaTime;

            // 1. 실제 로딩 진행률 (0.0 ~ 1.0)
            // op.progress는 최대 0.9까지만 오르므로 0.9로 나눠서 1.0으로 보정
            float loadingProgress = op.progress / 0.9f; 

            // 2. 시간 진행률 (0.0 ~ 1.0)
            // 흐른 시간을 목표 시간으로 나눔
            float timeProgress = Mathf.Clamp01(timer / minLoadingTime);

            // [핵심] 둘 중 *더 느린* 쪽을 기준으로 로딩 바를 채웁니다.
            // 로딩이 빨라도 시간이 안 됐으면 천천히 차오르고,
            // 시간이 다 됐어도 로딩이 안 됐으면 기다립니다.
            float targetValue = Mathf.Min(loadingProgress, timeProgress);

            // 부드럽게 슬라이더 움직임
            loadingBar.value = Mathf.Lerp(loadingBar.value, targetValue, Time.deltaTime * 5f);

            // 로딩도 끝났고(0.9 이상) && 최소 시간도 지났다면(timeProgress 1.0)
            if (op.progress >= 0.9f && timeProgress >= 1.0f)
            {
                // 바를 꽉 채우고 씬 전환
                loadingBar.value = 1.0f;
                op.allowSceneActivation = true;
            }
        }
    }

    public static void LoadScene(string sceneName)
    {
        nextSceneName = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }
}