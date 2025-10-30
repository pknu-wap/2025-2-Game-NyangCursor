using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerShotManager : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint; // 총알 발사 위치
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

    private void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    private void Update()
    {
        if (isReloading) return;

        // 발사 입력
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (!canShoot || currentAmmo <= 0)
            return;

        Shoot();
        currentAmmo--;
        UpdateAmmoUI();

        // 쿨타임
        StartCoroutine(FireCooldownRoutine());

        // 총알 다 썼으면 자동 재장전 시작
        if (currentAmmo <= 0)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("⚠️ Bullet Prefab 또는 Fire Point가 지정되지 않았습니다.");
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 커서 방향 계산
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - firePoint.position).normalized;

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = direction * bulletSpeed;
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
            ammoImages[i].enabled = i < currentAmmo;
        }
    }
}
