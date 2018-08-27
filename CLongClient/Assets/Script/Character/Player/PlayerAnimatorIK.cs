using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;

public class PlayerAnimatorIK : MonoBehaviour
{
    public Animator animator;
    public Vector3 lookTarget;
    public Vector3 camEulerAngle;

    public Transform leftHandTargetIK;

    public Transform equipWeaponTransform;

    public Transform leftHand;
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

    //Animation Bool
    bool StandRelaxedState = false;
    bool StandAimState = false;

    int standAnimatorInt = 0;
    int moveAnimatorInt = 0;

    public ActionState AnimActionState;

    void Start()
    {
        leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        spine = animator.GetBoneTransform(HumanBodyBones.Spine);
    }
    //0.07 -0.15 0.1
    //-0.12 -0.17
    private void Update()
    {
        animator.SetInteger("StateParam", (int)AnimActionState);

        //LookTarget으로 할시에 각도가 안맞을경우가 생김.. 
        equipWeaponTransform.eulerAngles = camEulerAngle;
        //equipWeaponTransform.LookAt(lookTarget);
    }
    private void LateUpdate()
    {
        //조준상태가 아닐경우 localEuler x : 0 y : -90 z : -90
        leftHand.position = leftHandTargetIK.position;
    }

    void OnAnimatorIK(int layerInex)
    {
        animator.SetLookAtWeight(lookWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
        animator.SetLookAtPosition(lookTarget);

        //이동중일경우 y값을 직접적용 
        if (AnimActionState != ActionState.None)
        {
            var dir = lookTarget - animator.bodyPosition;
            dir = dir.normalized;

            Quaternion q = Quaternion.identity;
            q.SetLookRotation(dir, Vector3.up);
            this.transform.eulerAngles = new Vector3(0, q.eulerAngles.y, 0);
        }
        //Debug.Log(spine.localRotation);
        if(spine.localRotation.x>0.4||spine.localRotation.x < -0.4)
        {
            var dir = lookTarget - animator.bodyPosition;
            dir = dir.normalized;

            Quaternion endQ = Quaternion.identity;
            endQ.SetLookRotation(dir, Vector3.up);
            this.transform.eulerAngles = new Vector3(0,Mathf.Lerp(this.transform.eulerAngles.y, endQ.eulerAngles.y,Time.deltaTime), 0);
           // animator.SetTrigger("RightTurn");
        }

        //왼손처리
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTargetIK.position);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTargetIK.rotation);
    }

    IEnumerator TurnCharacter()
    {

        yield return null;
    }
}
