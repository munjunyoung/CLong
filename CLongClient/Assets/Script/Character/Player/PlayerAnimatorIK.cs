using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;

public class PlayerAnimatorIK : MonoBehaviour
{
    public Animator animator;
    public Vector3 lookTarget;

    public Transform leftHandTargetIK;

    public Transform equipWeaponTransform;

    Transform leftHand;
    
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
    }
    //0.07 -0.15 0.1
    //-0.12 -0.17
    private void Update()
    {
        animator.SetInteger("StateParam", (int)AnimActionState);
    }
    private void LateUpdate()
    {
        //조준상태가 아닐경우 localEuler x : 0 y : -90 z : -90
        leftHand.position = leftHandTargetIK.position;
        equipWeaponTransform.LookAt(lookTarget);
    }
    
    void OnAnimatorIK(int layerInex)
    {
        if (AnimActionState == ActionState.None)
        {
            animator.SetLookAtWeight(lookWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
            animator.SetLookAtPosition(lookTarget);
        }
        //왼손처리
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTargetIK.position);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTargetIK.rotation);
    }


}
