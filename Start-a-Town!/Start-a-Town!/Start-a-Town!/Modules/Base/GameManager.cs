using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Start_a_Town_.Net.Packets;

namespace Start_a_Town_.Modules.Base
{
    class GameManager : GameComponent
    {
        public override void Initialize()
        {
            Net.Client.Instance.RegisterPacket(PacketType.MergeEntities, PacketMergeEntities.Handle);
        }
    }
}
