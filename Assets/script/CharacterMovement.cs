using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3.0f;
    public float turnSpeed = 360.0f;
    public float jumpForce = 5.0f;
    public float jumpWaitTime = 0.8f;

    private Rigidbody rb;
    private Queue<CommandData> actionQueue = new Queue<CommandData>();
    private bool isExecutingSequence = false;
    private System.Action onSequenceComplete;
    private Vector3 teleportOffset = Vector3.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; 
    }

    public void StartSequence(List<CommandData> commands, System.Action onComplete)
    {
        StopAllMovement();
        actionQueue.Clear();
        teleportOffset = Vector3.zero;
        this.onSequenceComplete = onComplete;

        if (rb != null) rb.isKinematic = true;

        foreach (var cmd in commands)
        {
            actionQueue.Enqueue(cmd);
        }

        if (!isExecutingSequence) StartCoroutine(ExecuteNextCommand());
    }

    private IEnumerator ExecuteNextCommand()
    {
        if (actionQueue.Count == 0)
        {
            FinishSequence();
            yield break;
        }

        isExecutingSequence = true;
        CommandData currentCmd = actionQueue.Dequeue();

        switch (currentCmd.action)
        {
            case ActionType.Stop:
                actionQueue.Clear(); 
                break;
            case ActionType.Wait:
                if (rb != null) 
                {
                    rb.linearVelocity = Vector3.zero;  
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = false;            
                }
                float waitTime = currentCmd.distance > 0 ? currentCmd.distance : 1f;
                yield return new WaitForSeconds(waitTime);
                break;
            case ActionType.Jump:
                if (rb != null) rb.isKinematic = false; 
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                yield return new WaitForSeconds(jumpWaitTime);
                if (rb != null && actionQueue.Count > 0 && actionQueue.Peek().action == ActionType.Move)
                {
                     rb.isKinematic = true;
                     rb.linearVelocity = Vector3.zero;
                }
                break;
            case ActionType.Move:
                if (rb != null) rb.isKinematic = true;
                yield return StartCoroutine(TurnAndMoveRoutine(currentCmd.dir, currentCmd.distance));
                break;
        }

        if (actionQueue.Count > 0) yield return StartCoroutine(ExecuteNextCommand());
        else FinishSequence();
    }

    private void FinishSequence()
    {
        isExecutingSequence = false;
        if (rb != null) 
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
        }
        onSequenceComplete?.Invoke();
    }

    private IEnumerator TurnAndMoveRoutine(MoveDir dir, float dist)
    {
        // 1. 회전 로직 (기존 유지)
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = startRot;
        bool needTurn = false;

        if (dir == MoveDir.Left) { targetRot = startRot * Quaternion.Euler(0, -90, 0); needTurn = true; }
        else if (dir == MoveDir.Right) { targetRot = startRot * Quaternion.Euler(0, 90, 0); needTurn = true; }

        if (needTurn)
        {
            while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
                yield return null;
            }
            transform.rotation = targetRot;
        }

        Vector3 moveDirVector = transform.forward;
        if (dir == MoveDir.Backward) moveDirVector = -transform.forward;

        // 2. 이동할 총 '칸' 수 계산
        int steps = 0;
        bool isInfinite = false;

        if (dist == -1f) isInfinite = true;
        else steps = Mathf.RoundToInt(dist);

        int currentStep = 0;

        // 3. [핵심 수정] 한 칸씩 쪼개서 레이저로 검사하고 이동합니다!
        while (isInfinite || currentStep < steps)
        {
            float gridSize = 1f; // 1칸의 거리
            RaycastHit hit;
            
            // 내 코앞 딱 1칸만 먼저 검사
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, moveDirVector, out hit, gridSize, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag("Block"))
                {
                    PushableBox box = hit.collider.GetComponent<PushableBox>();
                    if (box != null)
                    {
                        Debug.Log("📦 [플레이어] 코앞에 박스 발견! 밀기 시도.");
                        // 박스를 밀어보고, 벽에 막혀서 안 밀린다면 내 이동도 즉시 취소!
                        if (!box.TryPush(moveDirVector, gridSize))
                        {
                            Debug.LogWarning("⚠️ 박스가 벽에 막혔습니다. 더 이상 전진할 수 없습니다.");
                            StopAllMovement();
                            yield break; 
                        }
                    }
                }
                else if (hit.collider.CompareTag("Wall"))
                {
                    Debug.LogWarning("⚠️ 코앞에 벽이 있습니다. 전진을 멈춥니다.");
                    StopAllMovement();
                    yield break; 
                }
            }

            // 앞이 뚫려있거나 박스를 밀어냈다면, 내 몸을 딱 '1칸만' 전진!
            Vector3 targetPos = transform.position + moveDirVector * gridSize;
            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                if (teleportOffset != Vector3.zero)
                {
                    targetPos += teleportOffset; 
                    teleportOffset = Vector3.zero; 
                }
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos; // 1칸 이동 완료 후 위치 딱 맞게 보정
            
            currentStep++; // 걸음 수 1 증가 (이걸 목표 거리까지 반복)
        }

    }

    public void StopAllMovement()
    {
        StopAllCoroutines();
        actionQueue.Clear();
        isExecutingSequence = false;
        teleportOffset = Vector3.zero;
        if (rb != null) 
        {
            if (!rb.isKinematic) 
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = false; 
        }
    }

    public void RequestTeleport(Vector3 newPos)
    {
        Vector3 offset = newPos - transform.position;
        transform.position = newPos;
        teleportOffset += offset;
    }
}