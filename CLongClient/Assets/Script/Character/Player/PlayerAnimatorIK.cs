using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;

public class PlayerAnimatorIK : MonoBehaviour
{
    public Player OwnPlayer;
    public Animator anim;
    
    //Player 부위 
    private Transform leftHand;
    private Transform spine;

    //-0.065 0.016 0.03
    [Range(0.0f, 1.0f)]
    public float lookWeight;
    [Range(0.0f, 1.0f)]
    public float bodyWeight;
    [Range(0.0f, 1.0f)]
    public float headWeight;
    [Range(0.0f, 1.0f)]
    public float eyesWeight;
    [Range(0.0f, 1.0f)]
    public float clampWeight;


    [Range(0.0f, 1.0f)]
    public float leftHandWeight;
    

    void Start()
    {
        leftHand = anim.GetBoneTransform(HumanBodyBones.LeftHand);
        spine = anim.GetBoneTransform(HumanBodyBones.Spine);
    }
    //0.07 -0.15 0.1
    //-0.12 -0.17
    private void LateUpdate()
    {
        anim.SetInteger("StateParam", (int)OwnPlayer.currentActionState);
        anim.SetFloat("WeightParam", OwnPlayer.dirWeightParam);
        anim.SetFloat("VerticalParam", OwnPlayer.verticalParam);
        anim.SetFloat("HorizontalParam", OwnPlayer.horizontalParam);

    }
    /// <summary>
    /// iK관련 함수 왼쪽 손 포지션, 움직이지않을경우 어느각도 이상 TURN했을경우 전체 로테이션변경, 
    /// </summary>
    /// <param name="layerInex"></param>
    void OnAnimatorIK(int layerInex)
    {
        anim.SetLookAtWeight(lookWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
        anim.SetLookAtPosition(OwnPlayer.lookTarget);

        //이동중일경우 y값을 오브젝트 최선임에게 적용 
        if (OwnPlayer.currentActionState != ActionState.None)
        {
            var dir = OwnPlayer.lookTarget - this.transform.position;
            dir = dir.normalized;

            Quaternion q = Quaternion.identity;
            q.SetLookRotation(dir, Vector3.up);
            this.transform.eulerAngles = new Vector3(0, q.eulerAngles.y, 0);
        }

        //플레이어 중심과 바라보는 방향 각도구하기
        var LowerDir = transform.forward;
        var UpperDir = new Vector3(OwnPlayer.lookTarget.x, transform.position.y, OwnPlayer.lookTarget.z) - transform.position;
        UpperDir = UpperDir.normalized;
        var angle = Vector3.Angle(LowerDir, UpperDir);
        //법선벡터의 y값이 양수면 우측turn 음수면 좌측turn 
        var turnDir = Vector3.Cross(LowerDir, UpperDir).y;

        if (angle > 90)
        {
            //우측Turn
            if (turnDir > 0)
            {
                this.transform.eulerAngles = this.transform.eulerAngles + new Vector3(0, Mathf.Lerp(0, 60, 5 * Time.deltaTime));
                //anim.SetBool("RightTurn", true);
            }
            //좌측Turn
            else
            {
                this.transform.eulerAngles = this.transform.eulerAngles + new Vector3(0, Mathf.Lerp(0, -60, 5 * Time.deltaTime));
                //anim.SetBool("LeftTurn", true);
            }
        }
        else
        {
            //anim.SetBool("RightTurn", false);
            //anim.SetBool("LeftTurn", false);
        }

        if (OwnPlayer.weaponManagerSc.CurrentleftHandTarget != null)
        {
            //왼손처리
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, OwnPlayer.weaponManagerSc.CurrentleftHandTarget.position);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, OwnPlayer.weaponManagerSc.CurrentleftHandTarget.rotation);
            leftHand.position = OwnPlayer.weaponManagerSc.CurrentleftHandTarget.position;
        }
    }


}

//카메라에서 변수로 처리하게되면 다른 오브젝트 처리가 불가하므로 playerik에서 처리
//if (spine.localRotation.x > 0.15)
//    anim.SetBool("RightTurn", true);
//else if (spine.localRotation.x < -0.15)
//    anim.SetBool("LeftTurn", true);
//else
//{
//    anim.SetBool("RightTurn", false);
//    anim.SetBool("LeftTurn", false);
//}



// if (spine.localRotation.x > 0.15)    
//      this.transform.eulerAngles = this.transform.eulerAngles + new Vector3(0, Mathf.Lerp(0, 60, 5*Time.deltaTime));
//else if (spine.localRotation.x < -0.15)
//      this.transform.eulerAngles = this.transform.eulerAngles + new Vector3(0, Mathf.Lerp(0, -60, 5*Time.deltaTime));

/*
/// <summary>
/// 방향에따라 설정변경 
/// </summary>
/// <param name="dir"></param>
/// <param name="duration"></param>
/// <returns></returns>
IEnumerator TurnCharacter(float turnDir, float duration)
{
if (TurnCoroutineState)
    yield 
TurnCoroutineState = true;
while (duration > 0)
{
    duration -= Time.deltaTime;

    yield return null;
}
TurnCoroutineState = false;
}*/
