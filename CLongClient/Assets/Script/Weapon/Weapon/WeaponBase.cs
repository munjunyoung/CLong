using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    //Weapon (Input에서 반동 처리를 하기위함..) 
    public float reboundIntensity; // 반동세기
    public float reboundRecoveryTime;//반동회복?
    //private WeaponScript go;
    public string weaponType; // 해당 타입에 따라 InputManager에서 줌을 사용을 처리하기 위해Public
    protected string weaponName; //
    //weapon Option
    protected int damage; // 총기 데미지
    protected int shellSpeed; // 총알이 날아가는 속도
    //Equip
    public int equipWeaponNum; //무기별 장착 번호
    // false -> 사용했음 true -> 가지고있음
    public bool weaponState = false;
    //Bomb
    //Eating

    /// <summary>
    /// 상속
    /// </summary>
    protected virtual void Start()
    {
    }
    
    /// <summary>
    /// 서버에서 패킷을 받은후 총알을 생성하기위함
    /// </summary>
    public virtual void Shoot(byte clientNum, Vector3 pos, Vector3 rot)
    {
        //연사속도관련
     
    }

    /// <summary>
    /// 웨폰에서 반동등 적용해서 전송하기 위함 위함
    /// </summary>
    /// <param name="clientNum"></param>
    /// <param name="pos"></param>
    /// <param name="dir"></param>
    public virtual void ShootSendServer(byte clientNum, Vector3 pos, Vector3 dir)
    {
        
    }
}
