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
    /// To entry ingameScene from client
    /// </summary>
    public class StartGameReq : Packet
    {
        public string Req { get; set; }
    }

    /// <summary>
    /// enqueue in queue list
    /// </summary>
    public class QueueEntry : Packet
    {
        public string Req { get; set; }
    }

    /// <summary>
    /// dequeue in queue list
    /// </summary>
    public class QueueLeave : Packet
    {
        public string Req { get; set; }
    }

    /// <summary>
    /// IngamePacket Position Info
    /// </summary>
    public class PositionInfo : Packet
    {
        public Vector3 ClientPos { get; set; }
        
        public PositionInfo(Vector3 p)
        {
            ClientPos = p;
        }
    }
    
    /// <summary>
    /// ingamePacket Rotation Info
    /// </summary>
    public class RotationInfo : Packet
    {
       public Vector3 ClientRot { get; set; }

        public RotationInfo(Vector3 r)
        {
            ClientRot = r;
        }
    }



    /// <summary>
    /// To Exit game from client
    /// </summary>
    public class ExitReq : Packet
    {
        public string Req { get; set; }
    }
}
