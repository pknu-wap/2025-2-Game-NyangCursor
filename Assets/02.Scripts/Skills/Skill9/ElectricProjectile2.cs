using System.Collections;
using UnityEngine;

public class ElectricProjectile2 : MonoBehaviour, IProjectile
{
    [Header("Effects & Prefabs")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject electricLinePrefab; // LineRenderer + EdgeCollider prefab
    [SerializeField] private LayerMask targetLayer;

    [Header("Settings")]
    private int electricLineCount;  // 랜덤 전류 개수
    [SerializeField] private float lineDuration = 0.5f;  // 전류 지속시간
    [SerializeField] private float lineMagnification = 20f; // 전류 길이 배율

    private float damage;
    private float duration;
    private float size;
    private Coroutine durationCoroutine;

    private void OnEnable()
    {
        if (durationCoroutine != null)
            StopCoroutine(durationCoroutine);
    }

    public void SetDamage(float dmg) => damage = dmg;

    public void SetDuration(float d)
    {
        duration = d;
        durationCoroutine = StartCoroutine(DespawnAfterDuration());
    }

    public void SetSize(float s)
    {
        size = s;
        transform.localScale = Vector3.one * size;
    }

    public void SetLightningCount(int count)
    {
        electricLineCount = count;
    }

    private IEnumerator DespawnAfterDuration()
    {
        yield return new WaitForSeconds(duration);
        Explode();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsTargetLayer(collision.gameObject))
            return;

        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
            damageable.TakeDamage(damage);

        Explode();
    }

    private bool IsTargetLayer(GameObject obj) => (targetLayer.value & (1 << obj.layer)) != 0;

    private void Explode()
    {
        // 폭발 이펙트
        PoolManager.instance.Spawn(explosionPrefab, transform.position);

        // 폭발 범위 데미지
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, size * 4f, targetLayer);
        foreach (var enemy in enemies)
        {
            IDamageable dmg = enemy.GetComponent<IDamageable>();
            if (dmg != null && !dmg.IsDead)
            {
                dmg.TakeDamage(damage);
                PoolManager.instance.Spawn(hitEffectPrefab, enemy.transform.position);
            }
        }

        // 랜덤 전류 생
        for (int i = 0; i < electricLineCount; i++)
        {
            float angle = Random.Range(0f, 360f);
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0).normalized;
            float lineLength = size * lineMagnification;
            float lineWidth = 0.2f; // collider 두께

            GameObject lineObj = PoolManager.instance.Spawn(electricLinePrefab, transform.position);

            // LineRenderer 설정 (월드 좌표 기준)
            LineRenderer lr = lineObj.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, transform.position + dir * lineLength);

            // BoxCollider2D 설정 (월드 좌표 기준)
            BoxCollider2D box = lineObj.GetComponent<BoxCollider2D>();
            if (box == null)
                box = lineObj.AddComponent<BoxCollider2D>();

            // 월드 좌표 기준 중앙 계산
            Vector2 start = transform.position;
            Vector2 end = transform.position + (Vector3)dir * lineLength;
            Vector2 center = (start + end) * 0.5f;
            float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // lineObj 위치 이동
            lineObj.transform.position = center;
            lineObj.transform.rotation = Quaternion.Euler(0, 0, angleDeg);

            // BoxCollider size & offset
            box.size = new Vector2(lineLength, lineWidth);
            box.offset = Vector2.zero; // 이미 중심 기준으로 잡았으므로 offset 필요 없음

            // ElectricLine 초기화
            ElectricLine el = lineObj.GetComponent<ElectricLine>();
            if (el != null)
                el.Init(damage / 2f, lineDuration, targetLayer, hitEffectPrefab);
        }





        // projectile 반납
        PoolManager.instance.Despawn(gameObject);
    }
}
