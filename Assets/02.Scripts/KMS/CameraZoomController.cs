using UnityEngine;
using System.Collections;

public class CameraZoomController : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Camera mainCam;

    [Header("Zoom Settings")]
    [SerializeField] private float defaultSize = 7f;
    [SerializeField] private float boostZoomSize = 8.5f;
    [SerializeField] private float zoomOutTime = 0.12f;
    [SerializeField] private float zoomInTime = 0.35f;

    [Header("Normal Reset Zoom")]
    [SerializeField] private float normalResetSize = 6.5f;      // 🔥 NormalEvent 때 줄 Size
    [SerializeField] private float normalZoomTime = 0.25f;    // 줌인 속도

    private Coroutine zoomRoutine;

    private void Awake()
    {
        if (mainCam == null)
            mainCam = Camera.main;
    }

    private void OnEnable()
    {
        GaugeOverdriveLogic.OnBoostEvent += HandleBoostZoom;
        GaugeOverdriveLogic.OnNormalEvent += HandleResetZoom;  //추가
        //PlayerSkill5.OnWaterBeam += HandleSkill5Zoom;
    }

    private void OnDisable()
    {
        GaugeOverdriveLogic.OnBoostEvent -= HandleBoostZoom;
        GaugeOverdriveLogic.OnNormalEvent -= HandleResetZoom;  //  추가
       // PlayerSkill5.OnWaterBeam -= HandleSkill5Zoom;
    }

    // -------------------------------
    //  오버드라이브 → Boost Zoom
    // -------------------------------
    private void HandleBoostZoom()
    {
        if (zoomRoutine != null)
            StopCoroutine(zoomRoutine);

        zoomRoutine = StartCoroutine(BoostZoomRoutine());
    }

    private IEnumerator BoostZoomRoutine()
    {
        // 1) 빠른 줌아웃
        yield return StartCoroutine(LerpCameraSize(mainCam.orthographicSize, boostZoomSize, zoomOutTime));

        // 2) 유지
        yield return new WaitForSeconds(1f);

        // 3) 기본값으로 복귀
        yield return StartCoroutine(LerpCameraSize(mainCam.orthographicSize, defaultSize, zoomInTime));

        zoomRoutine = null;
    }



    // -------------------------------
    //   Normal 복귀 → 카메라 Size=6
    // -------------------------------
    private void HandleResetZoom()
    {
        if (zoomRoutine != null)
            StopCoroutine(zoomRoutine);

        zoomRoutine = StartCoroutine(LerpCameraSize(mainCam.orthographicSize, normalResetSize, normalZoomTime));
    }

    // -------------------------------
    //  공통 Lerp 함수
    // -------------------------------
    private IEnumerator LerpCameraSize(float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Lerp(from, to, t / duration);
            mainCam.orthographicSize = lerp;

            yield return null;
        }

        mainCam.orthographicSize = to;
    }

    //물대포 줌아웃
    private void HandleSkill5Zoom()
    {
        if (zoomRoutine != null)
            StopCoroutine(zoomRoutine);

        zoomRoutine = StartCoroutine(Skill5ZoomRoutine());
    }

    private IEnumerator Skill5ZoomRoutine()
    {
        // 1) 빠른 줌아웃
        yield return StartCoroutine(LerpCameraSize(mainCam.orthographicSize, 10f, 0.5f));

        // 2) 유지
        yield return new WaitForSeconds(1f);

        // 3) 기본값으로 복귀
        yield return StartCoroutine(LerpCameraSize(mainCam.orthographicSize, defaultSize, zoomInTime));

        zoomRoutine = null;
    }
}
