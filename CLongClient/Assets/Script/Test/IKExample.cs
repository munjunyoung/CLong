using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKExample : MonoBehaviour
{
    
    public Vector3 lookTarget;
    public Transform testTarget;
    public Transform rightHandTarget;
    public Transform leftHandTarget;
    public Transform rightArmTarget;
    public Transform leftArmTarget;

    Transform chest;

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
    [Range(0.0f, 1.0f)]
    public float leftArmWeight;
    [Range(0.0f, 1.0f)]
    public float rightArmWeight;
    void Start()
    {
        //animator = GetComponent<Animator>();
        chest = animator.GetBoneTransform(HumanBodyBones.Chest); 
    }
    //0.07 -0.15 0.1
    //-0.12 -0.17
    private void Update()
    {

        //if (Input.GetKeyDown(KeyCode.W))
        //    animator.SetBool("forward", true);
        //if (Input.GetKeyUp(KeyCode.W))
        //    animator.SetBool("forward", false);

        //if (Input.GetKeyDown(KeyCode.D))
        //    animator.SetBool("right", true);
        //if (Input.GetKeyUp(KeyCode.D))
        //    animator.SetBool("right", false);

        //if (Input.GetKeyDown(KeyCode.A))
        //    animator.SetBool("left", true);
        //if (Input.GetKeyUp(KeyCode.A))
        //    animator.SetBool("left", false);
        

    }
    private void LateUpdate()
    {

        
       // chest.transform.LookAt(lookTarget);
       // chest.transform.rotation = chest.rotation * Quaternion.Euler(0, 270, -90);
        //Debug.Log("확인 : " + lookTarget);
        //rightHandTarget.rotation = animator.GetBoneTransform(HumanBodyBones.right).rotation * Quaternion.Euler(90, 0, 0);
        //leftHandTarget.position = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
    }

    void OnAnimatorIK(int layerInex)
    {

        animator.SetLookAtWeight(lookWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
        animator.SetLookAtPosition(lookTarget);
        //animator.SetBoneLocalRotation(HumanBodyBones.Chest, );
        //animator.GetBoneTransform(HumanBodyBones.Chest).transform.LookAt(lookTarget);
     
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);

        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);

        //animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, leftArmWeight);
        //animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftArmTarget.position);

        //animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, rightArmWeight);
        //animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightArmTarget.position);



        //animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        //animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
        //animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);

    }

    
}
