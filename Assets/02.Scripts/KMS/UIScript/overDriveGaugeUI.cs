using PixelUI;
using System.Collections;
using UnityEngine;
using DG.Tweening; // DOTween 추가

public class overDriveGaugeUI : MonoBehaviour
{
    [SerializeField] private RectTransform gaugeRect;
    [SerializeField] private ValueBar valueBar;
    [SerializeField] private Material gaugeMaterial;

    private Coroutine colorChangeRoutine;
    private Vector2 shownPos = new Vector2(0f, 40f);
    private Vector2 hiddenPos = new Vector2(0f, -100f);

    private void OnEnable()
    {
        GaugeOverdriveLogic.OnOverDriveTick += HandleUpdateOverDriveGauge;
        GaugeRidingLogic.OnOverDriveEvent += HandleShowOverDriveGauge;
        GaugeOverdriveLogic.OnGetOffEvent += HandleHideOverDriveGauge;
    }

    private void OnDisable()
    {
        GaugeOverdriveLogic.OnOverDriveTick -= HandleUpdateOverDriveGauge;
        GaugeRidingLogic.OnOverDriveEvent -= HandleShowOverDriveGauge;
        GaugeOverdriveLogic.OnGetOffEvent -= HandleHideOverDriveGauge;
    }

    private void HandleUpdateOverDriveGauge(float value)
    {
        // 게이지 UI 반영
        valueBar.SetDirect(value);

        // 0~100 기준으로 4단계 색상 나누기
        float targetHue = 0f;   // HSV Hue
        float targetSat = 2f;
        float targetBright = 1f;

        if (value < 20f)
        {
            targetHue = 330f; // 빨강
        }
        else if (value < 50f)
        {
            targetHue = 310f; // 노랑
        }
        else if (value < 75f)
        {
            targetHue = 265f; // 초록
        }
        else
        {
            targetHue = 265f; // 보라
        }

        // 이미 실행 중이면 중단하고 새로 시작
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

            float newHue = Mathf.Lerp(startHue, targetHue, t);
            float newSat = Mathf.Lerp(startSat, targetSat, t);
            float newBright = Mathf.Lerp(startBright, targetBright, t);

            gaugeMaterial.SetFloat("_HsvShift", newHue);
            gaugeMaterial.SetFloat("_HsvSaturation", newSat);
            gaugeMaterial.SetFloat("_HsvBright", newBright);

            yield return null;
        }

        gaugeMaterial.SetFloat("_HsvShift", targetHue);
        gaugeMaterial.SetFloat("_HsvSaturation", targetSat);
        gaugeMaterial.SetFloat("_HsvBright", targetBright);

        colorChangeRoutine = null;
    }

    // 🔹 오버드라이브 진입 시 게이지 표시
    private void HandleShowOverDriveGauge()
    {
        gaugeRect.DOKill();
        gaugeRect.DOAnchorPos(shownPos, 0.6f)
                 .SetEase(Ease.OutBack)
                 .SetUpdate(true);
    }

    // 🔹 노말 상태 복귀 시 게이지 숨김
    private void HandleHideOverDriveGauge()
    {
        gaugeRect.DOKill();
        gaugeRect.DOAnchorPos(hiddenPos, 0.4f)
                 .SetEase(Ease.InBack)
                 .SetUpdate(true);
    }
}
