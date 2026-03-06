using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("🎯 할당 설정")]
    public GameObject playerCharacter;
    public Transform spawnPoint;
    
    [Header("UI 설정")]
    public GameObject clearUIPanel;
    public TextMeshProUGUI objectiveTextUI; 

    [Header("게임 규칙")]
    public int currentLevelIndex = 1;
    public float fallThreshold = -10.0f;
    
    [TextArea(2, 3)]
    public string levelObjective = "도착 지점으로 이동하세요.";

    [HideInInspector]
    public bool isLevelClear = false;

    void Awake()
    {
        Instance = this;

        // [핵심 수정] 씬이 켜지자마자 가장 먼저 클리어 UI를 강제로 꺼버립니다!
        // 에디터에서 실수로 켜두고 저장하더라도 시작할 때 무조건 비활성화됩니다.
        if (clearUIPanel != null)
        {
            clearUIPanel.SetActive(false);
        }
    }

    void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        ResetStatusOnly(); 
        RespawnPlayer();   

        // 1. 캐릭터가 진행 중이던 명령 강제 중지
        if (playerCharacter != null)
        {
            CharacterMovement movement = playerCharacter.GetComponent<CharacterMovement>();
            if (movement != null) movement.StopAllMovement();
        }

        // 2. 잠긴 UI 프롬프트 강제로 다시 열기
        LocalCommandParser parser = Object.FindFirstObjectByType<LocalCommandParser>();
        if (parser != null)
        {
            parser.ResetUI();
        }
    }

    public void ResetStatusOnly()
    {
        isLevelClear = false;
        Time.timeScale = 1.0f;
        
        // Start 이후의 초기화(리스폰 등)를 위해서 여기에도 남겨둡니다.
        if (clearUIPanel != null) clearUIPanel.SetActive(false);
        
        if (objectiveTextUI != null)
        {
            objectiveTextUI.text = $"목표: {levelObjective}";
        }
    }

    void Update()
    {
        // 씬에 있는 진짜 "Player"를 강제로 찾습니다.
        if (playerCharacter == null || !playerCharacter.scene.IsValid())
        {
            playerCharacter = GameObject.FindGameObjectWithTag("Player");
        }

        // 실시간 추락 감지 (Y좌표 확인)
        if (playerCharacter != null && !isLevelClear)
        {
            if (playerCharacter.transform.position.y < fallThreshold)
            {
                Debug.Log($"📉 추락 감지! (현재 Y: {playerCharacter.transform.position.y}) 대기 취소 및 즉시 리스폰!");
                InitializeGame(); 
            }
        }
    }

    public void CheckResult()
    {
        if (isLevelClear)
        {
            Debug.Log("🎉 성공! (위치 유지)");
        }
        else
        {
            Debug.Log("❌ 실패! (처음으로 복귀)");
            InitializeGame(); 
        }
    }

    public void SetLevelClear()
    {
        if (isLevelClear) return;
        isLevelClear = true;
        
        if (clearUIPanel != null) clearUIPanel.SetActive(true);
        LevelData.UnlockLevel(currentLevelIndex);
        Debug.Log("🏆 스테이지 클리어!");
    }

    private void RespawnPlayer()
    {
        if (playerCharacter == null) return;

        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.TeleportCharacter(playerCharacter, spawnPoint);
        }
        else
        {
            // 혹시 RespawnManager가 없어도 강제로 옮기는 안전장치
            playerCharacter.transform.position = spawnPoint.position;
        }
    }

    public void LoadNextLevel()
    {
        string nextSceneName = "Level_" + (currentLevelIndex + 1);
        if (Application.CanStreamedLevelBeLoaded(nextSceneName)) SceneManager.LoadScene(nextSceneName);
        else SceneManager.LoadScene("Main_Menu");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Main_Menu");
    }
}