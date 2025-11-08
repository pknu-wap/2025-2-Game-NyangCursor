using System;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    private int savedGold;  // 이전까지 저장한 골드
    private int earnedGold = 0; // 이번 스테이지에서 얻은 골드
    [SerializeField][Range(0f, 1f)] private float goldGainBonus = 0f;   // 보너스 골드 증가율

    // 참조 방식을 사용하는 이유는 GoldManaer 와 GoldUIManager 오브젝트의 생성이 비동기적으로 일어나므로
    // 이벤트 방식을 사용하면 초기 UI 반영이 성공적으로 실행되는 것을 보장할 수 없기 때문임
    [SerializeField] private GoldUIManager goldUIManager;

    // 외부에서 접근할 수 있도록 프로퍼티 사용
    // 현재는 StageEndManager 에서 사용중
    public int SavedGold => savedGold;
    public int EarnedGold => earnedGold;

    private void Awake()
    {
        // 골드는 PlayerPrefs 에 저장되므로 이를 가져옴
        savedGold = PlayerPrefs.GetInt("Money");

        // 디버그용
        Debug.Log($"지금까지 모은 골드: {savedGold}");
    }

    private void OnEnable()
    {
        GoldDropObject.OnGoldCollected += AddGold;
        goldUIManager.UpdateGoldUI(savedGold, earnedGold);    // 시작했을때 초기 UI 업데이트
    }

    private void OnDisable()
    {
        GoldDropObject.OnGoldCollected -= AddGold;
    }

    private void Update()
    {
        // 테스트용 savedGold, earnedGold 초기화 코드
        // PlayerPrefs 를 초기화함
        if (Input.GetKeyDown(KeyCode.G))
        {
            PlayerPrefs.DeleteKey("Money");
            savedGold = PlayerPrefs.GetInt("Money");
            earnedGold = 0;
            goldUIManager.UpdateGoldUI(savedGold, earnedGold);
            Debug.Log("savedGold, earnedGold 초기화!");
        }
    }

    private void AddGold(int goldAmount)
    {
        // 업데이트된 골드값은 임시로 earnedGold 에 저장하고
        // PlayerPrefs 반영은 스테이지 종료시 StageEndManager 가 한꺼번에 처리하도록 구현하였음 (Lazy Write)

        earnedGold += goldAmount + Mathf.RoundToInt(goldAmount * goldGainBonus);   // 현재 골드량을 획득한 골드량 + 보너스 증가량만큼 증가시킴
        goldUIManager.UpdateGoldUI(savedGold, earnedGold);    // UI 업데이트
    }
}
