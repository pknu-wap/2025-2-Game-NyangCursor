using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class CursorMove : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] Vector3 ridingOffset = Vector3.zero; // 오프셋 (0,0,0이면 플레이어 위치와 동일)
    [SerializeField] private Vector3 baseOffset = new Vector3(0.6f, 0, 0);

    [SerializeField] private float followSmooth = 0.15f; // 부드럽게 따라가는 정도
    private Vector3 velocity = Vector3.zero;



    [SerializeField] SpriteRenderer overRideCursorSpriteRenderer;
    [SerializeField] SpriteRenderer normalCursorRenderer;

    private void Awake()
    {
        GaugeRidingLogic.OnRidingEvent += HandleRidingMotion; // E키를 눌렀을때 호출됨
        GaugeOverdriveLogic.OnGetOffEvent += HandleGetOffMotion;//게이지가 0이 되었을때 호출됨
    }

    private void OnDestroy()
    {
        GaugeRidingLogic.OnRidingEvent -= HandleRidingMotion;
        GaugeOverdriveLogic.OnGetOffEvent += HandleGetOffMotion;
    }

    void Update()
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerStateLogic.PlayerState.Normal)
            return;

        // 플레이어 위치
        Vector3 playerPos = player.transform.position;

        // 플레이어의 좌우 반전(localScale.x) 확인
        float dir = Mathf.Sign(player.transform.localScale.x); // -1 또는 1

        // 좌우에 따라 offset.x 반전
        Vector3 targetOffset = baseOffset;
        targetOffset.x *= dir;

        // 최종 목표 위치
        Vector3 targetPos = playerPos + targetOffset;

        // 부드럽게 따라가기
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, followSmooth);
    }

    private void HandleRidingMotion() //라이딩 마우스 모션
    {
        // 목표 지점 = 플레이어 위치 + 오프셋
        Vector3 target = player.transform.position + ridingOffset;
        StartCoroutine(MoveToTargetForRiding(target, 0.5f)); // 0.5초 동안 이동
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
        overRideCursorSpriteRenderer.enabled = true;
        normalCursorRenderer.enabled = false;   
        
    }
    private void HandleGetOffMotion() // 내리기 마우스 모션 및 플레이어 회전 복구
    {
        // 1) 커서를 즉시 baseOffset 위치로 이동
        transform.position = player.transform.position + baseOffset;

        // 2) 커서 회전을 즉시 (0,0,0)으로 리셋
        transform.rotation = Quaternion.Euler(Vector3.zero);

        // 3) 커서 스프라이트 전환
        overRideCursorSpriteRenderer.enabled = false;
        normalCursorRenderer.enabled = true;

        // 4) 플레이어 회전을 1초 동안 0으로 복구
        StartCoroutine(RestorePlayerRotation());
    }

    private IEnumerator RestorePlayerRotation()
    {
        Quaternion startRot = player.transform.rotation;
        Quaternion targetRot = Quaternion.identity; // (0,0,0) 회전
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            player.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        // 최종 보정
        player.transform.rotation = targetRot;
    }





}
