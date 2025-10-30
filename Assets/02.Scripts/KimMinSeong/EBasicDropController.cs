using UnityEngine;

public class EBasicDropController : MonoBehaviour, IDroppable
{
    private Enemy owner;

    public void Initialize()
    {
        // Enemy 타입만 허용
        if (owner is not Enemy enemy)
        {
            Debug.LogError($"EBasicHpController 는 Enemy 타입만 지원합니다. 현재 타입: {owner.GetType().Name}");
            return;
        }


    }

    public void Cleanup()
    {

    }

    public void Drop()
    {

    }
}
