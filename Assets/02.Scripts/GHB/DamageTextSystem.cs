using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

[System.Serializable]
public struct DamageColorRule
{
    public float minDamage;
    public float maxDamage;
    public Color color;
}

public class DamageTextSystem : MonoBehaviour
{
    [Header("데미지 텍스트 프리팹 & Canvas")]
    [SerializeField] private GameObject damageTextPrefab;

    [Header("데미지 색상 규칙")]
    [SerializeField] private List<DamageColorRule> colorRules = new();

    private void Awake()
    {
        // 글로벌 이벤트 버스 사용
        GameEvents.Subscribe<(float, Transform)>(GameEventType.OnEnemyTakeDamage, OnDamage);
    }

    private void OnDestroy()
    {
        // 글로벌 이벤트 버스 사용
        GameEvents.Unsubscribe<(float, Transform)>(GameEventType.OnEnemyTakeDamage, OnDamage);
    }

    private void OnDamage((float damage, Transform source) data)
    {
        SpawnAndAnimateDamageText(data.damage, data.source);
    }

    private void SpawnAndAnimateDamageText(float damage, Transform source)
    {
        Vector3 spawnPos = source.position;
        // Pool에서 스폰
        GameObject obj = PoolManager.instance.Spawn(damageTextPrefab, spawnPos);

        TextMeshProUGUI textMesh = obj.GetComponentInChildren<TextMeshProUGUI>();
        // 적의 좌우 반전을 보정
        Vector3 localScale = obj.transform.localScale;
        localScale.x = Mathf.Abs(localScale.x);
        // X축 양수 고정
        obj.transform.localScale = localScale;

        // Tween 초기화
        textMesh.DOKill();
        obj.transform.DOKill();
        // 색상 적용
        Color c = GetDamageColor(damage); c.a = 1f;
        textMesh.color = c;
        textMesh.text = damage.ToString("0");
        // 이동 연출
        Vector3 start = obj.transform.position;
        Vector3 end = start + Vector3.up * 1f;
        Sequence seq = DOTween.Sequence();

        seq.Append(obj.transform.DOMove(end, 0.8f));
        seq.Join(textMesh.DOFade(0, 0.8f));
        seq.OnComplete(() =>
        {
            PoolManager.instance.Despawn(obj);
        });
    }



    private Color GetDamageColor(float damage)
    {
        foreach (var rule in colorRules)
        {
            if (damage >= rule.minDamage && damage <= rule.maxDamage)
                return rule.color;
        }
        return Color.white; // 기본 색상
    }
}
