using UnityEngine;

public class PlayerAltarInteractor : MonoBehaviour
{
    private AltarBase currentAltar;
    private float holdTimer = 0f;
    private bool isCharging = false;

    private void Update()
    {
        // 제단이 없거나 이미 사용된 경우 초기화
        if (currentAltar == null || !currentAltar.IsPlayerInside || currentAltar.isUsed)
        {
            ResetGaugeState();
            return;
        }

        // 범위 안에 있을 때 자동 진행
        if (!isCharging)
        {
            isCharging = true;
            holdTimer = 0f;
            currentAltar.ShowGauge();
        }

        // 게이지 자동 증가
        holdTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(holdTimer / currentAltar.RequiredHoldTime);
        currentAltar.UpdateGauge(progress);

        // 완료 시 실행
        if (progress >= 1f)
        {
            currentAltar.Execute(transform);
            currentAltar.isUsed = true;
            ResetGaugeState();
        }
    }

    private void ResetGaugeState()
    {
        isCharging = false;
        holdTimer = 0f;
        if (currentAltar != null)
            currentAltar.HideGauge();
    }

    public void SetCurrentAltar(AltarBase altar)
    {
        currentAltar = altar;
        ResetGaugeState();
    }

    public void ClearCurrentAltar(AltarBase altar)
    {
        if (currentAltar == altar)
        {
            currentAltar = null;
            ResetGaugeState();
        }
    }
}
