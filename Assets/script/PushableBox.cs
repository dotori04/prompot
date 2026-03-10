using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PushableBox : MonoBehaviour
{
    [Header("밀기 설정")]
    public float pushDuration = 0.5f;

    private Rigidbody rb;
    private bool isMoving = false;
    private Vector3 targetPos;

    // [추가됨] 시작 위치 저장용 변수
    private Vector3 initialPos;

    // [추가됨] Awake: 다른 스크립트가 부르기 전에 가장 먼저 자기 위치를 기억해둡니다.
    void Awake()
    {
        initialPos = transform.position;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        gameObject.tag = "Block";
    }

    // [핵심 추가] 리스폰 시 CharacterMovement가 호출할 초기화 함수
    public void ResetBox()
    {
        // 1. 밀리고 있던 중이라면 강제 정지
        StopAllCoroutines(); 
        
        // 2. 상태 초기화
        isMoving = false;

        // 3. 원래 위치로 복귀
        transform.position = initialPos;
        
        if (rb != null)
        {
            rb.position = initialPos;
            rb.linearVelocity = Vector3.zero; // 혹시 모를 물리력(관성) 제거
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"📦 [PushableBox] {gameObject.name} 원위치로 초기화 완료");
    }

    public bool TryPush(Vector3 pushDir, float gridSize)
    {
        if (isMoving) return false;

        targetPos = transform.position + pushDir * gridSize;
        targetPos = new Vector3(Mathf.Round(targetPos.x), transform.position.y, Mathf.Round(targetPos.z));

        // 박스도 이동할 때 트랩 등의 Trigger를 무시하도록 처리
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, pushDir, out hit, gridSize, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Block"))
            {
                Debug.Log("📦 [박스] 앞이 막혀서 밀 수 없습니다.");
                return false;
            }
        }

        StartCoroutine(PushRoutine(targetPos));
        return true;
    }

    private IEnumerator PushRoutine(Vector3 target)
    {
        isMoving = true;
        Vector3 startPos = rb.position; 
        float elapsedTime = 0f;

        while (elapsedTime < pushDuration)
        {
            elapsedTime += Time.fixedDeltaTime;
            rb.MovePosition(Vector3.Lerp(startPos, target, elapsedTime / pushDuration));
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(target); 
        isMoving = false;
    }
}