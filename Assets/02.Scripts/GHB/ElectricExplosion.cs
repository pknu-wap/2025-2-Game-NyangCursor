using System.Collections;
using UnityEngine;

public class ElectricExplosion : MonoBehaviour
{
    [SerializeField] private float duration = 0.5f;

    private void OnEnable()
    {
        StartCoroutine(AutoDespawn());
    }

    private IEnumerator AutoDespawn()
    {
        yield return new WaitForSeconds(duration);

        PoolManager.instance.Despawn(gameObject);
    }
}
