using UnityEngine;

public class TempPlayerprefsClear : MonoBehaviour
{
    public void ClearAllPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("초기화");
    }
}
