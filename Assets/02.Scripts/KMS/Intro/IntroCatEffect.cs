using UnityEngine;
using System.Collections;

public class IntroCatEffect : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Material mat;

    [Header("Effect Durations (각자 다른 시간으로 감소)")]
    [SerializeField] private float hologramDuration = 1.5f;
    [SerializeField] private float pixelDuration = 1.2f;
    [SerializeField] private float alphaDuration = 2.0f;

    // 시작값
    private float startHolo = 0.2f; //최대 1
    private float startPixel = 110f;//최대 512
    private float startAlpha = 1f;//최대1

    // 종료값
    private float endHolo = 1f;
    private float endPixel = 4;
    private float endAlpha = 1f;

    private Coroutine holoRoutine;
    private Coroutine pixelRoutine;
    private Coroutine alphaRoutine;

    private void OnEnable()
    {
        if (mat == null)
        {
            Debug.LogError("Material is not assigned.");
            return;
        }

        // 초기값 세팅
        mat.SetFloat("_HologramBlend", startHolo);
        mat.SetFloat("_PixelateSize", startPixel);
        mat.SetFloat("_Alpha", startAlpha);
    }

    /// <summary>
    /// 🔥 외부에서 이 함수만 호출하면
    /// 3개 효과가 모두 다른 시간으로 비동기적으로 감소
    /// </summary>
    public void PlayDisappearEffect()
    {
        if (holoRoutine != null) StopCoroutine(holoRoutine);
        if (pixelRoutine != null) StopCoroutine(pixelRoutine);
        if (alphaRoutine != null) StopCoroutine(alphaRoutine);

        holoRoutine = StartCoroutine(HoloRoutine());
        pixelRoutine = StartCoroutine(PixelRoutine());
        alphaRoutine = StartCoroutine(AlphaRoutine());
    }

    private IEnumerator HoloRoutine()
    {
        float t = 0f;
        while (t < hologramDuration)
        {
            t += Time.deltaTime;
            float lerp = t / hologramDuration;
            mat.SetFloat("_HologramBlend", Mathf.Lerp(startHolo, endHolo, lerp));
            yield return null;
        }
        mat.SetFloat("_HologramBlend", endHolo);
    }

    private IEnumerator PixelRoutine()
    {
        float t = 0f;
        while (t < pixelDuration)
        {
            t += Time.deltaTime;
            float lerp = t / pixelDuration;
            mat.SetFloat("_PixelateSize", Mathf.Lerp(startPixel, endPixel, lerp));
            yield return null;
        }
        mat.SetFloat("_PixelateSize", endPixel);
    }

    private IEnumerator AlphaRoutine()
    {
        float t = 0f;
        while (t < alphaDuration)
        {
            t += Time.deltaTime;
            float lerp = t / alphaDuration;
            mat.SetFloat("_Alpha", Mathf.Lerp(startAlpha, endAlpha, lerp));
            yield return null;
        }
        mat.SetFloat("_Alpha", endAlpha);
    }
}
