using System;
using UnityEngine;

[Serializable]
public class SkillSlot
{
    public string skillName;
    public GameObject skillManagerObject;
    [HideInInspector] public bool isUnlocked = false;
    [HideInInspector] public Sprite icon;
}
