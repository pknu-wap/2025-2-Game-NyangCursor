using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{

    [SerializeField] private Animator animator;

    [SerializeField] private GameObject NormalObj;
    [SerializeField] private GameObject OverDriveObj;


    void Awake()
    {
        animator = GetComponent<Animator>();
        GaugeRidingLogic.OnRidingEvent += HandleRidingAnim;
        GaugeOverdriveLogic.OnGetOffEvent += HandleGetOffAnim;
        GaugeRidingLogic.OnOverDriveEvent += HandleOverDriveAnim;
        NormalModController.OnWalk += HandleWalkAnim;
    }

    void OnDestroy()
    {
        GaugeRidingLogic.OnRidingEvent -= HandleRidingAnim;
        GaugeOverdriveLogic.OnGetOffEvent -= HandleGetOffAnim;
        GaugeRidingLogic.OnOverDriveEvent -= HandleOverDriveAnim;
        NormalModController.OnWalk -= HandleWalkAnim;
    }

    //애니메이션 조건 

    private void HandleWalkAnim(bool isWalk) //걷는애니메이션 
    {
        animator.SetBool("isWalk", isWalk);
    }
    private void HandleRidingAnim() //타는 애니메이션
    {
        animator.SetBool("isRiding", true);
    }
    private void HandleOverDriveAnim() //오버드라이브 애니메이션
    {
        animator.SetBool("isOverDrive", true);
        OverDriveObj.SetActive(true);
        NormalObj.SetActive(false);
    }

    private void HandleGetOffAnim() //떨어지는 애니메이션
    {
        animator.SetBool("isRiding", false);
        animator.SetBool("isOverDrive", false);
        animator.SetBool("isWalk", false);
        NormalObj.SetActive(true);
        OverDriveObj.SetActive(false);
    }

 


}
