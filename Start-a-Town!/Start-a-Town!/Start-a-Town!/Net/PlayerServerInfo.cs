using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Net
{
    class PlayerServerInfo : PlayerData
    {
        public ConcurrentDictionary<long, Packet> WaitingForAck = new ConcurrentDictionary<long, Packet>();
    }
}
