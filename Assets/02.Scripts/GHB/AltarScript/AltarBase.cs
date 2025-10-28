using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public abstract class AltarBase : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float requiredHoldTime = 2f;
    [SerializeField] private Canvas gaugeCanvas; // 제단 자식, 게이지 UI
    [SerializeField] private Image gaugeFill;

    public float RequiredHoldTime => requiredHoldTime;
    public bool IsPlayerInside { get; private set; } = false;
    public Transform PlayerInside { get; private set; } = null;
    public bool isUsed = false;

    private void Awake()
    {
        if (gaugeCanvas != null)
            gaugeCanvas.enabled = false; // 초기에는 끔
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IsPlayerInside = true;
            PlayerInside = other.transform;

            // 플레이어에게 자기 자신 할당 (UI 켜기는 키 입력에서)
            other.GetComponent<PlayerAltarInteractor>()?.SetCurrentAltar(this);
            Debug.Log($"{gameObject.name}  {other.name} 할당됨");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IsPlayerInside = false;
            PlayerInside = null;

            // UI 끄고 초기화
            HideGauge();

            // 플레이어 해제
            other.GetComponent<PlayerAltarInteractor>()?.ClearCurrentAltar(this);
            Debug.Log($"{gameObject.name}  {other.name} 비할당");
        }
    }

    // 키 누를 때 UI 켜기
    public void ShowGauge()
    {
        if (gaugeCanvas != null)
            gaugeCanvas.enabled = true;
    }

    // 키 뗄 때 UI와 게이지 모두 초기화
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

    public void ResetGauge()
    {
        if (gaugeFill != null)
            gaugeFill.fillAmount = 0f;
    }

    // 제단별 기능 구현
    public abstract void Execute(Transform player);
}
