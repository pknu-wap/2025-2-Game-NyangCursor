using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;
using static PlayerStateLogic;

public class PlayerGroundCheck : MonoBehaviour
{
    [Header("Tilemap")]
    public Tilemap groundTilemap;

    [Header("Check Settings")]
    public float checkOffsetY = 0.15f;
    public float safeRadius = 0.25f;
    public float gizmoRadius = 0.05f;

    [Header("Fall Animation")]
    public float fallDistance = 0.5f;
    public float fallDuration = 0.4f;

    private bool isFalling = false;
    public Material instancedMat;
    private Color originalColor = Color.white;
    private Rigidbody2D rb;

    private bool forceFallMode = false; // ⬅ 오버드라이브 탈출 직후 즉사모드
    private float forceFallTimer = 0f;
    private const float forceFallDuration = 2f;

    private readonly int HologramBlendID = Shader.PropertyToID("_HologramBlend");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        isFalling = false;
        instancedMat.SetColor("_Color", originalColor);
        instancedMat.SetFloat(HologramBlendID, 0f);

        rb.simulated = true;


        GaugeOverdriveLogic.OnGetOffEvent += EnableForceFallMode;
    }

    private void OnDisable()
    {
        GaugeOverdriveLogic.OnGetOffEvent -= EnableForceFallMode;
    }

    private void EnableForceFallMode()
    {
        forceFallMode = true;
        forceFallTimer = forceFallDuration;
    }

    void Update()
    {
        if (forceFallMode)
        {
            forceFallTimer -= Time.deltaTime;
            if (forceFallTimer <= 0f)
                forceFallMode = false; // 시간이 지나면 복구
        }

        CheckGround();
    }

    void CheckGround()
    {
        if (isFalling) return;
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.Normal) return;

        Vector3 footPos = transform.position + Vector3.down * checkOffsetY;
        Vector3Int cell = groundTilemap.WorldToCell(footPos);

        TileBase tile = groundTilemap.GetTile(cell);

        // 즉사 모드: 중심에 타일 없으면 바로 죽음
        if (forceFallMode)
        {
            if (tile == null)
            {
                StartFall();
            }
            return;
        }

        //원래 검증 방식으로 복귀
        if (tile != null) return;

        Vector3 tileCenterPos = groundTilemap.GetCellCenterWorld(cell);
        float dist = Vector3.Distance(footPos, tileCenterPos);

        if (dist < safeRadius)
        {
            StartFall();
        }
    }

    void StartFall()
    {
        if (isFalling) return;
        isFalling = true;


        Debug.Log("[FALL] DOTween Shader Fade!");

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        instancedMat.SetFloat(HologramBlendID, 1f); //홀로그램

        Sequence seq = DOTween.Sequence();

        seq.Join(transform.DOMoveY(transform.position.y - fallDistance, fallDuration).SetEase(Ease.InSine));
        seq.Join(DOTween.To(
            () => instancedMat.GetColor("_Color"),
            value => instancedMat.SetColor("_Color", value),
            new Color(originalColor.r, originalColor.g, originalColor.b, 0f),
            fallDuration
        ));

        seq.OnComplete(() =>
        {
            PlayerHpController.Instance.Die();
        });
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * checkOffsetY, gizmoRadius);
    }
#endif
}
