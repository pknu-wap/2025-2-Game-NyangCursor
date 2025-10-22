using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillCooldownUI : MonoBehaviour
{
    [SerializeField] private Image cooldownMask;

    private Coroutine cooldownRoutine;

    public void StartCooldown(float duration)
    {
        if (duration <= 0f || cooldownMask == null) return;

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
    }
}
