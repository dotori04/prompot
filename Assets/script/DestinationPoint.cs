using UnityEngine;

public class DestinationPoint : MonoBehaviour
{
    // 충돌체(Collider)의 Is Trigger가 체크되어 있어야 합니다!
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("📍 [Trigger] 플레이어 도착 감지!");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetLevelClear();
            }
        }
    }
}