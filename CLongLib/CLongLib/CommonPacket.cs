﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Numerics;
namespace CLongLib
{
    //SceneState
    public enum Scene { Login, Lobby, Ingame};
    //Key
    public enum Key { W = 0, S, A, D, LeftShift, LeftControl, Z, Alpha1, Alpha2, Space};
    //Action State server and client 
    public enum ActionState { None, Run, Seat, Lie, Jump };
    /// <summary>
    /// Common Packet;
    /// </summary>
    public class Packet
    {
        [JsonIgnore]
        public string MsgName => GetType().Name;
        [JsonIgnore]
        public string Data => JsonConvert.SerializeObject(this);
    }
   
    
    /// <summary>
    /// To Exit game from client
    /// </summary>
    public class ExitReq : Packet
    {
        public string Req { get; set; }
    }
}
