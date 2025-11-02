using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpUIManager : MonoBehaviour
{
    [SerializeField] private GameObject hpUICanvas;  // 플레이어 Hp UI 오브젝트
    [SerializeField] private Image hpBar;  // 체력바
    [SerializeField] private Image hpBorder; // 체력바 테두리
    [SerializeField] private Image hpProgress; // 현재 체력 상태
    [SerializeField] private TextMeshProUGUI hpText; // 현재 체력과 최대 체력을 보여주는 텍스트

    private void OnEnable()
    {
        PlayerHpController.OnInitializeHp += UpdateHpUI;
        PlayerHpController.OnTakeDamage += UpdateHpUI;
    }

    private void OnDisable()
    {
        PlayerHpController.OnInitializeHp -= UpdateHpUI;
        PlayerHpController.OnTakeDamage -= UpdateHpUI;
    }

    private void UpdateHpUI(float currentHp, float maxHp)
    {
        // 세부 구현은 기획에 따라 변경 가능
        float hpRatio = currentHp / maxHp;
        hpProgress.fillAmount = hpRatio;
        hpText.text = $"{(int)currentHp} / {(int)maxHp}";
    }
}
