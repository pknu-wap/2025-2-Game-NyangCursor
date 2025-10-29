using UnityEngine;

public class PlayerAltarInteractor : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private AltarBase currentAltar;
    private float holdTimer = 0f;
    private bool isHolding = false; // 게이지 진행 중인지

    private void Update()
    {
        if (currentAltar == null || !currentAltar.IsPlayerInside || currentAltar.isUsed)
        {
            ResetGaugeState();
            return;
        }

        // 키 눌러서 게이지 시작
        if (Input.GetKeyDown(interactKey))
        {
            isHolding = true;
            holdTimer = 0f;
            currentAltar.ShowGauge();
        }

        // 키 누르고 있는 동안 게이지 증가
        if (isHolding && Input.GetKey(interactKey))
        {
            holdTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(holdTimer / currentAltar.RequiredHoldTime);
            currentAltar.UpdateGauge(progress);

            if (progress >= 1f)
            {
                currentAltar.Execute(transform);
                currentAltar.isUsed = true;
                ResetGaugeState();
            }
        }

        // 키 뗐을 때 초기화
        if (Input.GetKeyUp(interactKey))
        {
            ResetGaugeState();
        }
    }

    private void ResetGaugeState()
    {
        isHolding = false;
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
