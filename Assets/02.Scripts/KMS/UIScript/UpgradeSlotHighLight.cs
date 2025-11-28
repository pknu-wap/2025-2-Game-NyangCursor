using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradeSlotHighLight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Material material;
    [SerializeField] private float glowOnValue = 20f;
    [SerializeField] private float glowOffValue = 0f;
    [SerializeField] private float glowLerpSpeed = 5f;

    [Header("등급 표시 TMP 텍스트")]
    [SerializeField] private TextMeshProUGUI gradeTMP;  // ★ string 대신 TMP 사용

    [Header("등급별 Hue Shift 값")]
    [SerializeField] private float normalHue = 0f;
    [SerializeField] private float rareHue = 300f;
    [SerializeField] private float legendaryHue = 70f;

    private Coroutine glowRoutine;
    private bool canHighlight = false;

    private void OnEnable()
    {
        if (glowRoutine != null)
            StopCoroutine(glowRoutine);

        //ApplyHueByGrade();
        StartCoroutine(GlowFlashOnEnable());

        StartCoroutine(DisableHighlightForSeconds(0.2f));
    }

    private void OnDisable()
    {
        if (material != null)
            material.SetFloat("_Glow", glowOffValue);

        canHighlight = false;
    }

    private IEnumerator DisableHighlightForSeconds(float delay)
    {
        canHighlight = false;

        if (material != null)
            material.SetFloat("_Glow", glowOffValue);

        yield return new WaitForSecondsRealtime(delay);
        canHighlight = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!canHighlight) return;
        if (glowRoutine != null) StopCoroutine(glowRoutine);

        glowRoutine = StartCoroutine(LerpGlow(glowOnValue));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!canHighlight) return;
        if (glowRoutine != null) StopCoroutine(glowRoutine);

        glowRoutine = StartCoroutine(LerpGlow(glowOffValue));
    }

    private IEnumerator LerpGlow(float targetGlow)
    {
        if (material == null) yield break;

        float current = material.GetFloat("_Glow");

        while (!Mathf.Approximately(current, targetGlow))
        {
            current = Mathf.Lerp(current, targetGlow, Time.unscaledDeltaTime * glowLerpSpeed);
            material.SetFloat("_Glow", current);
            yield return null;
        }

        material.SetFloat("_Glow", targetGlow);
        glowRoutine = null;
    }

    private IEnumerator GlowFlashOnEnable()
    {
        if (material == null) yield break;

        material.SetFloat("_Glow", 10f);
        yield return new WaitForSecondsRealtime(0.1f);

        //glowRoutine = StartCoroutine(LerpGlow(glowOffValue));
    }

    private void ApplyHueByGrade()
    {
        if (material == null || gradeTMP == null) return;

        string grade = gradeTMP.text;  // ★ TMP 텍스트로 등급 확인

        float hueValue = normalHue;

        switch (grade)
        {
            case "[Normal]":
                hueValue = normalHue;
                break;

            case "[Rare]":
                hueValue = rareHue;
                break;

            case "[Legendary]":
                hueValue = legendaryHue;
                break;
        }

        material.SetFloat("_HsvShift", hueValue);
    }
}
