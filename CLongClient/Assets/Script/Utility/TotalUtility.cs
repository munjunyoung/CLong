using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TotalUtility
{
    #region ChangeVector
    /// <summary>
    /// return Numerics Vector3
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static System.Numerics.Vector3 ToNumericVectorChange(Vector3 pos)
    {
        var tempPos = new System.Numerics.Vector3(pos.x, pos.y, pos.z);
        return tempPos;
    }

    /// <summary>
    /// return Unity Vector3
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static Vector3 ToUnityVectorChange(System.Numerics.Vector3 pos)
    {
        var tempPos = new Vector3(pos.X, pos.Y, pos.Z);
        return tempPos;
    }

    #endregion
}

