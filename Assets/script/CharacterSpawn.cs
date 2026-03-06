using UnityEngine;
using System.Collections;

public class CharacterSpawn : MonoBehaviour
{
    [Header("스폰 설정")]
    public Transform spawnPoint; // 인스펙터에서 빈 오브젝트 연결 필수!

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // 게임 시작 시 1회 실행
        StartCoroutine(ForceRespawnRoutine());
    }

    // 외부에서 호출하는 함수
    public void Respawn()
    {
        StartCoroutine(ForceRespawnRoutine());
    }

    // 물리 엔진의 간섭을 무시하고 강제로 이동시키는 코루틴
    private IEnumerator ForceRespawnRoutine()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("🚨 [CharacterSpawn] SpawnPoint가 연결되지 않았습니다!");
            yield break;
        }

        // 1. 물리 연산 잠시 끄기 (가장 중요!)
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;  // 이동 속도 0
            rb.angularVelocity = Vector3.zero; // 회전 속도 0
            rb.isKinematic = true;       // 물리 엔진 영향 받지 않게 설정
        }
        UnityEngine.Debug.Log($"최초 위치: {transform.position}");

        // 2. 위치 및 회전 강제 지정
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        UnityEngine.Debug.Log($"현재 위치: {transform.position}");
        
        // 혹시 모를 Rigidbody 위치도 같이 이동
        if (rb != null)
        {
            rb.position = spawnPoint.position;
            rb.rotation = spawnPoint.rotation;
        }

        Debug.Log($"🔄 [Spawn] {spawnPoint.position} 좌표로 강제 이동 완료.");

        // 3. 한 프레임 대기 (유니티가 위치 변경을 인식할 시간 주기)
        yield return null; 

        // 4. 물리 연산 다시 켜기
        if (rb != null)
        {
            rb.isKinematic = false; 
        }
    }
}