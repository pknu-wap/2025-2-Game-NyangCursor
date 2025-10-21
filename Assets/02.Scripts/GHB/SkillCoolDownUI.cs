using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillCooldownUI : MonoBehaviour
{
    [SerializeField] private Image cooldownMask; // CooldownMask Image
    [SerializeField] private float cooldownTime = 5f; // 테스트용 쿨타임

    private Coroutine cooldownRoutine;

    private void Awake()
    {
        if (cooldownMask != null)
            cooldownMask.fillAmount = 0; // 기본적으로 쿨타임 없음
    }

    public void StartCooldown(float duration)
    {
        if (cooldownRoutine != null)
            StopCoroutine(cooldownRoutine);

        cooldownRoutine = StartCoroutine(CooldownRoutine(duration));
    }

    private IEnumerator CooldownRoutine(float duration)
    {
        float elapsed = 0f;

        cooldownMask.fillAmount = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cooldownMask.fillAmount = 1f - (elapsed / duration);
            yield return null;
        }

        cooldownMask.fillAmount = 0f; // 완료 시 완전히 사라짐
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            StartCooldown(cooldownTime);
        }
    }
}
