using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemAltarUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Button useButton;
    [SerializeField] private Button BackButton;


    public void Setup(ItemSO item, Transform playerTransform)
    {
        if (itemNameText != null) itemNameText.text = item.itemName;
        if (itemIconImage != null) itemIconImage.sprite = item.itemImage;
        if (itemDescriptionText != null) itemDescriptionText.text = item.itemDescription;

        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(() =>
        {
            item.Execute(playerTransform); // 실행
            // 플레이어에서 ITEMMANAGER GETCOMPONENT 해서 LIST에 추가해두는 등 작업 가능
            ClosUI();

        });
        BackButton.onClick.RemoveAllListeners();
        BackButton.onClick.AddListener(() =>
        {
            ClosUI();
        });
    }

    private void ClosUI()
    {
        StageFlowManager.instance.SetStateToPlay();
        gameObject.SetActive(false);
    }

}
