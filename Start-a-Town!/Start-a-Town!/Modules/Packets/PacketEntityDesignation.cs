using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [Obsolete]
    class PacketEntityDesignation
    {
        const int SelectionRectangle = 0;
        const int SelectionList = 1;

        static public void Init()
        {
            Client.RegisterPacketHandler(PacketType.ChoppingDesignation, Receive);
            Server.RegisterPacketHandler(PacketType.ChoppingDesignation, Receive);
        }
        static public void Send(IObjectProvider net, int type, IEnumerable<GameObject> targets, bool value)
        {
            Send(net, type, targets.Select(i => i.RefID).ToList(), value);
        }
        static public void Send(IObjectProvider net, int type, IEnumerable<TargetArgs> targets, bool value)
        {
            Send(net, type, targets.Select(i => i.Object.RefID).ToList(), value);
        }
        static public void Send(IObjectProvider net, int type, List<int> targets, bool value)
        {
            var strem = net.GetOutgoingStream();
            strem.Write((int)PacketType.ChoppingDesignation);
            strem.Write(type);
            strem.Write(SelectionList);
            strem.Write(targets);
            strem.Write(value);
        }
        static public void Send(IObjectProvider net, int type, Vector3 start, Vector3 end, bool value)
        {
            var strem = net.GetOutgoingStream();
            strem.Write((int)PacketType.ChoppingDesignation);
            strem.Write(type);
            strem.Write(SelectionRectangle);
            strem.Write(start);
            strem.Write(end);
            strem.Write(value);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var manager = net.Map.Town.ChoppingManager;
            var type = r.ReadInt32();
            var selectionType = r.ReadInt32();
            switch(selectionType)
            {
                case SelectionRectangle:
                    {
                    var start = r.ReadVector3();
                    var end = r.ReadVector3();
                    var remove = r.ReadBoolean();
                    manager.Designate(type, start, end, remove);
                    if (net is Server)
                        Send(net, type, start, end, remove);
                    }
                    break;

                case SelectionList:
                    {
                    var ids = r.ReadListInt();
                    var remove = r.ReadBoolean();
                    manager.Designate(type, ids, remove);
                    if (net is Server)
                        Send(net, type, ids, remove);
                    }
                    break;

                default:
                    throw new Exception();
            }
           
        }
        static public byte[] Write(int entityID, Vector3 start, Vector3 end, bool value)
        {
            return Network.Serialize(w =>
            {
                w.Write(entityID);
                w.Write(start);
                w.Write(end);
                w.Write(value);
            });
        }
        static public void Read(BinaryReader r, out int entityID, out Vector3 start, out Vector3 end, out bool value)
        {
            entityID = r.ReadInt32();
            start = r.ReadVector3();
            end = r.ReadVector3();
            value = r.ReadBoolean();
        }
    }
}
