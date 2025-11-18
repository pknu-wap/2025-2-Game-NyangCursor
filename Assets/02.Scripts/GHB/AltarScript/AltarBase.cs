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
        // ignoreNextExit = false;

        // if (IsPlayerInside)
        //     return;

        IsPlayerInside = true;
        PlayerInside = other.transform;

        other.GetComponent<PlayerAltarInteractor>()?.SetCurrentAltar(this);
        Debug.Log($"{gameObject.name}  {other.name} 할당됨 (Enter 처리)");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        HideGauge();
        other.GetComponent<PlayerAltarInteractor>()?.ClearCurrentAltar(this);
        Debug.Log($"{gameObject.name}  {other.name} 비할당 (Exit 처리)");
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
