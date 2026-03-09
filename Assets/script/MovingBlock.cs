using UnityEngine;
using System.Collections;

public class MovingBlock : MonoBehaviour
{
    [Header("경로 설정")]
    public Transform[] waypoints;

    [Header("이동 설정")]
    public float moveSpeed = 3.0f;
    public float waitTime = 1.0f;
    public bool isLoop = true;

    [Header("상호작용 설정")]
    public Material activeMaterial;

    private int currentPointIndex = 0;
    private bool isWaiting = false;
    private bool isPlayerOnBlock = false;

    private Material originalMaterial;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) originalMaterial = meshRenderer.material;
    }

    void Start()
    {
        if (waypoints.Length > 0 && waypoints[0] != null)
        {
            transform.position = waypoints[0].position;
        }
    }

    public void ResetBlock()
    {
        UnityEngine.Debug.Log($"🛠️ [MovingBlock] {gameObject.name} 초기화 시작.");

        // 1. 동작 완전 정지
        StopAllCoroutines();

        // 2. 상태 초기화
        currentPointIndex = 0;
        isWaiting = false;
        isPlayerOnBlock = false;

        // 3. 재질 초기화
        if (meshRenderer != null && originalMaterial != null)
        {
            meshRenderer.material = originalMaterial;
        }

        // 4. 위치 강제 이동
        if (waypoints.Length > 0 && waypoints[0] != null)
        {
            UnityEngine.Debug.Log($"🛠️ [MovingBlock] {gameObject.name} 위치 이동 -> {waypoints[0].position}");
            transform.position = waypoints[0].position;
        }
        else
        {
            UnityEngine.Debug.LogError($"🚨 [MovingBlock] {gameObject.name}의 Waypoint[0]이 비어있습니다!");
        }
        
        // 5. 꺼져 있었더라도 다시 켜주기
        this.enabled = true;
    }

    void FixedUpdate()
    {
        if (waypoints.Length == 0 || isWaiting || !isPlayerOnBlock) return;
        Move();
    }

    private void Move()
    {
        if (currentPointIndex >= waypoints.Length) currentPointIndex = 0;
        Transform targetPoint = waypoints[currentPointIndex];
        
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.01f)
        {
            transform.position = targetPoint.position;
            StartCoroutine(WaitAndNext());
        }
    }

    private IEnumerator WaitAndNext()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        currentPointIndex++;
        if (currentPointIndex >= waypoints.Length)
        {
            if (isLoop) currentPointIndex = 0;
            else
            {
                currentPointIndex = waypoints.Length - 1;
                this.enabled = false; 
            }
        }
        isWaiting = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            UnityEngine.Debug.Log($"🦶 [MovingBlock] 플레이어가 {gameObject.name} 위에 올라탐.");
            isPlayerOnBlock = true;
            collision.transform.SetParent(transform); 
            if (meshRenderer != null && activeMaterial != null) meshRenderer.material = activeMaterial;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            UnityEngine.Debug.Log($"🦶 [MovingBlock] 플레이어가 {gameObject.name}에서 벗어남.");
            isPlayerOnBlock = false;
            
            if (collision.gameObject.activeInHierarchy) 
            {
                collision.transform.SetParent(null); 
            }
            if (meshRenderer != null && originalMaterial != null) meshRenderer.material = originalMaterial;
        }
    }

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = Color.cyan;
        foreach (var point in waypoints) if (point != null) Gizmos.DrawWireSphere(point.position, 0.3f);
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length - 1; i++)
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
    }
}