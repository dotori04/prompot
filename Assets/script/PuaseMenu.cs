using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI 패널 연결")]
    public GameObject pauseMenuUI;   // 일시 정지 메인 화면 (버튼들 있는 곳)
    public GameObject settingsUI;    // 설정 화면 (슬라이더 등 있는 곳)

    [Header("설정")]
    public string mainMenuSceneName = "Main_Menu"; // 메인 메뉴 씬 이름

    private bool isPaused = false;

    void Start()
    {
        // 시작할 때 모든 패널 끄기
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsUI != null) settingsUI.SetActive(false);
    }

    void Update()
    {
        // ESC 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume(); // 이미 멈춰있으면 게임 재개
            }
            else
            {
                Pause(); // 게임 중이면 멈춤
            }
        }
    }

    // 게임 재개 (Resume)
    public void Resume()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsUI != null) settingsUI.SetActive(false);
        
        Time.timeScale = 1f; // 시간 다시 흐르게
        isPaused = false;
    }

    // 게임 일시 정지 (Pause)
    void Pause()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        // 설정창은 끄고 메인 버튼들만 보이게 초기화
        if (settingsUI != null) settingsUI.SetActive(false);

        Time.timeScale = 0f; // 시간 정지
        isPaused = true;
    }

    // [설정] 버튼 눌렀을 때
    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false); // 메인 버튼 숨김
        settingsUI.SetActive(true);   // 설정창 보임
    }

    // [설정]에서 [뒤로가기] 눌렀을 때
    public void CloseSettings()
    {
        settingsUI.SetActive(false); // 설정창 숨김
        pauseMenuUI.SetActive(true); // 메인 버튼 다시 보임
    }

    // [메인 메뉴로] 버튼 눌렀을 때
    public void LoadMenu()
    {
        Time.timeScale = 1f; // 씬 이동 전에 시간 정상화 (필수!)
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // [게임 종료] 버튼 눌렀을 때
    public void QuitGame()
    {
        Debug.Log("게임 종료...");
        Application.Quit();
    }
}