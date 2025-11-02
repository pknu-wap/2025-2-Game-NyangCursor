using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpUIManager : MonoBehaviour
{
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
        hpText.text = $"{(int)currentHp} / {(int)maxHp}";
    }
}
