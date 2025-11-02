using PixelUI;
using UnityEngine;

public class expGaugeUI : MonoBehaviour
{
    public ValueBar valueBar;

    private void OnEnable()
    {
        ExpManager.OnExpChanged += UpdateExpUI;
        //ExpManager.OnLevelUp += UpdateLevelUI;
    }

    private void OnDisable()
    {
        ExpManager.OnExpChanged -= UpdateExpUI;
        //ExpManager.OnLevelUp -= UpdateLevelUI;
    }
    
       private void UpdateExpUI(int currentExp, int requiredExp, float expRatio)
    {
        // 세부 구현은 기획에 따라 변경 가능
        valueBar.SetDirect(expRatio);
  
    }




}
