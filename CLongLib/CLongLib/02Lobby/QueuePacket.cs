using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLongLib
{ 
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
    public class QueueCancel : Packet
    {
        public string Req { get; set; }
    }
    /// <summary>
    /// matching complete
    /// </summary>
    public class MatchingComplete : Packet
    {
        public string Req { get; set; }
    }

    /// <summary>
    /// To entry ingameScene from client
    /// </summary>
    public class StartGameReq : Packet
    {
        public string Req { get; set; }
    }

}
