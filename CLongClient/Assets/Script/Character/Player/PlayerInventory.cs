using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory { 
    string mainWeapon; //주무기
    string secondWeapon; // 권총
    string assistWeapon; // 칼 도끼등
    string Bomb; // 수류탄 연막탄등
    
    /// <summary>
    /// when the game starts, setting Inventory
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="third"></param>
    /// <param name="throwAble"></param>
    PlayerInventory(string first, string second, string third, string throwAble) { }
}
