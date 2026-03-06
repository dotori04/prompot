using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    // 시작 버튼을 눌렀을 때 실행될 함수
    public void OnStartButtonClicked()
    {
        // LevelData에서 현재까지 깬 진행도를 가져옵니다. (기본값은 1)
        int reachedLevel = LevelData.GetReachedLevel();

        // 도달한 레벨이 1이라면 (즉, 게임을 처음 설치했거나 1탄을 아직 못 깬 상태)
        if (reachedLevel == 1)
        {
            Debug.Log("새로운 플레이어 감지! Level_1로 바로 이동합니다.");
            SceneManager.LoadScene("Level_1");
        }
        // 이미 1탄을 깨서 2탄 이상 열려있는 상태라면 (기존 유저)
        else
        {
            Debug.Log("기존 플레이어 감지! 레벨 선택 화면으로 이동합니다.");
            SceneManager.LoadScene("Main_Menu"); 
        }
    }
}