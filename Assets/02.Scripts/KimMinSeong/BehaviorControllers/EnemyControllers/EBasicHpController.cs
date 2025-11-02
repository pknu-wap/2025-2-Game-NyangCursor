
using System;
using UnityEngine;

public class EBasicHpController : MonoBehaviour, IDamageable
{
    private Enemy owner;
    private float currentHp;
    private float maxHp;
    private bool isDead;

    // 외부에서 필드를 쉽게 접근할 수 있도록 하기 위한 프로퍼티들
    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

    public event Action OnDeath;
    public event Action<float, float> OnInitializeHp;
    public event Action<float, float> OnTakeDamage;

    public void Initialize(Component owner)
    {
        // Enemy 타입만 허용
        if (owner is not Enemy enemy)
        {
            Debug.LogError($"EBasicHpController 는 Enemy 타입만 지원합니다. 현재 타입: {owner.GetType().Name}");
            return;
        }

        // 필드 초기화
        this.owner = enemy;
        maxHp = enemy.Data.maxHP;
        currentHp = maxHp;
        isDead = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) 
            return;

        currentHp -= damage;
        currentHp = Mathf.Max(0, currentHp);

        Debug.Log($"<color=yellow>[{owner.name}] 데미지 받음! -{damage} HP | 남은 체력: {currentHp}/{maxHp}</color>");

        if (currentHp <= 0)
        {
            Debug.Log($"<color=red>[{owner.name}] 사망!</color>");
            Debug.Log($"<color=red>[Die() 호출 시도]</color>");
            Die();
        }
    }

    public void Die()
    {
        if (isDead) 
            return;

        isDead = true;

        // 여기에 사망 처리 로직 추가 가능 (ex. 아이템, 경험치 드랍, 애니메이션 재생 등)
        // 죽었을때 이벤트 발행
        OnDeath?.Invoke();
    }

    public void Cleanup()
    {
        // 필요시 수정
    }

    // 디버깅용 - 씬 뷰에서 HP 바 표시
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
        float hpRatio = currentHp / maxHp;
        Gizmos.color = Color.green;
        Gizmos.DrawCube(position - new Vector3(barWidth * (1 - hpRatio) * 0.5f, 0, 0),
            new Vector3(barWidth * hpRatio, barHeight, 0));
    }
}