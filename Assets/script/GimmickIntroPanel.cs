using UnityEngine;
using TMPro;

public class GimmickIntroPanel : MonoBehaviour
{
    [Header("기믹 정보")]
    [Tooltip("이 맵에서 소개할 기믹 ID (예: Teleport) - 한 번만 띄우기 위해 사용")]
    public string gimmickID;
    public string gimmickName;
    [TextArea(3, 5)] public string gimmickDescription;

    [Header("UI 연결")]
    public GameObject introPanel;     // 전체 팝업 창
    public TextMeshProUGUI nameText;  // 왼쪽 위 기믹 이름
    public TextMeshProUGUI descText;  // 왼쪽 아래 설명
    
    // 플레이어가 팝업을 보는 동안 입력을 못 하도록 막을 용도
    private LocalCommandParser commandParser;

    void Start()
    {
        commandParser = Object.FindFirstObjectByType<LocalCommandParser>();

        // "Tutorial_기믹ID" 값이 0이면 처음 보는 것!
        if (PlayerPrefs.GetInt("Tutorial_" + gimmickID, 0) == 0)
        {
            ShowIntro();
        }
        else
        {
            // 이미 깼던 맵이거나 봤던 튜토리얼이면 바로 게임 시작
            if (introPanel != null) introPanel.SetActive(false);
        }
    }

    private void ShowIntro()
    {
        // 텍스트 세팅
        if (nameText != null) nameText.text = gimmickName;
        if (descText != null) descText.text = gimmickDescription;

        // 팝업 켜기
        if (introPanel != null) introPanel.SetActive(true);

        // 프롬프트 입력창 숨기기 (비디오가 재생되어야 하므로 Time.timeScale=0 대신 입력을 막음)
        if (commandParser != null) commandParser.gameObject.SetActive(false);
    }

    // [시작하기] 버튼에 연결할 함수
    public void CloseIntroAndStart()
    {
        // 도감(컴퓨터)에 "이 튜토리얼 봤음" 이라고 저장
        PlayerPrefs.SetInt("Tutorial_" + gimmickID, 1);
        PlayerPrefs.Save();

        // 팝업 끄기
        if (introPanel != null) introPanel.SetActive(false);

        // 프롬프트 입력창 다시 켜서 게임 시작!
        if (commandParser != null) commandParser.gameObject.SetActive(true);
    }
}