using UnityEngine;

// 아이템을 기획 후 로직을 작성할 때 이 스크립트에 execute 함수를 오버라이드
[CreateAssetMenu(fileName = "TempItemActionSO", menuName = "Scriptable Objects/TempItemActionSO")]
public class TempItemActionSO : ItemSO
{
    public override void Execute(Transform player)
    {
        Debug.Log($"{itemName} 실행됨");
        // 여기에 행동 로직 직접 작성
    }
}
