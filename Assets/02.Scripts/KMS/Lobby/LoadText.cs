using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LoadText : MonoBehaviour
{
    TextMeshProUGUI loadtext;
    int rand;
    void Start()
    {
        loadtext = GetComponent<TextMeshProUGUI>();
        rand = Random.Range(1, 2);
        switch (rand)
        {
            case 1:
                loadtext.text = "쥐 군단 잡으로 가는중..";
                break;
            case 2:
                loadtext.text = "쥐 군단 잡으로 가는중..";
                break;
            case 3:
                loadtext.text = "커서에서 내릴때 구멍을 조심하세요!";
                break;
        }

    }


}
