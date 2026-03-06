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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        gameObject.tag = "Block";
    }

    public bool TryPush(Vector3 pushDir, float gridSize)
    {
        if (isMoving) return false;

        targetPos = transform.position + pushDir * gridSize;
        targetPos = new Vector3(Mathf.Round(targetPos.x), transform.position.y, Mathf.Round(targetPos.z));

        // [수정됨] 박스도 이동할 때 트랩 등의 Trigger를 무시하도록 처리
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
        Vector3 startPos = rb.position; // transform 대신 rigidbody 위치 사용
        float elapsedTime = 0f;

        while (elapsedTime < pushDuration)
        {
            // Time.deltaTime 대신 물리 전용 시간인 fixedDeltaTime 사용
            elapsedTime += Time.fixedDeltaTime;

            // 위치를 그냥 덮어씌우는 게 아니라 '물리적으로 밀어냅니다' (Trigger 100% 감지)
            rb.MovePosition(Vector3.Lerp(startPos, target, elapsedTime / pushDuration));

            // Update가 아닌 물리 업데이트 주기에 맞춤
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(target); // 최종 위치 보정
        isMoving = false;
    }
}