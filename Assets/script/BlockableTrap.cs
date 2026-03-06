using UnityEngine;
using System.Collections;

public class BlockableTrap : MonoBehaviour
{
    [Header("타이밍 설정")]
    public float onDuration = 3f;   
    public float offDuration = 2f;  
    public float startDelay = 0f;

    [Header("시각 효과")]
    public Material activeMaterial;   // 빨간색 (위험)
    public Material inactiveMaterial; // 초록색/투명 (꺼짐, 안전)
    public Material blockedMaterial;  // 회색/파란색 (박스로 막힘, 안전)
    
    private MeshRenderer rend;
    private bool isBlocked = false;       // 박스로 막혔는지 여부
    private bool isPlayerInside = false;  // 플레이어가 위에 있는지 여부
    private bool isTimerActive = true;    // 타이머에 의해 켜져야 하는 상태인지

    void Start()
    {
        rend = GetComponent<MeshRenderer>();
        StartCoroutine(TrapRoutine());
    }

    private IEnumerator TrapRoutine()
    {
        // 엇박자 시작
        if (startDelay > 0)
        {
            isTimerActive = false;
            UpdateTrapState();
            yield return new WaitForSeconds(startDelay);
        }

        while (true)
        {
            // 트랩 ON 타이밍
            isTimerActive = true;
            UpdateTrapState();
            yield return new WaitForSeconds(onDuration);

            // 트랩 OFF 타이밍
            isTimerActive = false;
            UpdateTrapState();
            yield return new WaitForSeconds(offDuration);
        }
    }

    private void UpdateTrapState()
    {
        if (rend != null)
        {
            // 1순위: 박스로 막혀있다면 무조건 봉인(안전) 상태의 색상!
            if (isBlocked) 
            {
                rend.material = blockedMaterial;
            }
            // 2순위: 안 막혀있다면 타이머에 따라 켜짐/꺼짐 색상 변경
            else 
            {
                rend.material = isTimerActive ? activeMaterial : inactiveMaterial;
            }
        }

        // 박스로 막히지 않았고, 트랩이 켜져 있는데, 플레이어가 그 안에 있다면 -> 발각!
        if (!isBlocked && isTimerActive && isPlayerInside)
        {
            CatchPlayer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Block"))
        {
            Debug.Log("🚨 [트랩] 박스에 의해 봉인되었습니다! (안전함)");
            isBlocked = true;
            UpdateTrapState();
        }
        else if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            
            if (!isBlocked && isTimerActive)
            {
                CatchPlayer();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Block"))
        {
            Debug.Log("🚨 [트랩] 박스가 빠져나갔습니다! 다시 타이머대로 작동합니다.");
            isBlocked = false;
            UpdateTrapState();
        }
        else if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    private void CatchPlayer()
    {
        Debug.Log("💀 [트랩] 레이저에 발각되었습니다! 리스폰.");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitializeGame();
        }
    }
}