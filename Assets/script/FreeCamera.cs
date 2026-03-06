using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 10f;      // 기본 이동 속도
    public float boostMultiplier = 2f; // Shift 누를 때 빨라지는 배율
    public float sensitivity = 2f;     // 마우스 회전 감도

    private float yaw = 0f;   // 가로 회전 값
    private float pitch = 0f; // 세로 회전 값

    void Update()
    {
        // 1. 마우스 회전 (우클릭 상태일 때만)
        if (Input.GetMouseButton(1)) 
        {
            // 마우스 커서 잠그기 (선택 사항)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            yaw += Input.GetAxis("Mouse X") * sensitivity;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity;

            // 고개를 너무 뒤로 젖히지 않게 제한 (-90도 ~ 90도)
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }
        else
        {
            // 우클릭을 떼면 커서 다시 보이기
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 2. 이동 속도 계산 (Shift 누르면 부스트)
        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= boostMultiplier;
        }

        if (Input.GetMouseButton(1)) 
        {
            // 3. 키보드 입력 (WASD + QE)
            Vector3 moveDirection = Vector3.zero;

            // 앞/뒤 (W, S) - 카메라가 바라보는 방향 기준
            if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
            if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;

            // 좌/우 (A, D) - 카메라의 오른쪽 기준
            if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;
            if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;

            // 위/아래 (Q, E) - 월드 기준 수직 이동
            if (Input.GetKey(KeyCode.E)) moveDirection += Vector3.up;   // 상승
            if (Input.GetKey(KeyCode.Q)) moveDirection -= Vector3.up;   // 하강

            // 4. 실제 이동 적용
            transform.position += moveDirection * currentSpeed * Time.deltaTime;
        }
    }
}