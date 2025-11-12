using PixelUI;
using System;
using UnityEngine;
using DG.Tweening; // DOTween 추가

public class hpGaugeUI : MonoBehaviour
{
    [SerializeField] private RectTransform gaugeRect;
    [SerializeField] private ValueBar valueBar;

    private Vector2 shownPos = new Vector2(0f, 40f);      // 보일 때 위치
    private Vector2 hiddenPos = new Vector2(0f, -100f);  // 숨김 위치

    private void OnEnable()
    {
        PlayerHpController.OnInitializeHp += UpdateHpUI;
        PlayerHpController.OnTakeDamage += UpdateHpUI;

        GaugeRidingLogic.OnRidingEvent += HandleHideUI;
        GaugeOverdriveLogic.OnNormalEvent += HandleShowUI;
    }

    private void OnDisable()
    {
        PlayerHpController.OnInitializeHp -= UpdateHpUI;
        PlayerHpController.OnTakeDamage -= UpdateHpUI;

        GaugeRidingLogic.OnRidingEvent -= HandleHideUI;
        GaugeOverdriveLogic.OnNormalEvent -= HandleShowUI;
    }

    private void HandleHideUI()
    {
        gaugeRect.DOKill(); // 기존 트윈 중단
        gaugeRect.DOAnchorPos(hiddenPos, 0.4f)
                 .SetEase(Ease.InBack)
                 .SetUpdate(true);
    }

    private void HandleShowUI()
    {
        gaugeRect.DOKill();
        gaugeRect.DOAnchorPos(shownPos, 0.8f)
                 .SetEase(Ease.OutBack)
                 .SetUpdate(true);
    }

    private void UpdateHpUI(float currentHp, float maxHp)
    {
        float hpRatio = currentHp / maxHp;
        valueBar.SetDirect(hpRatio);
    }
}
