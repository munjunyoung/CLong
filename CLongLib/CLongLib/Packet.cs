using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

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
    /// To Exit game from client
    /// </summary>
    public class ExitReq : Packet
    {
        public string Req { get; set; }
    }

    /// <summary>
    /// IngamePacket Position Info
    /// </summary>
    public class PositionInfo : Packet
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public PositionInfo(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    /// <summary>
    /// ingamePacket Rotation Info
    /// </summary>
    public class RotationInfo : Packet
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public RotationInfo(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}