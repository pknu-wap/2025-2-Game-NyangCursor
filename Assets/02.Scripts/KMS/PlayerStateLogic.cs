using System;
using UnityEngine;

public class PlayerStateLogic : MonoBehaviour
{
    public static PlayerStateLogic Instance { get; private set; }

    public enum PlayerState
    {
     Normal, //일반
     Riding, //탑승 중
     OverDrive,//오버드라이브
     Berserk,//폭주
     GetOff,//내리는 중
     Die //죽음
    }

    public event Action<PlayerState> OnStateChanged;
    public PlayerState CurrentState = PlayerState.Normal;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }
        Instance = this;
    }

    public void ChangeState(PlayerState newState)
    {
        if (CurrentState == newState)
            return; // 같은 상태면 무시

            // 상태 변경
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
        Debug.Log($"[PlayerStateLogic] 상태 변경: {newState}");
    }
}
