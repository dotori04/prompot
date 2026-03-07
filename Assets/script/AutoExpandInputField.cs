using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class AutoExpandInputField : MonoBehaviour
{
    [Header("크기 설정")]
    [Tooltip("입력창의 기본(최소) 높이")]
    public float minHeight = 50f;    
    
    [Tooltip("입력창의 최대 높이 (이 이상은 창이 안 커지고 스크롤이 생깁니다)")]
    public float maxHeight = 200f;   
    
    [Tooltip("텍스트 위아래 여백 (글자가 박스에 딱 붙지 않게 조절)")]
    public float padding = 20f;      

    private TMP_InputField inputField;
    private RectTransform rectTransform;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        rectTransform = GetComponent<RectTransform>();

        // 속성을 여러 줄 입력 가능하도록 설정
        inputField.lineType = TMP_InputField.LineType.MultiLineNewline;

        // 글자가 입력되거나 지워질 때마다 UpdateHeight 함수를 실행하도록 연결
        inputField.onValueChanged.AddListener(UpdateHeight);

        // 처음 시작할 때 크기 한 번 맞추기
        UpdateHeight(inputField.text);
    }

    private void UpdateHeight(string text)
    {
        // 현재 적힌 텍스트가 실제로 차지하는 높이를 계산
        float textHeight = inputField.textComponent.preferredHeight;

        // 여백(padding)을 더한 최종 높이를 계산하되, 최소/최대 높이 안에서만 변하도록 제한(Clamp)
        float targetHeight = Mathf.Clamp(textHeight + padding, minHeight, maxHeight);

        // 입력창의 세로 길이(Height) 변경!
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, targetHeight);
    }
}