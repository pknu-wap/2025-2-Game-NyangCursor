using PixelUI;
using System;
using UnityEngine;
using DG.Tweening;

public class hpGaugeUI : MonoBehaviour
{
    [SerializeField] private RectTransform gaugeRect;
    [SerializeField] private ValueBar valueBar;

    // Glow 효과용
    [Header("Glow Settings")]
    [SerializeField] private Material glowMaterial;   // _Glow 값이 들어있는 머티리얼
    [SerializeField] private float glowPeak = 5f;
    [SerializeField] private float glowFadeTime = 0.25f;

    private int GlowID = Shader.PropertyToID("_Glow");

    private Vector2 shownPos = new Vector2(0f, 40f);
    private Vector2 hiddenPos = new Vector2(0f, -100f);

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
        gaugeRect.DOKill();
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

        TriggerGlowFlashOnce();
    }

    // ============================================
    //  Glow 한 번 깜빡이는 기능
    // ============================================
    private void TriggerGlowFlashOnce()
    {
        if (glowMaterial == null)
            return;

        // 기존 glow tween 중단
        glowMaterial.DOKill();

        // 즉시 5로 올림
        glowMaterial.SetFloat(GlowID, glowPeak);

        // 0으로 자연스럽게 감소
        glowMaterial
            .DOFloat(0f, GlowID, glowFadeTime)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }
}
