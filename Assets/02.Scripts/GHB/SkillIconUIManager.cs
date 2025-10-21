using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillIconUIManager : MonoBehaviour
{
    public static SkillIconUIManager Instance { get; private set; }

    [SerializeField] private Transform iconParent;  // SkillIconPanel의 transform
    [SerializeField] private GameObject iconPrefab; // SkillIcon 프리팹

    private readonly List<Image> activeIcons = new List<Image>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // 스킬 아이콘 추가 (획득)
    public void AddSkillIcon(string skillName, Sprite skillSprite)
    {
        // 중복 방지: 같은 이름의 아이콘이 이미 있으면 리턴
        if (activeIcons.Exists(i => i.name == skillName)) return;

        GameObject iconObj = Instantiate(iconPrefab, iconParent);
        iconObj.name = skillName;

        Image img = iconObj.GetComponent<Image>();
        img.sprite = skillSprite;
        img.color = Color.white;
        img.raycastTarget = false;

        activeIcons.Add(img);
        RefreshOrder();
    }

    // 스킬 아이콘 제거 (비해금 시)
    public void RemoveSkillIcon(string skillName)
    {
        var target = activeIcons.Find(i => i.name == skillName);
        if (target == null) return;

        activeIcons.Remove(target);
        Destroy(target.gameObject);
        RefreshOrder();
    }

    // 오른쪽 정렬 상태로 순서 유지 (LayoutGroup이 있으면 sibling index로 제어)
    private void RefreshOrder()
    {
        // LayoutGroup이 Right-anchored 상태라면, sibling index를 0..n-1로 설정해도
        // 오른쪽 정렬을 유지합니다. (HorizontalLayoutGroup의 ChildAlignment = UpperRight)
        for (int i = 0; i < activeIcons.Count; i++)
        {
            activeIcons[i].transform.SetSiblingIndex(i);
        }
    }
}
