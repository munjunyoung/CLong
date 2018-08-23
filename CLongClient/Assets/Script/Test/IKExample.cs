using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKExample : MonoBehaviour
{
    
    public Vector3 lookTarget;
    public Transform testTarget;
    public Transform rightHandTargetIK;
    public Transform leftHandTargetIK;

    public Transform equipWeaponTransform;
    Transform rightShoulder;

    Transform rightHand;
    Transform leftHand;

    //Turning
    Transform spine;
    //Hip은 chest의 상위이기 때문에 구분불가
    //왼쪽으로 볼경우 leftLeg와 비교 오른쪽을 볼경우 right와 비교
    Transform leftLeg;
    Transform rightLeg;

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

    public Animator animator;

    [Range(0.0f, 1.0f)]
    public float leftHandWeight;
    [Range(0.0f, 1.0f)]
    public float rightHandWeight;

    //Animation Bool
    bool StandRelaxedState = false;
    bool StandAimState = false;

    int standAnimatorInt = 0;
    int moveAnimatorInt = 0;


    void Start()
    {
        //animator = GetComponent<Animator>();
        rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
        leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

        //Turning
        spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        leftLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        rightLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        
    }
    //0.07 -0.15 0.1
    //-0.12 -0.17
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            standAnimatorInt = standAnimatorInt.Equals(1) ? 0 : 1;
        if(Input.GetKeyDown(KeyCode.Alpha2))
            standAnimatorInt = standAnimatorInt.Equals(2) ? 1 : 2;

        if (Input.GetKey(KeyCode.W))
            moveAnimatorInt = 1;
        if (Input.GetKeyUp(KeyCode.W))
            moveAnimatorInt = 0;


        //Debug.Log(animator.GetLayerName());
        animator.SetInteger("StandEquipState", standAnimatorInt);
        animator.SetInteger("MoveEquipState", moveAnimatorInt);
        //Animation
        //equipWeaponTransform.position = rightShoulder.position;
        //rightHand.position = rightHand.position;
    }
    private void LateUpdate()
    {
        if(standAnimatorInt>0||moveAnimatorInt>0)
            leftHand.position = leftHandTargetIK.position;
    }

    private void TurningAngleSet()
    {
       
    }

    void OnAnimatorIK(int layerInex)
    {
        animator.SetLookAtWeight(lookWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
        animator.SetLookAtPosition(lookTarget);


        //animator.SetBoneLocalRotation(HumanBodyBones.Chest, );
        //animator.GetBoneTransform(HumanBodyBones.Chest).transform.LookAt(lookTarget);

        //animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
        //animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTargetIK.position);

        //animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
        //animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTargetIK.position);
        if (standAnimatorInt>0||moveAnimatorInt>0)
        {
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTargetIK.rotation);
        }
            //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
        //animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        
    }

    
}
