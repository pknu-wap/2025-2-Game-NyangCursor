using UnityEngine;

public class SelectedChararcterDataManager : MonoBehaviour
{
    public static SelectedChararcterDataManager instance = null;

    [HideInInspector] // 인위적 조작 방지를 위한 하이드 인 인스펙터
    public UpgradeOptionSO selectedStartData;

    public CharacterDataSO characterDataSO;


    
    void Awake()
    {
        if (instance == null) 
            instance = this; 
        else if (instance != this) 
            Destroy(gameObject);
            
        DontDestroyOnLoad(gameObject); 
    }
}
