using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerShotManager : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;

    [SerializeField] private LayerMask enemyLayer; //넉백당할 적 레이어

    [SerializeField]private GameObject MuzzleFlashEffect; //총구이펙트

    [SerializeField] private GameObject firePoint; // 총알 발사 위치
    private CursorMove cursorMove;
    [SerializeField] private float bulletSpeed = 10f;

    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 10;
    [SerializeField] private List<Image> ammoImages; // 탄창 UI 이미지 리스트

    [Header("Cooldown Settings")]
    [SerializeField] private float fireCooldown = 1f;
    [SerializeField] private float reloadCooldown = 3f;

    private int currentAmmo;
    private bool canShoot = true;
    private bool isReloading = false;

    public static Action OnshotEvent;
    private void Start()
    {
        // 캐싱
        cursorMove = firePoint.GetComponent<CursorMove>();
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    private bool IsAbleToShoot()
    {
        return PlayerStateLogic.Instance.CurrentState == PlayerStateLogic.PlayerState.Normal;
    }

    private void Update()
    {
        // 노말 모드 아니면 총 쏠수 없게
        if (!IsAbleToShoot()) return;
        if (isReloading) return;

        // 발사 입력
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (!canShoot || currentAmmo <= 0) return;

        // 1) 커서 반동 이동 시작 → 도착 시점에 ShootProcess 실행
        cursorMove.StartShotRecoil(() =>
        {
            StartCoroutine(ShootProcess());
        });
    }

    private IEnumerator ShootProcess()
    {
        // 2) 총구 이펙트
        MuzzleFlashEffect.SetActive(false);
        MuzzleFlashEffect.SetActive(true);

        // 3) 총알 생성
        GameObject bullet = PoolManager.instance.Spawn(bulletPrefab, firePoint.transform.position);

        // 4) 마우스 방향 계산
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector2 direction = (worldMousePos - firePoint.transform.position).normalized;

        // 5) 총알 속도 적용
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = direction * bulletSpeed;

        // 6) 커서 방향 연출
        cursorMove.RotateTowardDirection(direction);

        // 7) 탄약 감소
        currentAmmo--;
        UpdateAmmoUI();

        // 8) 쿨타임
        StartCoroutine(FireCooldownRoutine());

        // 9) 재장전 체크
        if (currentAmmo <= 0)
            StartCoroutine(ReloadRoutine());

        yield break;
    }
    void KnockBack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 2, enemyLayer);

        foreach (var col in enemies)
        {
            var knockback = col.GetComponent<IKnockbackable>();
            if (knockback != null)
            {
                knockback.ApplyKnockback(firePoint.transform.position, 20);
            }
        }
    }


    private IEnumerator FireCooldownRoutine()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireCooldown);
        canShoot = true;
    }

    private IEnumerator ReloadRoutine()
    {
        if (isReloading) yield break;

        isReloading = true;
        canShoot = false;

        Debug.Log("🔄 재장전 중...");

        yield return new WaitForSeconds(reloadCooldown);

        currentAmmo = maxAmmo;
        UpdateAmmoUI();

        Debug.Log("✅ 재장전 완료!");

        isReloading = false;
        canShoot = true;
    }

    private void UpdateAmmoUI()
    {
        for (int i = 0; i < ammoImages.Count; i++)
        {
            int reversedIndex = ammoImages.Count - 1 - i;
            ammoImages[reversedIndex].enabled = i < currentAmmo;
        }
    }

}
