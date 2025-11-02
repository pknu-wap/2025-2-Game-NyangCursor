using PixelUI;
using UnityEngine;

public class hpGaugeUI : MonoBehaviour
{
  public ValueBar valueBar;

    private void OnEnable()
    {
         PlayerHpController.OnInitializeHp+= UpdateHpUI;
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
         valueBar.SetDirect(hpRatio);
    }
}
