using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataSO", menuName = "Scriptable Objects/CharacterDataSO")]
public class CharacterDataSO : ScriptableObject
{
    [Header("캐릭터 기본 정보")]
    public string characterName;
    public Sprite characterImage;

    [TextArea(2, 5)]
    [Tooltip("로비에서 커서를 올렸을 때 표시될 캐릭터 설명")]
    public string characterDescription;

    [Header("캐릭터 시작 기본 스킬")]
    public UpgradeOptionSO startingSkill;

    // [Header("캐릭터 패시브 능력 (추후 구현 예정)")]
}
