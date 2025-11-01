using System;
using UnityEngine;

public class ExpManager : MonoBehaviour
{
    private int currentLevel = 1;  // 현재 플레이어 레벨
    private int currentExp = 0; // 현재 경험치량

    private int requiredExp; // 레벨업을 하기 위해 필요한 경험치량
    [SerializeField] private int baseExp = 100; // 초기 경험치량
    [SerializeField] private int expGrowth = 100; // 레벨에 따른 필요 경험치 증가량

    // 추후에 플레이어 스탯이나 아이템에서 경험치 증가율을 변경할 수 있도록 수정할 예정. 현재는 Inspector 할당으로 구현
    [SerializeField] [Range(0f, 1f)] private float expGainBonus = 0f;   // 보너스 경험치 증가율

    public static event Action<int> OnLevelUp;  // 레벨업 발생시 발행하는 이벤트
    public static event Action<int, int, float> OnExpChanged; // 경험치 변경시 발행하는 이벤트 

    public float ExpRatio => Mathf.Clamp01((float)currentExp / requiredExp); // 경험치바 UI 에서 사용할 비율

    private void Awake()
    {
        requiredExp = CalculateRequiredExp();
    }

    private void OnEnable()
    {
        ExpDropObject.OnExpCollected += AddExp;
    }
    private void OnDisable()
    {
        ExpDropObject.OnExpCollected -= AddExp;
    }

    // 경험치를 먹었을 때 실행할 콜백 함수
    private void AddExp(int expAmount)
    {
        // 디버그용
        Debug.Log("경험치 업데이트!");

        // 현재 경험치량을 획득한 경험치량 + 보너스 증가량만큼 증가시킴
        currentExp += expAmount + Mathf.RoundToInt(expAmount * expGainBonus);

        // UI 업데이트 등등의 작업을 하기 위해 이벤트 발행
        OnExpChanged?.Invoke(currentExp, requiredExp, ExpRatio);

        // 레벨업을 할 수 있다면 레벨업
        if (currentExp >= requiredExp)
            LevelUp();
    }

    private void LevelUp()
    {
        // 디버그용
        Debug.Log("레벨업!");

        // 1. 레벨업 갱신 및 이벤트 발행
        currentLevel += 1;
        OnLevelUp?.Invoke(currentLevel);

        // 2. 증강 페이즈 전환
        StageFlowManager.instance.SetStateToAugment();

        // 3. 경험치 갱신
        currentExp -= requiredExp;   // 리셋을 하되 이전 레벨에서 초과된 경험치는 반영되도록 설정
        requiredExp = CalculateRequiredExp();   // 필요 경험치량 갱신

        // 4. 경험치 변경 이벤트 발행
        OnExpChanged?.Invoke(currentExp, requiredExp, ExpRatio);    // UI 업데이트 등등의 작업을 하기 위해 이벤트 발행
    }

    // 레벨업을 하기 위해 필요한 경험치량을 계산하는 함수
    // 현재는 레벨업마다 선형적으로 증가함
    private int CalculateRequiredExp()
    {
        // 1레벨부터 시작하므로 (level - 1) 를 사용
        return baseExp + (currentLevel - 1) * expGrowth;
    }
}
