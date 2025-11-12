using System;
using UnityEngine;
using static PlayerStateLogic;

public class GaugeRidingLogic : MonoBehaviour
{

    public float ridingGauge = 0f;

    private bool activeEkey = false; //게이지 100프로일때 E키를 눌러서 탑승 가능

    //각종 이벤트
    public static event Action<float> OnRidingGaugeTick; //라이딩 게이지 틱 이벤트
    public static event Action OnFullRidingGauge; //라이딩 게이지 100퍼센트 달성 이벤트
    public static event Action OnRidingEvent; //E키를 눌렀을때 라이딩 하는 이벤트
    public static event Action OnOverDriveEvent;//오버드라이브 진입한 이벤트

    [SerializeField] private Rigidbody2D rigidPlayer;


    private void Awake()
    {
        OnRidingGaugeTick?.Invoke(ridingGauge);//라이딩 게이지 틱 이벤트 발송


    }

    private void OnEnable()
    {
        BulletColliderManager.OnBulletHit += HandleBulletHit;
    }

    private void OnDisable()
    {
        BulletColliderManager.OnBulletHit -= HandleBulletHit;
    }

    private void HandleBulletHit(float value)
    {
        // 
        UpRidingGauge(value);
    }

    void Update()
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.Normal)
            return;

        if (activeEkey == true && ridingGauge >= 100f)
        {
            // E 키 입력 → Riding() 호출
            if (Input.GetKeyDown(KeyCode.E))
            {
                OnRidingEvent?.Invoke(); //라이딩 이벤트 발송 To(PlayerAnimator,NormalCursorMove)
                PlayerStateLogic.Instance.ChangeState(PlayerState.Riding); //라이딩 상태변경
                ResetRidingGauge();

                
                rigidPlayer.bodyType = RigidbodyType2D.Kinematic; //탑승중 키네마틱(안밀림)

                Invoke("ChangeSpeedZero", 0.1f);
                Invoke("ChangeStateToOverDrive", 1); //1초뒤에 오버드라이브 상태변경
            }
        }
    }

    public void ChangeStateToOverDrive()
    {
        PlayerStateLogic.Instance.ChangeState(PlayerState.OverDrive);
        OnOverDriveEvent?.Invoke(); //오버드라이브 이벤트 발송 To(OverDriveModController)

        rigidPlayer.bodyType = RigidbodyType2D.Dynamic; //오버드라이브 다이나믹(밀림)
    }

    public void ChangeSpeedZero()
    {
        rigidPlayer.linearVelocity = Vector2.zero;
    }

    public void UpRidingGauge(float amount)
    {
        if (PlayerStateLogic.Instance.CurrentState != PlayerState.Normal)
            return;

        ridingGauge = Mathf.Clamp(ridingGauge + amount, 0, 100);
        OnRidingGaugeTick?.Invoke(ridingGauge);//라이딩 게이지 틱 이벤트 발송

        //100프로 달성 시 E 버튼 활성화
        if (ridingGauge >= 100f)
        {
            activeEkey = true;
        }
    }

    public void ResetRidingGauge() //라이딩 게이지 초기화
    {
        ridingGauge = 0f;
        OnRidingGaugeTick?.Invoke(ridingGauge);
        activeEkey = false;
    }


}
