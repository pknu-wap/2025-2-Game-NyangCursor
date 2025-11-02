using UnityEngine;

public class PlayerShadow : MonoBehaviour
{
    [SerializeField] GameObject shadow;

    private void OnEnable()
    {
        if (PlayerStateLogic.Instance != null)
            PlayerStateLogic.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (PlayerStateLogic.Instance != null)
            PlayerStateLogic.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(PlayerStateLogic.PlayerState state)
    {
        Debug.Log($"[PlayerShadow] 상태 변경 감지: {state}");

        if (shadow == null) return;

        if (state == PlayerStateLogic.PlayerState.OverDrive)
            shadow.SetActive(false);
        else if (state == PlayerStateLogic.PlayerState.Normal)
            shadow.SetActive(true);
    }
}
