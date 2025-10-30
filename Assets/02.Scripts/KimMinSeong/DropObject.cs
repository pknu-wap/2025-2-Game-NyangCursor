using UnityEngine;

public abstract class DropObject
{
    public string objectName;

    // 드랍 실행 (각 타입별로 구현)
    public abstract void ExecuteDrop(Vector3 position);
}