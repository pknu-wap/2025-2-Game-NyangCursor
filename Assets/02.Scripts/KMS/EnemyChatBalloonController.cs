using System;
using UnityEngine;
using DG.Tweening;

public class EnemyChatBalloonController : MonoBehaviour
{
    public GameObject chatBalloon;

    [Header("Settings")]
    public float delayTime = 0.5f; // 말풍선 등장 전 딜레이
    public float popScale = 1.3f;
    public float popDuration = 0.20f;
    public float fadeDuration = 0.35f;

    private Sequence popTween;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = chatBalloon.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = chatBalloon.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        GaugeRidingLogic.OnRidingEvent += HandleShowBalloon;

        chatBalloon.SetActive(false);
        ResetBalloon();
    }

    private void OnDisable()
    {
        GaugeRidingLogic.OnRidingEvent -= HandleShowBalloon;

        if (popTween != null && popTween.IsActive())
            popTween.Kill();

        chatBalloon.SetActive(false);
        ResetBalloon();
    }

    void ResetBalloon()
    {
        chatBalloon.transform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
    }

    private void HandleShowBalloon()
    {
        ShowBalloonEffect();
    }

    private void ShowBalloonEffect()
    {
        if (popTween != null && popTween.IsActive())
            popTween.Kill();

        ResetBalloon();
        chatBalloon.SetActive(false); //  딜레이 중 표시 안 되도록

        popTween = DOTween.Sequence()
            .AppendInterval(delayTime)
            .AppendCallback(() =>
            {
                chatBalloon.SetActive(true); // 딜레이 완료 후 켜기
            })
            .Append(canvasGroup.DOFade(1f, 0f)) // 바로 보이도록 알파 설정
            .Append(chatBalloon.transform
                .DOScale(popScale, popDuration)
                .SetEase(Ease.OutBack))
            .Append(canvasGroup
                .DOFade(0f, fadeDuration)
                .SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                chatBalloon.SetActive(false);
                ResetBalloon();
            });
    }
}
