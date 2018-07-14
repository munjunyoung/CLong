using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Numerics;
namespace CLongLib
{
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
