using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using tcpNet;
using CLongLib;

public class ARBase : WeaponBase
{
    public int ShootPeriod; // 연사속도
    //Material material;
    //Shell
    protected string shellType; //사용하는 총알의 종류
    protected GameObject shellPrefab; // 총알 오브젝트 PREFAB

    public int shootPeriodCount = 0;

    /// <summary>
    /// AR 총
    /// </summary>
    /// <param name="clientNum"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    public override void Shoot(int clientNum, Vector3 pos, Vector3 dir)
    {
        base.Shoot(clientNum, pos, dir);

        ShellIns(shellType, clientNum, pos, dir);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="clientNum"></param>
    /// <param name="pos"></param>
    /// <param name="dir"></param>
    public override void ShootSendServer(int clientNum, Vector3 pos, Vector3 dir)
    {
        base.ShootSendServer(clientNum, pos, dir);
        if (shootPeriodCount > ShootPeriod)
        {
            NetworkManagerTCP.SendTCP(new InsShell(clientNum, IngameProcess.ToNumericVectorChange(pos), IngameProcess.ToNumericVectorChange(dir)));
            shootPeriodCount = 0;
        }
        shootPeriodCount++;
    }
    /// <summary>
    /// Create Shell
    /// </summary>
    /// <param name="st"></param>
    protected virtual void ShellIns(string st, int num, Vector3 pos, Vector3 rot)
    {
        shellPrefab = Instantiate(Resources.Load("Prefab/Weapon/Shell/" + st)) as GameObject;
        //var weaponFireTransform = transform.Find("FirePosition").transform;
        var tmpShellSc = shellPrefab.GetComponent<ShellScript>();
        tmpShellSc.transform.localPosition = pos;
        tmpShellSc.transform.eulerAngles = rot;
        tmpShellSc.shellSpeed = shellSpeed;
        tmpShellSc.clientNum = num;
        tmpShellSc.damage = damage;
    }
}

