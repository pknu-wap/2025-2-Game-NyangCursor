using UnityEngine;
using System;

public class SelectCursorUI : MonoBehaviour
{
    public static Action<SelectCursorUI> OnSelectedEvent;

    [Header("선택 효과 (예: 네온Glow 오브젝트)")]
    [SerializeField] private GameObject selectEffect;

    private void OnEnable()
    {
        OnSelectedEvent += HandleSelectedEvent;
    }

    private void OnDisable()
    {
        OnSelectedEvent -= HandleSelectedEvent;
    }

    /// <summary>
    /// 슬롯/버튼에서 클릭될 때 호출될 함수
    /// </summary>
    public void SelectThis()
    {
        // 본인을 이벤트로 뿌림
        OnSelectedEvent?.Invoke(this);
    }

    private void HandleSelectedEvent(SelectCursorUI selectedUI)
    {
        // 본인이면 효과 ON, 아니면 OFF
        bool isMine = (selectedUI == this);
        selectEffect.SetActive(isMine);
    }
}