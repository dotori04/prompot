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
    private Animator anim;
    private Queue<CommandData> actionQueue = new Queue<CommandData>();
    private bool isExecutingSequence = false;
    private System.Action onSequenceComplete;
    private Vector3 pendingTeleportOffset = Vector3.zero;

    IEnumerator Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        UnityEngine.Debug.Log("⏳ [CharacterMovement] 게임 시작. 1프레임 대기 중...");
        yield return null;

        UnityEngine.Debug.Log("▶ [CharacterMovement] 대기 완료. Respawn 함수 호출!");
        Respawn();
    }

    public void Respawn()
    {
        UnityEngine.Debug.Log("🔄 [CharacterMovement] Respawn() 시작됨.");

        // 1. 부모 해제 확인 (블록 위에 타고 있었을 경우 강제 하차)
        if (transform.parent != null)
        {
            UnityEngine.Debug.Log($"🔓 [CharacterMovement] 현재 부모({transform.parent.name})에서 분리합니다.");
            transform.SetParent(null);
        }
        else
        {
            UnityEngine.Debug.Log("🔓 [CharacterMovement] 부모 없음. 분리 불필요.");
        }

        // 2. Respawn 태그 찾기
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("Respawn");

        if (spawnPoint != null)
        {
            UnityEngine.Debug.Log($"📍 [CharacterMovement] Respawn 위치 찾음: {spawnPoint.name} / 위치: {spawnPoint.transform.position}");

            // 3-1. 무빙 블록(MovingBlock) 초기화 (꺼져있는 블록까지 모두 포함)
            MovingBlock[] movingBlocks = FindObjectsByType<MovingBlock>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            UnityEngine.Debug.Log($"📦 [CharacterMovement] 발견된 MovingBlock 개수: {movingBlocks.Length}개");
            foreach (var block in movingBlocks)
            {
                block.ResetBlock();
            }

            // 3-2. 밀 수 있는 박스(PushableBox) 초기화 (꺼져있는 박스까지 모두 포함)
            PushableBox[] pushableBoxes = FindObjectsByType<PushableBox>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            UnityEngine.Debug.Log($"📦 [CharacterMovement] 발견된 PushableBox 개수: {pushableBoxes.Length}개");
            foreach (var box in pushableBoxes)
            {
                box.ResetBox();
            }

            // 4. 플레이어 이동 로직 정지 (진행 중이던 명령 취소)
            StopAllMovement();

            // 5. 강제 이동 실행
            UnityEngine.Debug.Log($"🚀 [CharacterMovement] 플레이어 이동 실행! (현재: {transform.position} -> 목표: {spawnPoint.transform.position})");

            transform.position = spawnPoint.transform.position;
            transform.rotation = spawnPoint.transform.rotation;

            if (rb != null)
            {
                rb.position = spawnPoint.transform.position;
                rb.rotation = spawnPoint.transform.rotation;
                rb.linearVelocity = Vector3.zero;  // 가속도 초기화 (Unity 6 방식)
                rb.angularVelocity = Vector3.zero; // 회전력 초기화
                rb.Sleep();                        // 물리 엔진 잠시 재우기 (떨림 방지)
            }

            // 6. 좌표 깔끔하게 보정
            SnapToGrid();
            UnityEngine.Debug.Log($"✅ [CharacterMovement] 이동 완료. 현재 좌표: {transform.position}");
        }
        else
        {
            UnityEngine.Debug.LogError("🚨 [CharacterMovement] 심각한 오류: Scene에 'Respawn' 태그가 붙은 오브젝트가 없습니다!");
        }
    }

    // ... (이하 StartSequence 등 나머지 코드는 기존과 동일하므로 생략하지 않고 그대로 유지) ...

    public void StartSequence(List<CommandData> commands, System.Action onComplete)
    {
        StopAllMovement();
        actionQueue.Clear();
        pendingTeleportOffset = Vector3.zero;
        this.onSequenceComplete = onComplete;

        foreach (var cmd in commands) actionQueue.Enqueue(cmd);
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
                if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
                SnapToGrid();
                float waitTime = currentCmd.distance > 0 ? currentCmd.distance : 1f;
                yield return new WaitForSeconds(waitTime);
                break;
            case ActionType.Jump:
                if (rb != null) rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                yield return new WaitForSeconds(jumpWaitTime);
                break;
            case ActionType.Move:
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
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            SnapToGrid();
        }
        onSequenceComplete?.Invoke();
    }

    private IEnumerator TurnAndMoveRoutine(MoveDir dir, float dist)
    {
        SnapToGrid();
        if (anim != null) anim.SetBool("IsWalking", true);

        Quaternion startRot = rb.rotation;
        Quaternion targetRot = startRot;
        bool needTurn = false;

        if (dir == MoveDir.Left) { targetRot = startRot * Quaternion.Euler(0, -90, 0); needTurn = true; }
        else if (dir == MoveDir.Right) { targetRot = startRot * Quaternion.Euler(0, 90, 0); needTurn = true; }

        if (needTurn)
        {
            while (Quaternion.Angle(rb.rotation, targetRot) > 0.1f)
            {
                Quaternion nextRot = Quaternion.RotateTowards(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
                rb.MoveRotation(nextRot);
                yield return new WaitForFixedUpdate();
            }
            rb.MoveRotation(targetRot);
        }

        Vector3 moveDirVector = transform.forward;
        if (dir == MoveDir.Backward) moveDirVector = -transform.forward;

        int steps = (dist == -1f) ? 999 : Mathf.RoundToInt(dist);
        int currentStep = 0;

        while ((dist == -1f || currentStep < steps))
        {
            float gridSize = 1f;
            if (Physics.Raycast(rb.position + Vector3.up * 0.5f, moveDirVector, out RaycastHit hitObstacle, gridSize, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hitObstacle.collider.CompareTag("Block"))
                {
                    PushableBox box = hitObstacle.collider.GetComponent<PushableBox>();
                    if (box != null && !box.TryPush(moveDirVector, gridSize))
                    {
                        StopAllMovement();
                        yield break;
                    }
                }
                else if (hitObstacle.collider.CompareTag("Wall"))
                {
                    StopAllMovement();
                    yield break;
                }
            }

            Vector3 startPos = rb.position;
            Vector3 targetPos = startPos + (moveDirVector * gridSize);
            if (pendingTeleportOffset != Vector3.zero)
            {
                targetPos += pendingTeleportOffset;
                rb.position += pendingTeleportOffset;
                pendingTeleportOffset = Vector3.zero;
            }

            Vector3 startPosFlat = new Vector3(startPos.x, 0, startPos.z);
            Vector3 targetPosFlat = new Vector3(targetPos.x, 0, targetPos.z);
            float distanceToMove = Vector3.Distance(startPosFlat, targetPosFlat);
            float travelTime = distanceToMove / moveSpeed;
            float elapsedTime = 0f;

            while (elapsedTime < travelTime)
            {
                if (pendingTeleportOffset != Vector3.zero)
                {
                    targetPos += pendingTeleportOffset;
                    rb.position += pendingTeleportOffset;
                    pendingTeleportOffset = Vector3.zero;
                }
                elapsedTime += Time.fixedDeltaTime;
                float t = elapsedTime / travelTime;
                Vector3 newPos = Vector3.Lerp(startPos, targetPos, t);
                newPos.y = rb.position.y;
                rb.MovePosition(newPos);
                yield return new WaitForFixedUpdate();
            }
            SnapToGrid();
            currentStep++;
        }
        if (anim != null) anim.SetBool("IsWalking", false);
    }

    private void SnapToGrid()
    {
        Vector3 currentPos = rb.position;
        float snappedX = Mathf.Round(currentPos.x);
        float snappedZ = Mathf.Round(currentPos.z);
        rb.MovePosition(new Vector3(snappedX, currentPos.y, snappedZ));
    }

    public void StopAllMovement()
    {
        StopAllCoroutines();
        actionQueue.Clear();
        isExecutingSequence = false;
        pendingTeleportOffset = Vector3.zero;
        if (anim != null) anim.SetBool("IsWalking", false);
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void RequestTeleport(Vector3 newPos)
    {
        Vector3 offset = newPos - transform.position;
        pendingTeleportOffset += offset;
    }
}