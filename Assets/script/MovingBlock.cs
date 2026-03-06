using UnityEngine;
using System.Collections;

public class MovingBlock : MonoBehaviour
{
    [Header("경로 설정")]
    [Tooltip("이동할 경로를 나타내는 빈 오브젝트들을 순서대로 넣으세요.")]
    public Transform[] waypoints;

    [Header("이동 설정")]
    public float moveSpeed = 3.0f;
    public float waitTime = 1.0f;
    public bool isLoop = true;

    [Header("상호작용 설정")]
    public Material activeMaterial; // 밟았을 때 변할 재질

    private int currentPointIndex = 0;
    private bool isWaiting = false;
    
    // [핵심 추가] 플레이어가 위에 있는지 확인하는 변수
    private bool isPlayerOnBlock = false;

    // 원래 재질 저장용
    private Material originalMaterial;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }

        if (waypoints.Length > 0 && waypoints[0] != null)
        {
            transform.position = waypoints[0].position;
        }
    }

    void FixedUpdate()
    {
        // 웨이포인트가 없거나, 대기 중이거나, 
        // [중요] 플레이어가 위에 없으면 움직이지 않음!
        if (waypoints.Length == 0 || isWaiting || !isPlayerOnBlock) return;
        
        Move();
    }

    private void Move()
    {
        Transform targetPoint = waypoints[currentPointIndex];
        
        // 이동 처리
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.fixedDeltaTime);

        // 목표 지점 도착 확인
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

        // 다음 목표 설정
        currentPointIndex++;
        if (currentPointIndex >= waypoints.Length)
        {
            if (isLoop) currentPointIndex = 0;
            else
            {
                currentPointIndex = waypoints.Length - 1;
                // 반복이 아니면 끝에 도달했을 때 아예 기능을 끕니다.
                this.enabled = false; 
            }
        }
        isWaiting = false;
    }

    // ---------------------------------------------------------
    // 👣 충돌 감지 (탑승 여부 체크)
    // ---------------------------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOnBlock = true; // [중요] 이제부터 움직임 허용!

            collision.transform.SetParent(transform);
            if (meshRenderer != null && activeMaterial != null)
            {
                meshRenderer.material = activeMaterial;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOnBlock = false; // [중요] 움직임 정지!

            collision.transform.SetParent(null);
            if (meshRenderer != null && originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
            }
        }
    }

    // ---------------------------------------------------------
    // 🎨 기즈모 (순차 연결)
    // ---------------------------------------------------------
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.cyan;
        foreach (var point in waypoints)
        {
            if (point != null) Gizmos.DrawWireSphere(point.position, 0.3f);
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}