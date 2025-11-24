using System.Collections;
using UnityEngine;

public class LobbyPlayerAnimator : MonoBehaviour
{

    private Animator animator;
    
    //플레이어 오브젝트 변환
    [SerializeField] private GameObject NormalObj;
    [SerializeField] private GameObject OverDriveObj;
    [SerializeField] private GameObject BoostObj;

    //플레이어 히트관련
    [Header("Hit Effect Settings")]
    [SerializeField] private Material material;
    [SerializeField] private float instantValue = 0.5f;     // 처음 즉시 줄 값
    [SerializeField] private float blinkValueA = 0.0f;       // 깜빡임 A
    [SerializeField] private float blinkValueB = 0.1f;       // 깜빡임 B
    [SerializeField] private float blinkDuration = 1f;     // 전체 깜빡임 지속 시간
    [SerializeField] private float lerpSpeed = 12f;          // 부드러운 Lerp 속도
    private Coroutine hitRoutine;
    private readonly int HitBlendID = Shader.PropertyToID("_HitEffectBlend");

    //플레이어 위로 이동관련
    [SerializeField] private float moveUpDistance = 5f;   // 위로 이동 거리
    [SerializeField] private float moveUpDuration = 0.5f;   // 이동 시간
    private Coroutine moveRoutine;

    public GameObject fadeOut;


    void Awake()
    {
        animator = GetComponent<Animator>();
        if (material != null)
            material.SetFloat(HitBlendID, 0f);

        LobbyCursorMove.OnCursorArrive += HandleRidingAnim;

    }

    void OnDestroy()
    {
        LobbyCursorMove.OnCursorArrive -= HandleRidingAnim;
    }

    private void HandleRidingAnim() //타는 애니메이션
    {
        animator.SetBool("isRiding", true);
        Invoke("HandleOverDriveAnim", 1);//1초 뒤 오버드라이브 애니메이션

    }
    private void HandleOverDriveAnim() //오버드라이브 애니메이션
    {
        animator.SetBool("isOverDrive", true);
        OverDriveObj.SetActive(true); //타는 고양이 on 
        NormalObj.SetActive(false); //메달린 고양이 off
        BoostObj.SetActive(true);// 부스터 on

        //플레이어 위로 이동
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveUpRoutine());//플레이어 위로 이동



    }


    private IEnumerator MoveUpRoutine()
    {
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = startPos + Vector3.up * moveUpDistance;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveUpDuration;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.localPosition = targetPos; // 마지막 보정

        //페이드아웃
        fadeOut.SetActive(true);

    }

    public void HandleHitAnim()
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
