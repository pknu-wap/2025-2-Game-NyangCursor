using PixelUI;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class overDriveGaugeUI : MonoBehaviour
{
    [SerializeField] ValueBar valueBar;
    [SerializeField] Material material;
    [SerializeField] private Material gaugeMaterial;
    private Coroutine colorChangeRoutine;

    private void OnEnable()
    {
        // 이벤트 구독
        GaugeOverdriveLogic.OnOverDriveTick += HandleUpdateOverDriveGauge;
    }

    private void OnDisable()
    {
        // 이벤트 해제
        GaugeOverdriveLogic.OnOverDriveTick -= HandleUpdateOverDriveGauge;
    }



private void HandleChangeColorGauge(int tier)
{
    float targetHue = 0f;
    float targetSat = 2f;
    float targetBright = 1f;

    switch (tier)
    {
        case 0:
            targetHue = 0f;   // 빨강 계열
            targetSat = 2f;
            targetBright = 1f;

            // 🔸 Overlay 효과 끄기
            if (gaugeMaterial.IsKeywordEnabled("OVERLAY_ON"))
                gaugeMaterial.DisableKeyword("OVERLAY_ON");
            break;

        case 1:
            targetHue = 60f;  // 노랑 계열
            targetSat = 2f;
            targetBright = 1f;
            break;

        case 2:
            targetHue = 126f; // 초록 계열
            targetSat = 2f;
            targetBright = 1f;
            break;

        case 3:
            targetHue = 178f; // 파랑 계열
            targetSat = 2f;
            targetBright = 1f;
            break;

        case 4:
            targetHue = 260f; // 보라 계열 (원하면 조정 가능)
            targetSat = 2f;
            targetBright = 1f;

            // 🔸 Overlay 효과 켜기
            if (!gaugeMaterial.IsKeywordEnabled("OVERLAY_ON"))
                gaugeMaterial.EnableKeyword("OVERLAY_ON");
            break;
    }

    // 이미 실행 중이면 중단
    if (colorChangeRoutine != null)
        StopCoroutine(colorChangeRoutine);

    colorChangeRoutine = StartCoroutine(LerpColorChange(targetHue, targetSat, targetBright, 0.8f));
}


    private IEnumerator LerpColorChange(float targetHue, float targetSat, float targetBright, float duration)
    {
        // 시작값 저장
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

        // 마지막 값 보정
        gaugeMaterial.SetFloat("_HsvShift", targetHue);
        gaugeMaterial.SetFloat("_HsvSaturation", targetSat);
        gaugeMaterial.SetFloat("_HsvBright", targetBright);

        colorChangeRoutine = null;
    }

    private void HandleUpdateOverDriveGauge(float value)
    {
        valueBar.SetDirect(value);
    }

    

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
