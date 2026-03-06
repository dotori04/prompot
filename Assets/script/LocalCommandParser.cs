using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LocalCommandParser : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_InputField promptInput;
    public UnityEngine.UI.Button sendButton;

    [Header("플레이어 연결")]
    public CharacterMovement character;

    void Start()
    {
        if (sendButton != null) sendButton.onClick.AddListener(OnSendPrompt);
    }

    public void OnSendPrompt()
    {
        string userInput = promptInput.text.Trim();
        if (string.IsNullOrEmpty(userInput)) return;

        Debug.Log($"📝 [입력] {userInput}");
        promptInput.text = ""; 
        AnalyzeAndQueueCommands(userInput);
    }

    private void AnalyzeAndQueueCommands(string input)
    {
        List<CommandData> commandSequence = new List<CommandData>();
        
        string pattern = @"(고\s*|서\s*|며\s*|면서\s*|그리고\s*|다음에\s*|그다음\s*|다음\s*|후\s*|뒤에\s*|,|\.|\n|하고\s*|가고\s*|뛰고\s*|쉬고\s*)";
        string[] phrases = Regex.Split(input, pattern);

        foreach (string phrase in phrases)
        {
            if (string.IsNullOrWhiteSpace(phrase) || Regex.IsMatch(phrase, pattern)) continue;
            CommandData cmd = ParseSinglePhrase(phrase);
            if (cmd != null && cmd.action != ActionType.None) commandSequence.Add(cmd);
        }

        if (commandSequence.Count > 0)
        {
            Debug.Log($"📋 [계획] {commandSequence.Count}개 행동 준비 완료");
            ToggleUI(false); // UI 잠금

            if (GameManager.Instance != null) 
            {
                GameManager.Instance.ResetStatusOnly();
            }

            character.StartSequence(commandSequence, OnSequenceFinished);
        }
        else
        {
            Debug.LogWarning("⚠️ 실행할 명령이 없습니다.");
        }
    }

    private void OnSequenceFinished()
    {
        StartCoroutine(WaitAndCheckResult());
    }

    private IEnumerator WaitAndCheckResult()
    {
        yield return new WaitForSeconds(0.1f);
        ToggleUI(true);
        if (GameManager.Instance != null) GameManager.Instance.CheckResult();
    }

    private void ToggleUI(bool isActive)
    {
        if (promptInput != null) promptInput.interactable = isActive;
        if (sendButton != null) sendButton.interactable = isActive;
        if (isActive && promptInput != null) promptInput.ActivateInputField();
    }

    // [추가된 부분] 게임매니저가 강제로 UI 잠금을 풀 때 부르는 함수입니다.
    public void ResetUI()
    {
        StopAllCoroutines(); // 혹시 돌고 있던 완료 대기 타이머(코루틴) 강제 종료
        ToggleUI(true);      // 입력창 다시 활성화
    }

    private CommandData ParseSinglePhrase(string phrase)
    {
        CommandData cmdData = new CommandData();
        cmdData.action = ActionType.None;
        cmdData.dir = MoveDir.Forward;
        cmdData.distance = -1f;

        string text = phrase.Replace(" ", "");
        Match numberMatch = Regex.Match(text, @"\d+(\.\d+)?");
        if (numberMatch.Success) cmdData.distance = float.Parse(numberMatch.Value);
        else
        {
             if (Regex.IsMatch(text, @"(한|일)")) cmdData.distance = 1f;
             else if (Regex.IsMatch(text, @"(두|이)")) cmdData.distance = 2f;
             else if (Regex.IsMatch(text, @"(세|삼)")) cmdData.distance = 3f;
             else if (Regex.IsMatch(text, @"(네|사)")) cmdData.distance = 4f;
             else if (Regex.IsMatch(text, @"(다섯|오)")) cmdData.distance = 5f;
        }

        bool hasSeconds = Regex.IsMatch(text, @"(초|s|sec|동안)");

        if (Regex.IsMatch(text, @"(기다|대기|쉬어|wait|홀드)")) cmdData.action = ActionType.Wait;
        else if (Regex.IsMatch(text, @"(멈|정지|그만|스톱)"))
        {
            if (hasSeconds) cmdData.action = ActionType.Wait;
            else cmdData.action = ActionType.Stop;
        }
        else if (Regex.IsMatch(text, @"(점프|뛰|위로)")) cmdData.action = ActionType.Jump;
        else if (Regex.IsMatch(text, @"(가|이동|돌진|전진|후진|백|움직)")) cmdData.action = ActionType.Move;

        if (Regex.IsMatch(text, @"(뒤|후진|백|아래)")) cmdData.dir = MoveDir.Backward;
        else if (Regex.IsMatch(text, @"(왼|좌)")) cmdData.dir = MoveDir.Left;
        else if (Regex.IsMatch(text, @"(오른|우)")) cmdData.dir = MoveDir.Right;

        if (cmdData.action == ActionType.None)
        {
            if (cmdData.dir != MoveDir.Forward || (cmdData.distance != -1f && !hasSeconds))
                cmdData.action = ActionType.Move;
        }
        return cmdData;
    }
}