using System.Collections;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{

     private Animator animator;

    [SerializeField] private GameObject NormalObj;
    [SerializeField] private GameObject OverDriveObj;
    [SerializeField] private GameObject boost;

    [Header("Hit Effect Settings")]
    [SerializeField] private Material material;
    [SerializeField] private float instantValue = 0.5f;     // 처음 즉시 줄 값
    [SerializeField] private float blinkValueA = 0.0f;       // 깜빡임 A
    [SerializeField] private float blinkValueB = 0.1f;       // 깜빡임 B
    [SerializeField] private float blinkDuration = 1f;     // 전체 깜빡임 지속 시간
    [SerializeField] private float lerpSpeed = 12f;          // 부드러운 Lerp 속도

    private Coroutine hitRoutine;

    private readonly int HitBlendID = Shader.PropertyToID("_HitEffectBlend");


    void Awake()
    {
        animator = GetComponent<Animator>();
        GaugeRidingLogic.OnRidingEvent += HandleRidingAnim;
        GaugeOverdriveLogic.OnGetOffEvent += HandleGetOffAnim;
        GaugeRidingLogic.OnOverDriveEvent += HandleOverDriveAnim;
        NormalModController.OnWalk += HandleWalkAnim;

        PlayerHpController.OnTakeDamage += HandleHitAnim;

        if (material != null)
            material.SetFloat(HitBlendID, 0f);


    }

    void OnDestroy()
    {
        GaugeRidingLogic.OnRidingEvent -= HandleRidingAnim;
        GaugeOverdriveLogic.OnGetOffEvent -= HandleGetOffAnim;
        GaugeRidingLogic.OnOverDriveEvent -= HandleOverDriveAnim;
        NormalModController.OnWalk -= HandleWalkAnim;

        PlayerHpController.OnTakeDamage -= HandleHitAnim;
    }

    //애니메이션 조건 

    private void HandleWalkAnim(bool isWalk) //걷는애니메이션 
    {
        animator.SetBool("isWalk", isWalk);
    }
    private void HandleRidingAnim() //타는 애니메이션
    {
        animator.SetBool("isRiding", true);
    }
    private void HandleOverDriveAnim() //오버드라이브 애니메이션
    {
        animator.SetBool("isOverDrive", true);
        OverDriveObj.SetActive(true);
        NormalObj.SetActive(false);
    }

    private void HandleGetOffAnim() //떨어지는 애니메이션
    {
        animator.SetBool("isRiding", false);
        animator.SetBool("isOverDrive", false);
        animator.SetBool("isWalk", false);
        NormalObj.SetActive(true);
        OverDriveObj.SetActive(false);
    }

    public void HandleHitAnim(float a, float b)
    {
        animator.SetTrigger("isHit");

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        hitRoutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        if (material == null)
            yield break;

        // 1) 즉시 0.5 설정
        material.SetFloat(HitBlendID, instantValue);

        int blinkCount = 3;            // 총 3번 깜빡임
        float totalDuration = blinkDuration;  // 1초
        float interval = totalDuration / blinkCount / 2f;
        // 예: 1초 / 3 / 2 = 0.166초마다 한쪽으로 깜빡

        // 2) 3회 깜빡
        for (int i = 0; i < blinkCount; i++)
        {
            // ↓↓↓ 0으로
            yield return LerpHitValue(blinkValueA, interval);

            // ↑↑↑ 0.1로
            yield return LerpHitValue(blinkValueB, interval);
        }

        // 3) 마지막에 자연스럽게 0으로 복귀
        yield return LerpHitValue(0f, 0.2f);

        hitRoutine = null;
    }

    private IEnumerator LerpHitValue(float target, float duration)
    {
        float start = material.GetFloat(HitBlendID);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float v = Mathf.Lerp(start, target, t);
            material.SetFloat(HitBlendID, v);

            yield return null;
        }

        material.SetFloat(HitBlendID, target);
    }




}
