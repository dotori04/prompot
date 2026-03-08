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
    
    private Vector3 pendingTeleportOffset = Vector3.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // 물리 설정 강제 적용
        rb.useGravity = true; // 중력 사용! (핵심)
        rb.isKinematic = false; 
        rb.interpolation = RigidbodyInterpolation.Interpolate; 
        rb.constraints = RigidbodyConstraints.FreezeRotation; // 회전은 코드로만
    }

    public void StartSequence(List<CommandData> commands, System.Action onComplete)
    {
        StopAllMovement();
        actionQueue.Clear();
        pendingTeleportOffset = Vector3.zero;
        this.onSequenceComplete = onComplete;

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
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                // 대기 중 찌꺼기 속도 제거 및 좌표 보정
                SnapToGrid(); 
                float waitTime = currentCmd.distance > 0 ? currentCmd.distance : 1f;
                yield return new WaitForSeconds(waitTime);
                break;

            case ActionType.Jump:
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
            SnapToGrid(); // 시퀀스 끝날 때도 깔끔하게 보정
        }
        onSequenceComplete?.Invoke();
    }

    private IEnumerator TurnAndMoveRoutine(MoveDir dir, float dist)
    {
        // [핵심 1] 회전하기 전에 좌표를 정수로 딱 맞춤 (누적 오차 제거)
        SnapToGrid();

        // 1. 회전 로직
        Quaternion startRot = rb.rotation;
        Quaternion targetRot = startRot;
        bool needTurn = false;

        if (dir == MoveDir.Left) { targetRot = startRot * Quaternion.Euler(0, -90, 0); needTurn = true; }
        else if (dir == MoveDir.Right) { targetRot = startRot * Quaternion.Euler(0, 90, 0); needTurn = true; }

        if (needTurn)
        {
            // 회전 중
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

        // 2. 이동 로직
        int steps = (dist == -1f) ? 999 : Mathf.RoundToInt(dist);
        int currentStep = 0;

        while ((dist == -1f || currentStep < steps))
        {
            float gridSize = 1f;
            
            // A. 정면 장애물 감지
            if (Physics.Raycast(rb.position + Vector3.up * 0.5f, moveDirVector, out RaycastHit hitObstacle, gridSize, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hitObstacle.collider.CompareTag("Block"))
                {
                    PushableBox box = hitObstacle.collider.GetComponent<PushableBox>();
                    if (box != null)
                    {
                        if (!box.TryPush(moveDirVector, gridSize))
                        {
                            StopAllMovement();
                            yield break;
                        }
                    }
                }
                else if (hitObstacle.collider.CompareTag("Wall"))
                {
                    StopAllMovement();
                    yield break;
                }
            }

            // B. 목표 위치 계산 (높이는 건드리지 않음 -> 중력에 맡김)
            Vector3 startPos = rb.position;
            // X, Z만 계산하고 Y는 현재 위치 유지 (하지만 중력이 아래로 당김)
            Vector3 targetPos = startPos + (moveDirVector * gridSize);

            // C. 1칸 이동
            if (pendingTeleportOffset != Vector3.zero)
            {
                targetPos += pendingTeleportOffset;
                rb.position += pendingTeleportOffset;
                pendingTeleportOffset = Vector3.zero;
            }

            // 수평 거리만 계산 (Y축 오차 무시)
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
                
                // 현재 진행률에 따른 X, Z 위치 계산
                float t = elapsedTime / travelTime;
                Vector3 newPos = Vector3.Lerp(startPos, targetPos, t);
                
                // [핵심 2] Y축은 물리 엔진이 알아서 하도록 놔둠 (현재 Y 유지 or 중력 반영)
                // MovePosition은 물리 연산을 포함하므로, 여기서 Y를 강제로 startPos.y로 고정하면 공중부양함.
                // 따라서 목표지점의 Y를 '현재 Rigidbody의 Y'로 계속 갱신해주는 것이 자연스러움.
                newPos.y = rb.position.y; 

                rb.MovePosition(newPos);
                yield return new WaitForFixedUpdate();
            }

            // 미세 오차 방지를 위해 이동 후 X,Z 좌표 정수화 (Snap)
            SnapToGrid();
            currentStep++;
        }
    }

    // [신규 기능] 좌표를 정수로 딱 맞춰주는 함수
    private void SnapToGrid()
    {
        Vector3 currentPos = rb.position;
        // X, Z는 반올림하여 정수로, Y는 그대로 둠 (바닥 높이 유지)
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