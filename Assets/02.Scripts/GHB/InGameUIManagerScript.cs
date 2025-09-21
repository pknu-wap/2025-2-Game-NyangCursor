using UnityEngine;
using TMPro; // TextMeshProUGUI 사용
using System;

public class InGameUIManager : MonoBehaviour
{
    [Header("타이머 텍스트")]
    [SerializeField] private TextMeshProUGUI timeText;

    void OnEnable()
    {
        TimerManager.OnTimerTick += UpdateTimerUI;
    }

    void OnDisable()
    {
        TimerManager.OnTimerTick -= UpdateTimerUI;
    }

    private void UpdateTimerUI(float elapsedTime)
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        // 두 자리 포맷
        timeText.text = $"{minutes:00}:{seconds:00}";
    }
}
