using UnityEngine;

public static class LevelData
{
    private const string REACHED_LEVEL_KEY = "ReachedLevel";

    // 현재까지 도달한 최고 레벨 가져오기 (기본값: 1)
    public static int GetReachedLevel()
    {
        return PlayerPrefs.GetInt(REACHED_LEVEL_KEY, 1);
    }

    // 다음 레벨 해금 (현재 깬 레벨이 최고 기록일 때만 저장)
    public static void UnlockLevel(int clearedLevel)
    {
        int currentReached = GetReachedLevel();
        
        // 방금 깬 레벨(1)이 현재 기록(1)과 같다면 -> 기록을 2로 갱신
        if (clearedLevel >= currentReached)
        {
            PlayerPrefs.SetInt(REACHED_LEVEL_KEY, clearedLevel + 1);
            PlayerPrefs.Save();
            Debug.Log($"🔓 진행 상황 저장됨: {clearedLevel + 1}레벨 해금!");
        }
    }

    // 데이터 초기화 (개발 테스트용)
    public static void ResetData()
    {
        PlayerPrefs.DeleteKey(REACHED_LEVEL_KEY);
    }
}