using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSkillLogic : MonoBehaviour
{
    [Header("공격 대상 레이어")]
    [SerializeField] private LayerMask targetLayer;

    private float damage;
    private float speed;
    private float range;

     private Transform player;

    private readonly HashSet<IDamageable> overlappingTargets = new();
    private Coroutine damageLoop;

    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponentInChildren<ParticleSystem>();
    }

    private void OnEnable()
    {
        // FireCircleSkill에서 생성될 때 자동으로 파티클 재생
        if (ps != null)
            ps.Play();


    }
    
    private void Update()
{
    if (player != null)
        transform.position = player.position;  // 위치만 따라감
}

    public void Initialize(float damage, float speed, float range,Transform transform)
    {
        this.damage = damage;
        this.speed = speed;
        this.range = range;
        this.player = transform;
       

       Debug.Log("데미지: " + damage + "스피드 : " + speed + "범위 :" + range);
        // 회전 속도 조정
        //AdjustParticleRotationSpeed();

        // 범위 확장 시작
        //StartCoroutine(ExpandRange());

        // 공격 루프 시작
        if (damageLoop == null)
            damageLoop = StartCoroutine(DamageOverTime());
    }

private void AdjustParticleRotationSpeed()
{
    if (ps == null) return;

    var rotationBySpeed = ps.rotationBySpeed;
    rotationBySpeed.enabled = true;

    // speed = 1 → 10도/초,  speed = 2 → 200도/초
    float angularVelocityDeg = Mathf.Lerp(10f, 200f, Mathf.Clamp01((speed - 1f) / 1f));

    // ParticleSystem은 내부적으로 radians 단위 사용하므로 변환
    float angularVelocityRad = angularVelocityDeg * Mathf.Deg2Rad;

    rotationBySpeed.z = new ParticleSystem.MinMaxCurve(angularVelocityRad);

    Debug.Log($"[CircleSkillLogic] speed:{speed:F2} → angularVelocity:{angularVelocityDeg:F1}°/s ({angularVelocityRad:F3} rad/s)");
}

    private IEnumerator ExpandRange()
    {
        Vector3 baseScale = new Vector3(0.5f, 0.5f, 1f);
        Vector3 targetScale = baseScale * range;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 0.5f; // 확장 속도
            transform.localScale = Vector3.Lerp(baseScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private IEnumerator DamageOverTime()
    {
        // speed가 높을수록 주기 짧아짐 (speed=1 → 1초, speed=1.5 → 0.66초)
        float baseInterval = 1f;
        float interval = Mathf.Max(0.1f, baseInterval / speed);

        while (true)
        {
            var targets = new List<IDamageable>(overlappingTargets);
            foreach (var target in targets)
            {
                if (target != null && !target.IsDead)
                    target.TakeDamage(damage);
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsTargetLayer(collision.gameObject))
            return;

        var damageable = collision.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
        {
            overlappingTargets.Add(damageable);
            damageable.TakeDamage(damage); // 첫 타격 즉시
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
            overlappingTargets.Remove(damageable);
    }

    private bool IsTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }

    private void OnDestroy()
    {
        if (damageLoop != null)
            StopCoroutine(damageLoop);

        overlappingTargets.Clear();
    }
}
