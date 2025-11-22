using UnityEngine;
using DG.Tweening;
using System;

public class CameraShakeController : MonoBehaviour
{
    [Header("Camera Root (흔들리는 부모 오브젝트)")]
    [SerializeField] private Transform shakeCameraRoot;

    [Header("Shake Settings")]
    [SerializeField] private float normalShakeStrength = 0.15f;
    [SerializeField] private float overdriveShakeStrength = 0.15f;
    [SerializeField] private float boostShakeStrength = 0.5f;

    [SerializeField] private float shakeDuration = 0.25f;

    private Vector3 originalLocalPos;

    private Tween normalShakeTween;
    private Tween boostShakeTween;

    private void Awake()
    {
        if (shakeCameraRoot == null)
        {
            Debug.LogError("[CameraShakeController] shakeCameraRoot가 연결되어 있지 않습니다!");
            return;
        }

        originalLocalPos = shakeCameraRoot.localPosition;
    }

    private void OnEnable()
    {
        PlayerHpController.OnTakeDamage += HandleTakeDamageShake;
        GaugeOverdriveLogic.OnBoostEvent += HandleBoostShake;

        // 글로벌 이벤트 버스 사용
        GameEvents.Subscribe(GameEventType.OnPlayerTakeDamage, HandleTakeReduceGaugeShake);
    }

    private void OnDisable()
    {
        PlayerHpController.OnTakeDamage -= HandleTakeDamageShake;
        GaugeOverdriveLogic.OnBoostEvent -= HandleBoostShake;

        // 글로벌 이벤트 버스 사용
        GameEvents.Unsubscribe(GameEventType.OnPlayerTakeDamage, HandleTakeReduceGaugeShake);
    }


    // ---------------------------
    // 일반 피격 (노말)
    // ---------------------------
    private void HandleTakeDamageShake(float currentHp, float maxHp)
    {
        ShakeNormal(normalShakeStrength, shakeDuration);
    }

    // ---------------------------
    // 오버드라이브 피격
    // ---------------------------
    private void HandleTakeReduceGaugeShake()
    {
        ShakeNormal(overdriveShakeStrength, shakeDuration);
    }

    // ---------------------------
    // 부스터 (강하게, Kill되지 않게)
    // ---------------------------
    private void HandleBoostShake()
    {
        ShakeBoost(boostShakeStrength, 1f);
    }


    // ====================================================
    //  일반 흔들림 (normal/overdrive)
    //  → boost 흔들림 Kill ❌
    //  → normal 흔들림만 Kill ⭕
    // ====================================================
    private void ShakeNormal(float strength, float duration)
    {
        if (shakeCameraRoot == null) return;

        // normal 흔들림만 Kill (boost는 Kill 안 함)
        normalShakeTween?.Kill();

        normalShakeTween = shakeCameraRoot
            .DOShakePosition(duration, strength, 20, 90f, false, true)
            .SetUpdate(false) //타임스케일 영향o
            .OnComplete(() =>
            {
                shakeCameraRoot.localPosition = originalLocalPos;
                normalShakeTween = null;
            });
    }

    // ====================================================
    //  부스터 흔들림
    //  → 어떤 Kill에도 영향 받지 않음
    // ====================================================
    private void ShakeBoost(float strength, float duration)
    {
        if (shakeCameraRoot == null) return;

        // boost 흔들림만 개별적으로 Kill
        boostShakeTween?.Kill();

        boostShakeTween = shakeCameraRoot
            .DOShakePosition(duration, strength, 25, 100f, false, true)
            .SetUpdate(false) //타임스케일 영향o
            .OnComplete(() =>
            {
                shakeCameraRoot.localPosition = originalLocalPos;
                boostShakeTween = null;
            });
    }
}
