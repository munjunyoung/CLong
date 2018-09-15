using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CLongLib;

public class ARBase : WeaponBase
{
    public int ShootPeriod; // 연사속도
    //Material material;
    //Shell
    protected string shellType; //사용하는 총알의 종류
    protected GameObject shellPrefab; // 총알 오브젝트 PREFAB

    public Transform LeftHand;
    public Transform ZoomPos;
    
    public int shootPeriodCount = 0;
    public ParticleSystem EffectPrefab;

    public GameObject[] Shell = new GameObject[30];

    
    
    /// <summary>
    /// AR 총
    /// </summary>
    /// <param name="clientNum"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    public override void Shoot(byte clientNum, Vector3 pos, Vector3 dir)
    {
        if (currentItemValue > 0)
        {
            base.Shoot(clientNum, pos, dir);
            EffectPrefab.Play(true);
            ShellIns(shellType, clientNum, pos, dir);
            weaponAudio.clip = weaponAudioClip[0];
            weaponAudio.Play();
            currentItemValue--;
        }
        else
        {
            weaponAudio.clip = weaponAudioClip[1];
            weaponAudio.Play();
        }
    }

    /// <summary>
    /// Reload Gun
    /// </summary>
    public void ReloadStart()
    {
        weaponAudio.clip = weaponAudioClip[2];
        weaponAudio.Play();
    }

    /// <summary>
    /// ReloadEnd
    /// </summary>
    public void ReloadEnd()
    {
        currentItemValue = MaxItemValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="clientNum"></param>
    /// <param name="pos"></param>
    /// <param name="dir"></param>
    public override void ShootSendServer(byte clientNum, Vector3 pos, Vector3 dir)
    {
        base.ShootSendServer(clientNum, pos, dir);
        if (shootPeriodCount > ShootPeriod)
        {
            NetworkManager.Instance.SendPacket(new Use_Item(clientNum,TotalUtility.ToNumericVectorChange(pos),TotalUtility.ToNumericVectorChange(dir)), NetworkManager.Protocol.TCP);
            shootPeriodCount = 0;
        }
        shootPeriodCount++;
    }

    /// <summary>
    /// Create Shell
    /// </summary>
    /// <param name="st"></param>
    protected virtual void ShellIns(string st, byte num, Vector3 pos, Vector3 dir)
    {
        shellPrefab = Instantiate(Resources.Load("Prefab/Item/Weapon/Shell/" + st)) as GameObject;
        //var weaponFireTransform = transform.Find("FirePosition").transform;
        var tmpShellSc = shellPrefab.GetComponentInChildren<ShellScript>();
        tmpShellSc.parentTransform.localPosition = pos;
        tmpShellSc.parentTransform.eulerAngles= dir;
        tmpShellSc.shellSpeed = shellSpeed;
        tmpShellSc.clientNum = num;
        tmpShellSc.damage = damage;
        
        //RaycastHit hit;
        //if(Physics.Raycast(pos, dir, out hit, 300f))
        //{
        //    Debug.Log(hit.collider.tag);
        //    Debug.DrawRay(pos, dir * 100f, Color.red, 5f);
        //}
        
    }
}

