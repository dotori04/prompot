using UnityEngine;

public class DictionaryUI : MonoBehaviour
{
    [System.Serializable]
    public class GimmickEntry
    {
        public string gimmickID;   // 검사할 ID (예: "Teleport")
        public GameObject entryUI; // 이 기믹을 설명하는 전체 UI 그룹 (버튼이나 패널)
    }

    [Header("UI 연결")]
    public GameObject dictionaryPanel; // 도감 전체 창
    public GimmickEntry[] entries;     // 도감 목록들

    // [?] 버튼을 눌렀을 때
    void Start()
    {
        if (dictionaryPanel != null) dictionaryPanel.SetActive(false); // 시작할 때는 도감 숨김
    }
    public void OpenDictionary()
    {
        RefreshDictionary(); // 열 때마다 최신 해금 상태로 업데이트
        dictionaryPanel.SetActive(true);
        Time.timeScale = 0f; // 도감을 읽는 동안 게임 일시정지
    }

    // 도감 닫기 버튼을 눌렀을 때
    public void CloseDictionary()
    {
        dictionaryPanel.SetActive(false);
        Time.timeScale = 1f; // 게임 재개
    }

    // [핵심] 해금된 항목만 켜고, 안 해금된 항목은 아예 꺼버립니다(숨김).
    private void RefreshDictionary()
    {
        foreach (var entry in entries)
        {
            if (entry.entryUI != null)
            {
                // IsUnlocked가 true면 켜지고, false면 꺼집니다.
                bool isUnlocked = GimmickManager.IsUnlocked(entry.gimmickID);
                entry.entryUI.SetActive(isUnlocked);
            }
        }
    }
}