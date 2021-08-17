using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketDesignation
    {
        enum SelectionType { List, Box }
        static int p;
        static public void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(INetwork net, DesignationDef designation, List<TargetArgs> targets, bool remove)
        {
            var stream = net.GetOutgoingStream();
            stream.Write(p);
            stream.Write(designation.Name);
            stream.Write(remove);
            stream.Write((int)SelectionType.List);
            stream.Write(targets);
        }
        static public void Send(INetwork net, DesignationDef designation, Vector3 begin, Vector3 end, bool remove)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            designation.Write(w);
            w.Write(remove);
            w.Write((int)SelectionType.Box);
            w.Write(begin);
            w.Write(end);
        }
        static public void Receive(INetwork net, BinaryReader r)
        {
            var typeName = r.ReadString();
            var designation = r.ReadDef<DesignationDef>();
            var remove = r.ReadBoolean();
            var selectionType = (SelectionType)r.ReadInt32();
            List<IntVec3> positions;
            if (selectionType == SelectionType.Box)
            {
                var begin = r.ReadVector3();
                var end = r.ReadVector3();
                positions = new BoundingBox(begin, end).GetBoxIntVec3();
                if (net is Server)
                    Send(net, designation, begin, end, remove);
            }
            else if (selectionType == SelectionType.List)
            {
                var list = r.ReadListTargets();
                positions = list.Select(t => (IntVec3)t.Global).ToList();
                if (net is Server)
                    Send(net, designation, list, remove);
            }
            else
                throw new Exception();

            net.EventOccured(Components.Message.Types.ZoneDesignation, designation, positions, remove);
        }
    }
}
