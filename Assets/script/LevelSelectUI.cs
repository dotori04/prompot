using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 이건 없어도 되지만, 혹시 모르니 남겨둡니다.
using TMPro;

public class LevelSelectUI : MonoBehaviour
{
    [Header("버튼 설정")]
    public Button[] levelButtons; 
    public Sprite lockedSprite;   

    void Start()
    {
        int reachedLevel = LevelData.GetReachedLevel();

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNum = i + 1; 

            if (levelNum <= reachedLevel)
            {
                // [해금된 레벨]
                levelButtons[i].interactable = true;
                
                TextMeshProUGUI btnText = levelButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = levelNum.ToString();

                string sceneName = "Level_" + levelNum; 
                
                // 클릭 시 리스너 연결
                // 람다식 안에서 LoadLevel 함수를 호출합니다.
                levelButtons[i].onClick.AddListener(() => LoadLevel(sceneName));
            }
            else
            {
                // [잠긴 레벨]
                levelButtons[i].interactable = false;
                
                if (lockedSprite != null)
                {
                    levelButtons[i].image.sprite = lockedSprite;
                }
                
                TextMeshProUGUI btnText = levelButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = ""; 
            }
        }
    }

    // [핵심 수정 부분]
    void LoadLevel(string sceneName)
    {
        // 기존: SceneManager.LoadScene(sceneName);
        // 변경: 로딩 매니저에게 "로딩 화면 띄우면서 이동해줘!" 라고 부탁합니다.
        LoadingSceneManager.LoadScene(sceneName);
    }
    
    public void ResetAllData()
    {
        LevelData.ResetData();
        // 리셋 후 현재 씬(메인메뉴) 새로고침은 로딩바 없이 즉시 해도 괜찮습니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }
}