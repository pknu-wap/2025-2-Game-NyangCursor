using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.PlayerLoop;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform contentParent; // GridLayoutGroup이 붙은 Content
    [SerializeField] private GameObject itemPrefab;   // 상점 아이템 프리팹
    [SerializeField] private List<ShopStatData> shopItems = new List<ShopStatData>(); // 스크립터블 오브젝트 배열

    private void Start()
    {
        InitializateShop();
    }

    private void InitializateShop()
    {
        foreach (var data in shopItems)
        {
            // 프리팹 생성
            GameObject item = Instantiate(itemPrefab, contentParent);

            // 자식 컴포넌트 가져오기
            Image iconImage = item.transform.Find("Icon").GetComponent<Image>();
            TMP_Text nameText = item.transform.Find("NameText").GetComponent<TMP_Text>();
            TMP_Text priceText = item.transform.Find("PriceText").GetComponent<TMP_Text>();
            Button buyButton = item.transform.Find("BuyButton").GetComponent<Button>();

            // 데이터 적용
            iconImage.sprite = data.icon;
            nameText.text    = data.itemName;
            priceText.text   = data.price.ToString();

            // 버튼 클릭 시 구매 로직
            buyButton.onClick.AddListener(() => BuyItem(data));
        }
    }

    private void BuyItem(ShopStatData data)
    {
        int money = PlayerPrefs.GetInt("Money", 0);
        if (money < data.price)
        {
            Debug.Log("돈 부족");
            return;
        }

        PlayerPrefs.SetInt("Money", money - data.price);

        // 스탯 증가 예제 (아이템 이름을 키로 사용)
        float current = PlayerPrefs.GetFloat(data.itemStatName, 0);
        PlayerPrefs.SetFloat(data.itemStatName, current + data.itemStatValue);

        Debug.Log($"{data.itemName} 구매됨, " + $"{data.itemStatName} +{data.itemStatValue}");
    }
}
