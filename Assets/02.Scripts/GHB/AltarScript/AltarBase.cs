using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public abstract class AltarBase : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float requiredHoldTime = 2f;
    [SerializeField] private Canvas gaugeCanvas; 
    [SerializeField] private Image gaugeFill;

    public float RequiredHoldTime => requiredHoldTime;
    public bool IsPlayerInside { get; private set; } = false;
    public Transform PlayerInside { get; private set; } = null;
    public bool isUsed = false;

    private bool ignoreNextExit = false; // 피격 때문에 Exit 이벤트 무시

    private void Awake()
    {
        if (gaugeCanvas != null)
            gaugeCanvas.enabled = false; 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // 이미 안에 있어도 Enter 들어오면 ignoreNextExit 초기화
        ignoreNextExit = false;

        if (IsPlayerInside)
            return;

        IsPlayerInside = true;
        PlayerInside = other.transform;

        other.GetComponent<PlayerAltarInteractor>()?.SetCurrentAltar(this);
        Debug.Log($"{gameObject.name}  {other.name} 할당됨 (Enter 처리)");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (ignoreNextExit)
        {
            // 피격 때문에 Exit 무시
            Debug.Log($"{gameObject.name} {other.name} EXIT 무시됨 (피격 보정)");
            StartCoroutine(ResetIgnoreExit());
            return;
        }

        // 실제 Exit 처리
        IsPlayerInside = false;
        PlayerInside = null;

        HideGauge();
        other.GetComponent<PlayerAltarInteractor>()?.ClearCurrentAltar(this);
        Debug.Log($"{gameObject.name}  {other.name} 비할당 (Exit 처리)");
    }

    // 플레이어가 피격 시 호출
    public void IgnoreExitTemporarily(float duration)
    {
        ignoreNextExit = true;
        StartCoroutine(ResetIgnoreExitAfter(duration));
    }

    private IEnumerator ResetIgnoreExitAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        ignoreNextExit = false;
    }

    private IEnumerator ResetIgnoreExit()
    {
        yield return null; // 한 프레임 기다리기만 해도 꼬임 방지
        ignoreNextExit = false;
    }

    public void ShowGauge()
    {
        if (gaugeCanvas != null)
            gaugeCanvas.enabled = true;
    }

    public void HideGauge()
    {
        if (gaugeCanvas != null)
            gaugeCanvas.enabled = false;
        if (gaugeFill != null)
            gaugeFill.fillAmount = 0f;
    }

    public void UpdateGauge(float progress)
    {
        if (gaugeFill != null)
            gaugeFill.fillAmount = progress;
    }

    public abstract void Execute(Transform player);
}
