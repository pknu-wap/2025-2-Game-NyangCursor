using TMPro;
using UnityEngine;

public class GoldUIManager : MonoBehaviour
{
    [SerializeField] private GameObject goldUICanvas;  // 골드 UI 오브젝트
    [SerializeField] private TextMeshProUGUI goldText;  // 현재 골드량을 표시할 UI

    public void UpdateGoldUI(int savedGold, int earnedGold)
    {
        goldText.text = $"savedGold:${savedGold} / earnedGold:${earnedGold}";
    }
}
