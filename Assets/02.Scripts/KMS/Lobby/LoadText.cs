using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadText : MonoBehaviour
{
    Text loadtext;
    int rand;
    void Start()
    {
        loadtext = GetComponent<Text>();
        rand = Random.Range(1, 8);
        switch (rand)
        {
            case 1:
                loadtext.text = "쥐 군단 잡으로 가는중..";
                break;
            case 2:
                loadtext.text = "e키를 눌러서 커서에 탑승 할 수 있습니다";
                break;
            case 3:
                loadtext.text = "커서에서 내릴때 구멍을 조심하세요!";
                break;
        }

    }


}
