using UnityEngine;

[CreateAssetMenu(fileName = "TempItemActionSO", menuName = "Scriptable Objects/TempItemActionSO")]
public class TempItemActionSO : ScriptableObject, IItemAction
{
    public void Execute(Transform player)
    {
        Debug.Log("실행됨");
        // 여기에 실제 로직 구현
    }
}
