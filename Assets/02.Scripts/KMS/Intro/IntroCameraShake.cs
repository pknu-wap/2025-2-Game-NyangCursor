using UnityEngine;
using DG.Tweening;

public class IntroCameraShakeController : MonoBehaviour
{
    [Header("Camera Root (흔들릴 실제 Transform)")]
    [SerializeField] private Transform shakeCameraRoot;

    [Header("Shake Settings")]
    [SerializeField] private float normalShakeStrength = 0.15f;
    [SerializeField] private float boostShakeStrength = 0.5f;
    [SerializeField] private float shakeDuration = 0.25f;

    private Vector3 originalLocalPos;

    private Tween normalShakeTween;
    private Tween boostShakeTween;

    private void Awake()
    {
        if (shakeCameraRoot == null)
        {
            Debug.LogError("[IntroCameraShakeController] shakeCameraRoot가 없습니다!");
            return;
        }

        originalLocalPos = shakeCameraRoot.localPosition;
    }

    // ===========================================================
    // 🔥 외부에서 이 함수만 호출하면 흔들림 발생
    // ===========================================================
    public void ShakeNormal()
    {
        ShakeNormal_Internal(normalShakeStrength, shakeDuration);
    }

    public void ShakeBoost()
    {
        ShakeBoost_Internal(boostShakeStrength, shakeDuration);
    }

    // ===========================================================
    // Normal Shake (Boost를 방해하지 않음)
    // ===========================================================
    private void ShakeNormal_Internal(float strength, float duration)
    {
        if (shakeCameraRoot == null) return;

        // normal만 개별 kill
        normalShakeTween?.Kill();

        normalShakeTween = shakeCameraRoot
            .DOShakePosition(duration, strength, 20, 90f, false, true)
            .SetUpdate(false)
            .OnComplete(() =>
            {
                shakeCameraRoot.localPosition = originalLocalPos;
                normalShakeTween = null;
            });
    }

    // ===========================================================
    // Boost Shake (어떤 흔들림과도 간섭 없이 독립!)
    // ===========================================================
    private void ShakeBoost_Internal(float strength, float duration)
    {
        if (shakeCameraRoot == null) return;

        boostShakeTween?.Kill();

        boostShakeTween = shakeCameraRoot
            .DOShakePosition(duration, strength, 25, 100f, false, true)
            .SetUpdate(false)
            .OnComplete(() =>
            {
                shakeCameraRoot.localPosition = originalLocalPos;
                boostShakeTween = null;
            });
    }
}

