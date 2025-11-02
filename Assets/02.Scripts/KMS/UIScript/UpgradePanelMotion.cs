using System.Collections.Generic;
using UnityEngine;

public class UpgradePanelMotion : MonoBehaviour
{
    public Animator animator;
    public List<GameObject> slotList = new();

    private void OnEnable()
    {
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;


        // 부모가 켜질 때 애니메이션 실행
        animator.SetTrigger("Show");

        // 모든 자식 슬롯 활성화
        for (int i = 0; i < slotList.Count; i++)
        {
            if (slotList[i] != null)
                slotList[i].SetActive(true);
        }
    }

    private void OnDisable()
    {
        // 부모가 꺼질 때 자식 슬롯도 같이 비활성화
        for (int i = 0; i < slotList.Count; i++)
        {
            if (slotList[i] != null)
                slotList[i].SetActive(false);
        }
    }
}
