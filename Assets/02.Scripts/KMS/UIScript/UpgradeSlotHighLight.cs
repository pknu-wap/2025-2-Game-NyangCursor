using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeSlotHighLight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Material material;
    [SerializeField] private float glowOnValue = 20f;
    [SerializeField] private float glowOffValue = 0f;
    [SerializeField] private float glowLerpSpeed = 5f;

    private Coroutine glowRoutine;
    private bool canHighlight = false; // 하이라이트 방지 타이머용 플래그

    private void OnEnable()
    {
        // 활성화될 때 Glow 초기화 + 짧은 대기
        if (glowRoutine != null) StopCoroutine(glowRoutine);
        StartCoroutine(DisableHighlightForSeconds(0.2f));
    }

    private void OnDisable()
    {
        // 비활성화될 때 Glow 0으로 리셋
        if (material != null)
            material.SetFloat("_Glow", glowOffValue);

        canHighlight = false;
    }

    private IEnumerator DisableHighlightForSeconds(float delay)
    {
        canHighlight = false;

        if (material != null)
            material.SetFloat("_Glow", glowOffValue);

        yield return new WaitForSeconds(delay);
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
            current = Mathf.Lerp(current, targetGlow, Time.deltaTime * glowLerpSpeed);
            material.SetFloat("_Glow", current);
            yield return null;
        }

        material.SetFloat("_Glow", targetGlow);
        glowRoutine = null;
    }
}
