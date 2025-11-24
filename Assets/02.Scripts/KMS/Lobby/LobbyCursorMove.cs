using System;
using System.Collections;
using UnityEngine;

public class LobbyCursorMove : MonoBehaviour
{
    public GameObject player;

    public Vector3 ridingOffset = Vector3.zero;

    public static event Action OnCursorArrive;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        CharacterSelectManager.OnLobbyRidingStart += HandleRidingMotion;
    }

    private void OnDisable()
    {
        CharacterSelectManager.OnLobbyRidingStart -= HandleRidingMotion;
    }


    private void HandleRidingMotion() //라이딩 마우스 모션
    {
        // 목표 지점 = 플레이어 위치 + 오프셋
        Vector3 target = player.transform.position + ridingOffset;
        StartCoroutine(MoveToTargetForRiding(target, 0.75f)); // 0.5초 동안 이동
    }

    private IEnumerator MoveToTargetForRiding(Vector3 target, float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(Vector3.zero); // (0,0,0) 회전

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // 위치 보간
            transform.position = Vector3.Lerp(startPos, target, t);

            // 회전 보간
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        // 최종 보정
        transform.position = target;
        transform.rotation = targetRot;

        OnCursorArrive?.Invoke();
        
        //overRideCursorSpriteRenderer.enabled = true;
        //normalCursorRenderer.enabled = false;

    }
}
