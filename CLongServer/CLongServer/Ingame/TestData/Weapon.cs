using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLongServer.Ingame
{
    public class Weapon
    {
        //Weapon
        protected string weaponType;
        public string weaponName;
        //Gun Option
        protected int damage; // 총기 데미지
        protected int shellSpeed; // 총알이 날아가는 속도
        public int ShootPeriod; // 연사속도
        protected int reboundIntensity; // 반동세기
        protected string shellType;
    }
    
    public class AR : Weapon
    {

    }

    public class AK : AR
    {
        public AK()
        {
            weaponType = "AR";
            weaponName = "AK";
            shellType = "7mm";
            damage = 43;
            shellSpeed = 50;
            ShootPeriod = 5;
        }
    }
    
    public class M4 : AR
    {
        public M4()
        {
            weaponType = "AR";
            weaponName = "M4";
            shellType = "5mm";
            damage = 30;
            shellSpeed = 70;
            ShootPeriod = 3;
        }
    }
}
