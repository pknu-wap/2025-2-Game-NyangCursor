using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PushBlast : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float pushPower = 15f;
    [SerializeField] private LayerMask enemyLayer;
    [Header("Visual Settings")]
    [SerializeField] private Color circleColor = new Color(1f, 0.5f, 0f, 0.4f);
    [SerializeField] private int circleSegments = 64;
    [SerializeField] private float showDuration = 0.3f;

    private LineRenderer lineRenderer;
    private float lastTriggerTime = -999f;
    [SerializeField] private float triggerCooldown = 0.5f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = circleSegments + 1;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = circleColor;
        lineRenderer.endColor = circleColor;
        lineRenderer.enabled = false; // 기본적으로 숨김
    }

    private void OnEnable()
    {
        GaugeOverdriveLogic.OnBoostEvent += Trigger;
    }

    private void OnDisable()
    {
        GaugeOverdriveLogic.OnBoostEvent -= Trigger;
    }


    void Update()
    {
        // 🔹 수동 테스트용: S키 누르면 폭발 트리거 실행
        if (Input.GetKeyDown(KeyCode.S))
        {
            Trigger();
        }
    }


    public void Trigger()
    {
        if (Time.time - lastTriggerTime < triggerCooldown)
            return;
        lastTriggerTime = Time.time;

        ShowRadiusCircle();

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        Debug.Log($"Detected enemies: {enemies.Length}");

        foreach (var col in enemies)
        {
            Rigidbody2D rb = col.attachedRigidbody;
            if (rb != null)
            {
                Vector2 dir = (col.transform.position - transform.position).normalized;
                rb.AddForce(dir * pushPower, ForceMode2D.Impulse);

                var mover = col.GetComponent<EBasicMoveController2>();
                if (mover != null)
                    mover.StopForSeconds(0.6f);
            }
        }
    }

    private void ShowRadiusCircle()
    {
        // 원형 라인 세팅
        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = (float)i / circleSegments * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            lineRenderer.SetPosition(i, pos);
        }

        // 원 표시
        lineRenderer.enabled = true;
        CancelInvoke(nameof(HideCircle));
        Invoke(nameof(HideCircle), showDuration);
    }

    private void HideCircle()
    {
        lineRenderer.enabled = false;
    }
}
