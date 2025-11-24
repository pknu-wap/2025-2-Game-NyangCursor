using System;
using System.Collections;
using UnityEngine;

public class PlayerHpController : MonoBehaviour, IDamageable
{
    public static PlayerHpController Instance { get; private set; }

    private Component owner;
    private float currentHp;
    private float maxHp;
    private bool isDead;

    private Rigidbody2D rb;

    [SerializeField] private GaugeOverdriveLogic gaugeOverdriveLogic;

    [Header("플레이어 무적 시간 설정")]
    [SerializeField] private float hitCooldown = 1f;
    private float lastHitTime;

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

    public static event Action<float, float> OnInitializeHp;
    public static event Action<float, float> OnTakeDamage;
    public static event Action OnReduceGauge;

    private void Awake()
    {
        // Singleton Setup (추가된 코드)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogWarning("[PlayerHpController] Rigidbody2D를 찾을 수 없습니다.");
    }

    public void Initialize(Component owner)
    {
        this.owner = owner;
        maxHp = PlayerStatsManager.instance.GetStat(StatType.MaxHealthUp);
        currentHp = maxHp;
        isDead = false;
        lastHitTime = -hitCooldown;

        OnInitializeHp?.Invoke(currentHp, maxHp);
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        if (Time.time - lastHitTime < hitCooldown)
            return;

        lastHitTime = Time.time;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (PlayerStateLogic.Instance.CurrentState == PlayerStateLogic.PlayerState.Normal)
        {
            currentHp -= damage;
            currentHp = Mathf.Max(0, currentHp);
            OnTakeDamage?.Invoke(currentHp, maxHp);

            // 글로벌 이벤트 버스 사용 → 카메라 흔들림 적용
            GameEvents.Publish(GameEventType.OnPlayerTakeDamage);

            rb.linearVelocity = Vector2.zero;
             rb.constraints = RigidbodyConstraints2D.FreezePosition;

            StartCoroutine(RestoreMovement(0.4f));

            if (currentHp <= 0)
                Die();
        }
        else if (PlayerStateLogic.Instance.CurrentState == PlayerStateLogic.PlayerState.OverDrive)
        {
            gaugeOverdriveLogic.UpOverDriveGauge(-10);
            OnReduceGauge?.Invoke();
        }
    }

    public void TakeCollisionDamage(float amount)
    {
        // 필요시 구현
    }

    private IEnumerator RestoreMovement(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (rb != null && gameObject.activeInHierarchy)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        StageFlowManager.instance.SetStateToEnd();
    }

    public void Cleanup()
    {
        // 필요하면 여기 추가
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || isDead)
            return;

        Vector3 position = transform.position + Vector3.up * 2f;
        float barWidth = 1f;
        float barHeight = 0.1f;

        Gizmos.color = Color.red;
        Gizmos.DrawCube(position, new Vector3(barWidth, barHeight, 0));

        float hpRatio = maxHp > 0 ? currentHp / maxHp : 0;
        Gizmos.color = Color.green;
        Gizmos.DrawCube(position - new Vector3(barWidth * (1 - hpRatio) * 0.5f, 0, 0),
            new Vector3(barWidth * hpRatio, barHeight, 0));
    }
#endif
}
