using UnityEngine;
using System.Collections;

public class RespawnManager : MonoBehaviour
{
    // 어디서든 부를 수 있게 싱글톤으로 만듭니다.
    public static RespawnManager Instance;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 외부에서 호출하는 함수: "누구를(character) 어디로(spawnPoint) 보내라"
    /// </summary>
    public void TeleportCharacter(GameObject character, Transform spawnPoint)
    {
        if (character == null || spawnPoint == null)
        {
            Debug.LogError("🚨 [RespawnManager] 캐릭터나 스폰 포인트가 없습니다!");
            return;
        }

        // 안전한 이동을 위해 코루틴 시작
        StartCoroutine(TeleportRoutine(character, spawnPoint));
    }

    private IEnumerator TeleportRoutine(GameObject charObj, Transform spot)
    {
        Rigidbody rb = charObj.GetComponent<Rigidbody>();

        // 1. 물리 엔진 잠시 끄기 (저항 없이 이동하기 위해)
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;  // 속도 0
            rb.angularVelocity = Vector3.zero; // 회전 0
            rb.isKinematic = true;       // 물리 영향 받지 않음
        }

        // 2. 위치 및 회전 강제 지정
        charObj.transform.position = spot.position;
        charObj.transform.rotation = spot.rotation;

        Debug.Log($"🚚 [이동 완료] {charObj.name} -> {spot.name} 위치로 이동.");

        // 3. 한 프레임 대기 (유니티가 위치 변경을 확정할 시간)
        yield return null; 
        // 혹은 물리 업데이트까지 대기하려면 yield return new WaitForFixedUpdate();

        // 4. 물리 엔진 다시 켜기
        if (rb != null)
        {
            rb.isKinematic = false; 
        }
    }
}