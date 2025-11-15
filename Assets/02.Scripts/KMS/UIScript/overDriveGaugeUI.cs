using PixelUI;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class overDriveGaugeUI : MonoBehaviour
{
    [SerializeField] private RectTransform gaugeRect;
    [SerializeField] private ValueBar valueBar;
    [SerializeField] private Material gaugeMaterial;

    private Coroutine colorChangeRoutine;
    private Vector2 shownPos = new Vector2(0f, 40f);
    private Vector2 hiddenPos = new Vector2(0f, -100f);

    // 🔥 Glow 설정
    [Header("Glow Effect")]
    [SerializeField] private float glowPeak = 4f;        // 즉시 올라갈 값
    [SerializeField] private float glowFadeTime = 0.25f; // 다시 내려오는 시간

    private int GlowID = Shader.PropertyToID("_Glow");

    private void OnEnable()
    {
        GaugeOverdriveLogic.OnOverDriveTick += HandleUpdateOverDriveGauge;
        GaugeRidingLogic.OnOverDriveEvent += HandleShowOverDriveGauge;
        GaugeOverdriveLogic.OnGetOffEvent += HandleHideOverDriveGauge;
        GaugeOverdriveLogic.OnUpOverDriveGauge += HandleGlowGauge;
    }

    private void OnDisable()
    {
        GaugeOverdriveLogic.OnOverDriveTick -= HandleUpdateOverDriveGauge;
        GaugeRidingLogic.OnOverDriveEvent -= HandleShowOverDriveGauge;
        GaugeOverdriveLogic.OnGetOffEvent -= HandleHideOverDriveGauge;
        GaugeOverdriveLogic.OnUpOverDriveGauge -= HandleGlowGauge;
    }

    private void HandleUpdateOverDriveGauge(float value)
    {
        valueBar.SetDirect(value);

        float targetHue = 0f;
        float targetSat = 2f;
        float targetBright = 1f;

        if (value < 20f)
            targetHue = 330f;
        else if (value < 50f)
            targetHue = 310f;
        else if (value < 75f)
            targetHue = 265f;
        else
            targetHue = 265f;

        if (colorChangeRoutine != null)
            StopCoroutine(colorChangeRoutine);

        colorChangeRoutine = StartCoroutine(LerpColorChange(targetHue, targetSat, targetBright, 0.4f));
    }

    private IEnumerator LerpColorChange(float targetHue, float targetSat, float targetBright, float duration)
    {
        float startHue = gaugeMaterial.GetFloat("_HsvShift");
        float startSat = gaugeMaterial.GetFloat("_HsvSaturation");
        float startBright = gaugeMaterial.GetFloat("_HsvBright");

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            gaugeMaterial.SetFloat("_HsvShift", Mathf.Lerp(startHue, targetHue, t));
            gaugeMaterial.SetFloat("_HsvSaturation", Mathf.Lerp(startSat, targetSat, t));
            gaugeMaterial.SetFloat("_HsvBright", Mathf.Lerp(startBright, targetBright, t));

            yield return null;
        }

        gaugeMaterial.SetFloat("_HsvShift", targetHue);
        gaugeMaterial.SetFloat("_HsvSaturation", targetSat);
        gaugeMaterial.SetFloat("_HsvBright", targetBright);

        colorChangeRoutine = null;
    }

    // =====================================================================
    //    ⭐ Glow 효과 추가 부분 (UpOverDriveGauge 이벤트에서 실행)
    // =====================================================================
    private void HandleGlowGauge()
    {
        if (gaugeMaterial == null)
            return;

        gaugeMaterial.DOKill();

        // 즉시 강한 Glow 설정
        gaugeMaterial.SetFloat(GlowID, glowPeak);

        // 0으로 감소
        gaugeMaterial
            .DOFloat(2f, GlowID, glowFadeTime)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    private void HandleShowOverDriveGauge()
    {
        gaugeRect.DOKill();
        gaugeRect.DOAnchorPos(shownPos, 0.6f)
                 .SetEase(Ease.OutBack)
                 .SetUpdate(true);
    }

    private void HandleHideOverDriveGauge()
    {
        gaugeRect.DOKill();
        gaugeRect.DOAnchorPos(hiddenPos, 0.4f)
                 .SetEase(Ease.InBack)
                 .SetUpdate(true);
    }
}
