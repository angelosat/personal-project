using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Components;

namespace Start_a_Town_.Net
{
    class RPCargs
    {
        static public byte[] Create(TargetArgs target, Message.Types type, Action<BinaryWriter> w)
        {
            return Net.Network.Serialize(ww =>
            {
                target.Write(ww);
                ww.Write((int)type);
                w(ww);
            });
        }
    }
}
