using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillCooldownUI : MonoBehaviour
{
    [SerializeField] private Image cooldownMask;

    private Coroutine cooldownRoutine;

    // 쿨다운 시작
    public void StartCooldown(float duration)
    {
        if (duration <= 0f || cooldownMask == null) return;

        // 기존 코루틴 종료
        if (cooldownRoutine != null)
            StopCoroutine(cooldownRoutine);

        cooldownRoutine = StartCoroutine(CooldownRoutine(duration));
    }

    private IEnumerator CooldownRoutine(float duration)
    {
        cooldownMask.fillAmount = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cooldownMask.fillAmount = 1f - (elapsed / duration);
            yield return null;
        }

        cooldownMask.fillAmount = 0f;
        cooldownRoutine = null; // 추가: 종료 시 null 처리
    }

    // ======================= 상태 관련 메서드 =======================

    // 즉시 시전 가능 표시
    public void ForceReset()
    {
        // 진행 중인 코루틴 종료
        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null; // 추가: 종료 후 null 처리
        }
        if (cooldownMask != null)
            cooldownMask.fillAmount = 0f;

    }

    // 사용 불가 상태 표시
    public void ForceFill()
    {
        // 진행 중인 코루틴 종료
        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null; // 추가: 종료 후 null 처리
        }
        if (cooldownMask != null)
            cooldownMask.fillAmount = 1f;
    }

}
