using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    [Header("텔레포트 설정")]
    [Tooltip("이동할 목적지(Transform)를 연결하세요.")]
    public Transform destination;

    [Tooltip("순간이동 쿨타임 (무한 루프 방지)")]
    public float cooldown = 0.5f;
    
    private static bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTeleporting || destination == null) return;

        if (other.CompareTag("Player"))
        {
            CharacterMovement movement = other.GetComponent<CharacterMovement>();
            if (movement != null)
            {
                StartCoroutine(TeleportRoutine(movement));
            }
        }
    }

    private IEnumerator TeleportRoutine(CharacterMovement movement)
    {
        isTeleporting = true; 
        
        Debug.Log($"✨ 텔레포트 작동! [{gameObject.name}] -> [{destination.name}]");
        
        // ---------------------------------------------------------
        // 📐 [핵심 수학 로직] 벡터의 덧셈을 이용한 좌표 보정
        // ---------------------------------------------------------
        // 1. 출발지(현재 발판)에서 목적지(도착 발판)까지의 '방향과 거리'를 구합니다.
        Vector3 offsetVector = destination.position - transform.position;
        
        // 2. 캐릭터의 현재 위치에 그 거리만큼만 정확히 더해줍니다.
        // 이렇게 하면 발판의 높이나 두께 상관없이 캐릭터의 발바닥 Y축이 그대로 유지됩니다.
        Vector3 exactNewPosition = movement.transform.position + offsetVector;

        // 3. 계산된 완벽한 위치로 순간이동 요청
        movement.RequestTeleport(exactNewPosition);

        yield return new WaitForSeconds(cooldown);
        isTeleporting = false;
    }
}