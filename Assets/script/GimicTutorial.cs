    using UnityEngine;
    using TMPro;

    public class GimmickTutorial : MonoBehaviour
    {
        [Header("기믹 정보 설정")]
        [Tooltip("도감에서 쓸 고유 ID (예: Teleport)")]
        public string gimmickID;
        public string titleTextStr;
        [TextArea(3, 5)] public string descTextStr;

        [Header("UI 연결")]
        public GameObject tutorialPanel;
        public TextMeshProUGUI titleUI;
        public TextMeshProUGUI descUI;

        void Start()
        {
            // 도감에 등록되지 않은(처음 보는) 기믹이라면?
            if (!GimmickManager.IsUnlocked(gimmickID))
            {
                ShowTutorial();
                GimmickManager.UnlockGimmick(gimmickID); // 도감에 영구 해금
            }
            else
            {
                // 이미 아는 거면 팝업 숨기기
                if (tutorialPanel != null) tutorialPanel.SetActive(false);
            }
        }

        public void ShowTutorial()
        {
            if (tutorialPanel == null) return;
            
            titleUI.text = titleTextStr;
            descUI.text = descTextStr;
            tutorialPanel.SetActive(true);
            
            // 글을 읽는 동안 게임(시간)을 일시정지!
            Time.timeScale = 0f; 
        }

        // '닫기' 버튼에 연결할 함수
        public void CloseTutorial()
        {
            tutorialPanel.SetActive(false);
            Time.timeScale = 1f; // 게임 다시 재생
        }
    }