using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectUI : MonoBehaviour
{
    [Header("버튼 설정")]
    public Button[] levelButtons; // 인스펙터에서 레벨 1, 2, 3... 버튼을 순서대로 넣으세요.
    public Sprite lockedSprite;   // 잠긴 버튼에 보여줄 이미지 (선택)

    void Start()
    {
        int reachedLevel = LevelData.GetReachedLevel();

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNum = i + 1; // 배열은 0부터지만 레벨은 1부터 시작

            if (levelNum <= reachedLevel)
            {
                // [해금된 레벨]
                levelButtons[i].interactable = true;
                
                // 버튼 텍스트를 "1", "2" 등으로 변경
                TextMeshProUGUI btnText = levelButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = levelNum.ToString();

                // 클릭 시 해당 씬으로 이동 (람다식 활용)
                string sceneName = "Level_" + levelNum; // 씬 이름 규칙: Level_1, Level_2...
                levelButtons[i].onClick.AddListener(() => LoadLevel(sceneName));
            }
            else
            {
                // [잠긴 레벨]
                levelButtons[i].interactable = false;
                
                // 잠김 이미지 변경 (있다면)
                if (lockedSprite != null)
                {
                    levelButtons[i].image.sprite = lockedSprite;
                }
                
                TextMeshProUGUI btnText = levelButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = ""; // 텍스트 숨김
            }
        }
    }

    void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    // 개발용: 데이터 리셋 버튼 연결용
    public void ResetAllData()
    {
        LevelData.ResetData();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // UI 새로고침
    }
}