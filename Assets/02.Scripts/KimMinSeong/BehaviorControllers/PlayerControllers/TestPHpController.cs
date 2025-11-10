using System;
using UnityEngine;

public class PlayerHpController : MonoBehaviour, IDamageable
{
    private Component owner;
    private float currentHp;
    private float maxHp;
    private bool isDead;

    [SerializeField] GaugeOverdriveLogic gaugeOverdriveLogic;

    [Header("플레이어 무적 시간 설정")]
    [SerializeField] private float hitCooldown = 0.5f; // 피격 후 일정 시간 동안 무적
    private float lastHitTime; // 마지막으로 데미지를 받은 시각

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

    public event Action OnDeath;
    public static event Action<float, float> OnInitializeHp;
    public static event Action<float, float> OnTakeDamage;

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

        if (PlayerStateLogic.Instance.CurrentState == PlayerStateLogic.PlayerState.Normal)
        {
            currentHp -= damage;
            currentHp = Mathf.Max(0, currentHp);
            OnTakeDamage?.Invoke(currentHp, maxHp);

            if (currentHp <= 0)
                Die();
        }
        else if (PlayerStateLogic.Instance.CurrentState == PlayerStateLogic.PlayerState.OverDrive)
        {
            gaugeOverdriveLogic.UpOverDriveGauge(-10);
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
#endif
}

