using UnityEngine;
using UnityEngine.UI;

public class ExpUIManager : MonoBehaviour
{
    [SerializeField] private GameObject expUICanvas;  // 경험치 UI 오브젝트
    [SerializeField] private Image expBar;  // 경험치바 
    [SerializeField] private Image expProgress; // 경험치 획득률

    private void Awake()
    {
        expProgress.fillAmount = 0;
    }

    private void OnEnable()
    {
        ExpManager.OnExpChanged += UpdateExpUI;
        ExpManager.OnLevelUp += UpdateLevelUI;
    }

    private void OnDisable()
    {
        ExpManager.OnExpChanged -= UpdateExpUI;
        ExpManager.OnLevelUp -= UpdateLevelUI;
    }

    private void UpdateExpUI(int currentExp, int requiredExp, float expRatio)
    {
        // 세부 구현은 기획에 따라 변경 가능
        expProgress.fillAmount = expRatio;
    }

    private void UpdateLevelUI(int level)
    {
        // 세부 구현은 기획에 따라 변경 가능
    }
}
