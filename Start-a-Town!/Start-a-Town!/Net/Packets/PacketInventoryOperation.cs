using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net.Packets
{
    class PacketInventoryOperation : Packet
    {
        public int EntityID;
        public TargetArgs SourceSlot, DestinationSlot;
        public int Amount;


    }
}
