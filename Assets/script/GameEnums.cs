using UnityEngine;

// 1. 행동 타입 (Enum)
public enum ActionType { Move, Jump, Stop, Wait, None }

// 2. 이동 방향 (Enum)
public enum MoveDir { Forward, Backward, Left, Right }

// 3. 데이터 가방 (Class) - [System.Serializable] 필수!
[System.Serializable]
public class CommandData
{
    public ActionType action;
    public MoveDir dir;
    public float distance;
}