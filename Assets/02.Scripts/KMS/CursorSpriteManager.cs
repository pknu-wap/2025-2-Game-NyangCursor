using UnityEngine;

public class CursorSpriteManager : MonoBehaviour
{
    public SpriteRenderer cursorSprite;

    public SpriteRenderer ridingCursorSprite;
    void Awake()
    {     
        var selectedDataManager = SelectedChararcterDataManager.instance;
        if (selectedDataManager == null || selectedDataManager.selectedStartData == null)
            return;

      cursorSprite.sprite =  selectedDataManager.characterDataSO.characterImage; 
        ridingCursorSprite.sprite = selectedDataManager.characterDataSO.characterImage; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
