using UnityEngine;

public class TempPlayerprefsClear : MonoBehaviour
{
    public void ClearAllPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        PlayerPrefs.SetInt("Money", 100);
        Debug.Log("초기화, 돈 100원");
    }
}
