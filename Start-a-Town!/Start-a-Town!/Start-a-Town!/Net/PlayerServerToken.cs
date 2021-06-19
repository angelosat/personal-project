using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using Start_a_Town_.Net;

namespace Start_a_Town_.Net
{
    class PlayerServerToken : PlayerData
    {
        Queue<Packet> Outgoing { get; set; }
        public void Enqueue(Packet msg)
        {
            this.Outgoing.Enqueue(msg);
        }
        public PlayerServerToken(EndPoint endPoint)
            : base(endPoint)
        {
            this.Outgoing = new Queue<Packet>();
        }
    }
}
