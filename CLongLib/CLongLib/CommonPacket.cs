using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Numerics;
namespace CLongLib
{
    public enum Key { W = 0, A, S, D, LeftShift, LeftControl, Z, Alpha1, Alpha2};
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
