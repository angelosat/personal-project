using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Net
{
    abstract class GameMode
    {
        public abstract void HandlePacket(PacketType type);
    }
}
