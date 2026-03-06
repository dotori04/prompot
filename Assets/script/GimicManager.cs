using UnityEngine;

public static class GimmickManager
{
    // 기믹 ID 예시: "Teleport", "Trap", "Box", "Switch"
    
    // 해당 기믹이 도감에 해금되었는지 확인
    public static bool IsUnlocked(string gimmickID)
    {
        return PlayerPrefs.GetInt("Gimmick_" + gimmickID, 0) == 1;
    }

    // 새로운 기믹을 발견했을 때 도감에 등록
    public static void UnlockGimmick(string gimmickID)
    {
        PlayerPrefs.SetInt("Gimmick_" + gimmickID, 1);
        PlayerPrefs.Save();
        Debug.Log($"📖 도감 업데이트: [{gimmickID}] 기믹이 해금되었습니다!");
    }
}