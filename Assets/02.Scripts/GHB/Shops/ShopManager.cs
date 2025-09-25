using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject shopUI;
    [SerializeField] private TMP_Text currentMoneyText;

    [Header("Setup")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private List<ShopStatData> shopItems = new List<ShopStatData>();
    

    [Header("Feedback")]
    private Color normalMoneyColor;
    [SerializeField] private Color insufficientMoneyColor = Color.red;
    [SerializeField] private float blinkDuration = 0.5f;

    private void Start()
    {
        // 임시로 돈 충전
        PlayerPrefs.SetInt("Money", 100);
        normalMoneyColor = currentMoneyText.color;
        UpdateMoneyUI();
        InitializateShop();
    }

    private void InitializateShop()
    {
        foreach (var data in shopItems)
        {
            GameObject item = Instantiate(itemPrefab, contentParent);

            Image iconImage = item.transform.Find("Icon").GetComponent<Image>();
            TMP_Text nameText = item.transform.Find("NameText").GetComponent<TMP_Text>();
            TMP_Text priceText = item.transform.Find("PriceText").GetComponent<TMP_Text>();
            Button buyButton = item.transform.Find("BuyButton").GetComponent<Button>();

            iconImage.sprite = data.icon;
            nameText.text = data.itemName;
            priceText.text = data.price.ToString();

            buyButton.onClick.AddListener(() => BuyItem(data));
        }
    }

    private void BuyItem(ShopStatData data)
    {
        int money = PlayerPrefs.GetInt("Money", 0);

        if (money < data.price)
        {
            Debug.Log("돈 부족");
            StartCoroutine(BlinkMoneyText());
            return;
        }

        PlayerPrefs.SetInt("Money", money - data.price);

        float current = PlayerPrefs.GetFloat(data.itemStatName, 0);
        PlayerPrefs.SetFloat(data.itemStatName, current + data.itemStatValue);

        Debug.Log($"{data.itemName} 구매됨, {data.itemStatName} +{data.itemStatValue}");

        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        int money = PlayerPrefs.GetInt("Money", 0);
        currentMoneyText.text = $"G : {money}";
        currentMoneyText.color = normalMoneyColor;
    }

    private IEnumerator BlinkMoneyText()
    {
        currentMoneyText.color = insufficientMoneyColor;
        yield return new WaitForSeconds(blinkDuration);
        currentMoneyText.color = normalMoneyColor;
    }

    // 상점 열기 버튼용 함수
    public void OpenShop()
    {
        if (shopUI != null)
        {
            shopUI.SetActive(true);
            UpdateMoneyUI();
        }
    }

    void Update()
{
    // 닫기
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        shopUI.SetActive(false);
    }

    // 임시로 Playerprefs 값들 출력
    if (Input.GetKeyDown(KeyCode.K))
    {
        // money 값 출력
        int money = PlayerPrefs.GetInt("Money", 0);
        Debug.Log($"Money : {money}");

        // shopItems 모든 스탯 값 출력
        foreach (var item in shopItems)
        {
            float val = PlayerPrefs.GetFloat(item.itemStatName, 0f);
            Debug.Log($"{item.itemStatName} : {val}");
        }
    }
}

}
