using UnityEngine;

public class PlayerHpController : MonoBehaviour, IDamageable
{
    private float maxHp;

    private Component owner;  // Player 참조
    private float currentHp;
    private bool isDead;

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

    public void Initialize(Component owner)
    {
        this.owner = owner;
        maxHp = PlayerStatsManager.instance.GetStat(StatType.MaxHealth);
        currentHp = maxHp;
        isDead = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHp -= damage;
        currentHp = Mathf.Max(0, currentHp);

        Debug.Log($"Player 가 {damage} 만큼의 데미지를 입었습니다: {currentHp}/{maxHp}");

        if (currentHp <= 0)
            Die();
    }

    public void Die()
    {
        if (isDead) 
            return;

        isDead = true;
        Debug.Log("Player 사망");

        // 테스트용: 게임 일시정지
        Time.timeScale = 0;
    }

    public void Cleanup()
    {
        // 필요시 수정
    }

    // 디버그용 - 화면에 HP 표시
    private void OnGUI()
    {
        GUI.color = Color.white;
        GUI.Label(new Rect(10, 10, 300, 30), $"Player HP: {currentHp:F1}/{maxHp:F1}");

        if (isDead)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(10, 40, 300, 30), "DEAD - Press R to Restart");
        }

        // R키로 재시작
        if (isDead && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
    }
}