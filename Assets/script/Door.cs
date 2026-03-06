using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("문 작동 설정")]
    [Tooltip("문이 열릴 때 이동할 거리 (예: Y로 3이면 위로 3칸 열림, Y로 -3이면 바닥으로 꺼짐)")]
    public Vector3 openOffset = new Vector3(0, 3f, 0); 
    public float openSpeed = 3f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Coroutine moveCoroutine;

    void Start()
    {
        // 게임 시작 시 문의 위치를 '닫힌 상태'로 기억
        closedPosition = transform.position;
        // 열릴 위치 계산
        openPosition = closedPosition + openOffset;
    }

    public void OpenDoor()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveDoorRoutine(openPosition));
    }

    public void CloseDoor()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveDoorRoutine(closedPosition));
    }

    private IEnumerator MoveDoorRoutine(Vector3 targetPos)
    {
        // 목표 위치에 도달할 때까지 부드럽게 이동
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, openSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
    }
}