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

    [Header("옵션 설정")]
    [Tooltip("체크하면 이미 본 튜토리얼이라도 게임 시작 시 무조건 팝업을 띄웁니다.")]
    public bool alwaysShow = false; // [추가된 기능]

    void Start()
    {
        commandParser = Object.FindFirstObjectByType<LocalCommandParser>();

        // 1. 저장된 기록 확인 (0이면 처음 보는 것)
        bool isFirstTime = PlayerPrefs.GetInt("Tutorial_" + gimmickID, 0) == 0;

        // [수정된 로직]
        // 처음 보거나 OR 항상 보여주기 옵션이 켜져 있다면 -> 팝업 표시
        if (isFirstTime || alwaysShow)
        {
            ShowIntro();
        }
        else
        {
            // 이미 봤고, 항상 보여주기도 꺼져 있다면 -> 바로 게임 시작 상태로 전환
            if (introPanel != null) introPanel.SetActive(false);
            
            // 팝업이 안 뜨면 입력창이 바로 활성화되어야 함
            if (commandParser != null) commandParser.gameObject.SetActive(true);
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
        // (항상 보여주기가 켜져 있어도, 닫는 순간 '봤음' 처리는 해두는 게 안전함)
        PlayerPrefs.SetInt("Tutorial_" + gimmickID, 1);
        PlayerPrefs.Save();

        // 팝업 끄기
        if (introPanel != null) introPanel.SetActive(false);

        // 프롬프트 입력창 다시 켜서 게임 시작!
        if (commandParser != null) commandParser.gameObject.SetActive(true);
    }
}