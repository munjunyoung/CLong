using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;

public class PlayerAnimatorIK : MonoBehaviour
{
    public Animator anim;
    public Vector3 lookTarget;
    public Transform camRot;
    
    //Weapon
    public Transform equipWeaponTransform;
    public Transform leftHandTargetIK;
    //Player 부위 
    public Transform leftHand;
    private Transform spine;
    private Transform head;
    
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
    
    //AnimationBool
    bool TurnCoroutineState = false;

    public ActionState AnimActionState;

    void Start()
    {
        leftHand = anim.GetBoneTransform(HumanBodyBones.LeftHand);
        spine = anim.GetBoneTransform(HumanBodyBones.Spine);
       
        head = anim.GetBoneTransform(HumanBodyBones.Head);
    }
    //0.07 -0.15 0.1
    //-0.12 -0.17
    private void Update()
    {
        anim.SetInteger("StateParam", (int)AnimActionState);
    }
    private void LateUpdate()
    {
        //조준상태가 아닐경우 localEuler x : 0 y : -90 z : -90
        leftHand.position = leftHandTargetIK.position;

        equipWeaponTransform.rotation = Quaternion.Slerp(equipWeaponTransform.rotation, camRot.rotation, 10 * Time.deltaTime);
        //LookTarget으로 할시에 줌을 했을때 각도가 안맞는경우가 생기는거같은데 잘모르겠다; 결국 카메라 로테이션 가져오는것으로 변경
        //equipWeaponTransform.LookAt(lookTarget);

        // equipWeaponTransform.eulerAngles = Vector3.Slerp(equipWeaponTransform.eulerAngles, camEulerAngle, 10 * Time.deltaTime);
        //equipWeaponTransform.LookAt(lookTarget);
    }

    void OnAnimatorIK(int layerInex)
    {
         anim.SetLookAtWeight(lookWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
         anim.SetLookAtPosition(lookTarget);
       
        //이동중일경우 y값을 직접적용 
        if (AnimActionState != ActionState.None)
        {
            var dir = lookTarget - anim.bodyPosition;
            dir = dir.normalized;

            Quaternion q = Quaternion.identity;
            q.SetLookRotation(dir, Vector3.up);
            this.transform.eulerAngles = new Vector3(0, q.eulerAngles.y, 0);
        }
        
        //카메라에서 변수로 처리하게되면 다른 오브젝트 처리가 불가하므로 playerik에서 처리
        if (spine.localRotation.x > 0.15)
            this.transform.eulerAngles = this.transform.eulerAngles + new Vector3(0, Mathf.Lerp(0, 60, 5*Time.deltaTime));
        else if (spine.localRotation.x < -0.15)
            this.transform.eulerAngles = this.transform.eulerAngles + new Vector3(0, Mathf.Lerp(0, -60, 5*Time.deltaTime));
        
        //왼손처리
        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
        anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTargetIK.position);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTargetIK.rotation);
    }
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
}
