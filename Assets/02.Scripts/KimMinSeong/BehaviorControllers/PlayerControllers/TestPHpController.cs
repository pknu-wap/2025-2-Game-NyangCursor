using System;
using System.Collections;
using UnityEngine;

public class PlayerHpController : MonoBehaviour, IDamageable
{
    private Component owner;
    private float currentHp;
    private float maxHp;
    private bool isDead;

    private Rigidbody2D rb; // ← Rigidbody 자동 할당용 변수

    [SerializeField] private GaugeOverdriveLogic gaugeOverdriveLogic;

    [Header("플레이어 무적 시간 설정")]
    [SerializeField] private float hitCooldown = 1f; // 피격 후 일정 시간 동안 무적
    private float lastHitTime; // 마지막으로 데미지를 받은 시각

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

    public event Action OnDeath;
    public static event Action<float, float> OnInitializeHp;
    public static event Action<float, float> OnTakeDamage;
    public static event Action OnReduceGauge;

    private void Awake()
    {
        // Rigidbody 자동 할당 (없으면 경고)
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
        lastHitTime = -hitCooldown; // 시작 시 즉시 피격 가능

        OnInitializeHp?.Invoke(currentHp, maxHp);
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        // 무적시간 체크
        if (Time.time - lastHitTime < hitCooldown)
            return;

        // 피격이 유효하면 공통적으로 시간 갱신
        lastHitTime = Time.time;

        //Rigidbody 속도를 0으로 만들어 피격 시 멈추게 하기
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // 속도 즉시 정지
        }

        if (PlayerStateLogic.Instance.CurrentState == PlayerStateLogic.PlayerState.Normal)
        {
            currentHp -= damage;
            currentHp = Mathf.Max(0, currentHp);
            OnTakeDamage?.Invoke(currentHp, maxHp);
            // 데미지 들어오면 잠시 Kinematic으로 변경 (밀림 방지)
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;

            var altar = GetComponent<PlayerAltarInteractor>()?.currentAltar;
            altar?.IgnoreExitTemporarily(0.5f); // 0.5초 동안 Exit 무시

            // 0.5초 뒤에 다시 Dynamic 복귀
            StartCoroutine(RestoreDynamicBody(0.4f));

            if (currentHp <= 0)
                Die();
        }
        else if (PlayerStateLogic.Instance.CurrentState == PlayerStateLogic.PlayerState.OverDrive)
        {
            gaugeOverdriveLogic.UpOverDriveGauge(-10);
            OnReduceGauge?.Invoke();
        }
    }

    private IEnumerator RestoreDynamicBody(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 혹시 이미 죽었거나 사라졌으면 복구 안 함
        if (rb != null && gameObject.activeInHierarchy)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // 플레이어 사망 시 게임 종료 요청
        StageFlowManager.instance.SetStateToEnd();
    }

    public void Cleanup()
    {
        // 필요 시 추가 정리
    }

#if UNITY_EDITOR
    //  디버깅용 - 씬 뷰에서 HP바 표시
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || isDead)
            return;

        Vector3 position = transform.position + Vector3.up * 2f;
        float barWidth = 1f;
        float barHeight = 0.1f;

        // 배경 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawCube(position, new Vector3(barWidth, barHeight, 0));

        // 현재 HP (초록색)
        float hpRatio = maxHp > 0 ? currentHp / maxHp : 0;
        Gizmos.color = Color.green;
        Gizmos.DrawCube(position - new Vector3(barWidth * (1 - hpRatio) * 0.5f, 0, 0),
            new Vector3(barWidth * hpRatio, barHeight, 0));
    }

    public void TakeCollisionDamage(float amount)
    {
        //몸박이여서 코드를 옮겨야함
        //스킬데미지랑 따로
    }
#endif
}
