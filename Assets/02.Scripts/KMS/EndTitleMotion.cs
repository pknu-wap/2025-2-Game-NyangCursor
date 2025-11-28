using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class EndTitleMotion : MonoBehaviour
{
    [Header("UI 요소들")]
    [SerializeField] private RectTransform gameOverRT;
    [SerializeField] private CanvasGroup surviveTitleCG;
    [SerializeField] private CanvasGroup cheeseCG;
    [SerializeField] private CanvasGroup button1CG;
    [SerializeField] private CanvasGroup button2CG;

    [Header("설정")]
    [SerializeField] private float gameOverMoveDuration = 0.5f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float cheeseDelay = 0.3f;
    [SerializeField] private float buttonsDelay = 0.3f;

    [SerializeField] private float bounceStrength = 1.7f;

    private void Start()
    {
        PlayMotion();
    }

    private void PlayMotion()
    {
        surviveTitleCG.alpha = 0f;
        cheeseCG.alpha = 0f;
        button1CG.alpha = 0f;
        button2CG.alpha = 0f;

        Sequence seq = DOTween.Sequence();

        // 1️⃣ 게임오버 이미지 통통 튀면서 내려오기 (TimeScale 영향 X)
        seq.Append(gameOverRT.DOAnchorPosY(73f, gameOverMoveDuration)
            .From(new Vector2(gameOverRT.anchoredPosition.x, 200f))
            .SetEase(Ease.OutBack, bounceStrength)
            .SetUpdate(true));

        // 2️⃣ 생존 타이틀 페이드인
        seq.Append(surviveTitleCG.DOFade(1f, fadeDuration).SetUpdate(true));

        // 3️⃣ 치즈 텍스트 페이드인
        seq.AppendInterval(cheeseDelay).SetUpdate(true);
        seq.Append(cheeseCG.DOFade(1f, fadeDuration).SetUpdate(true));

        // 4️⃣ 버튼 두 개 동시에 페이드인
        seq.AppendInterval(buttonsDelay).SetUpdate(true);
        seq.Append(button1CG.DOFade(1f, fadeDuration).SetUpdate(true));
        seq.Join(button2CG.DOFade(1f, fadeDuration).SetUpdate(true));

        seq.SetUpdate(true);  // ★ 시퀀스 전체를 TimeScale 0 무시 모드로 설정
    }
}
