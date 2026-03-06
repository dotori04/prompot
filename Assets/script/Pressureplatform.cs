using UnityEngine;

public class PressurePlatform : MonoBehaviour
{
    [Header("연결할 장치")]
    [Tooltip("이 발판을 밟았을 때 열릴 문을 끌어다 넣으세요.")]
    public Door targetDoor;

    [Header("시각 효과")]
    public Material pressedMaterial;  // 눌렸을 때 색상 (예: 초록색)
    public Material defaultMaterial;  // 평소 색상 (예: 빨간색)

    private MeshRenderer rend;
    private int objectsOnPlatform = 0; // 발판을 누르고 있는 물체의 수

    void Start()
    {
        rend = GetComponent<MeshRenderer>();
        if (rend != null && defaultMaterial != null) rend.material = defaultMaterial;
    }

    private void OnTriggerEnter(Collider other)
    {
        objectsOnPlatform++;
        UnityEngine.Debug.Log($"👣 {other.name}이(가) 발판에 올라갔습니다.{objectsOnPlatform}");

        // 처음 하나가 올라갔을 때 문 열기
        if (objectsOnPlatform <= 1)
        {
            if (rend != null && pressedMaterial != null) rend.material = pressedMaterial;
            if (targetDoor != null) targetDoor.OpenDoor();
            Debug.Log("🔘 발판 ON! 문이 열립니다.");
        }
        // 박스("Block")나 플레이어("Player")가 발판에 닿았을 때
        // if (other.CompareTag("Block") || other.CompareTag("Player"))
        // {   
        //     objectsOnPlatform++;
        //     UnityEngine.Debug.Log($"👣 {other.name}이(가) 발판에 올라갔습니다.{objectsOnPlatform}");

        //     // 처음 하나가 올라갔을 때 문 열기
        //     if (objectsOnPlatform <= 1)
        //     {
        //         if (rend != null && pressedMaterial != null) rend.material = pressedMaterial;
        //         if (targetDoor != null) targetDoor.OpenDoor();
        //         Debug.Log("🔘 발판 ON! 문이 열립니다.");
        //     }
        // }
    }

    private void OnTriggerExit(Collider other)
    {
        // 밟고 있던 물체가 내려갔을 때
        if (other.CompareTag("Block") || other.CompareTag("Player"))
        {
            objectsOnPlatform--;

            // 올라갔던 물체가 모두 내려왔을 때 다시 문 닫기
            if (objectsOnPlatform <= 0)
            {
                objectsOnPlatform = 0;
                if (rend != null && defaultMaterial != null) rend.material = defaultMaterial;
                if (targetDoor != null) targetDoor.CloseDoor();
                Debug.Log("🔘 발판 OFF! 문이 닫힙니다.");
            }
        }
    }
}