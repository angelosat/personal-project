﻿using System;
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
        static public void Init()
        {
            // TODO
            Server.RegisterPacketHandler(PacketType.Designation, Receive);
            Client.RegisterPacketHandler(PacketType.Designation, Receive);
        }
        static public void Send(IObjectProvider net, DesignationDef designation, List<TargetArgs> targets, bool remove)
        {
            var stream = net.GetOutgoingStream();
            stream.Write((int)PacketType.Designation);
            stream.Write(designation.Name);
            stream.Write(remove);
            stream.Write((int)SelectionType.List);
            stream.Write(targets);
        }
        static public void Send(IObjectProvider net, DesignationDef designation, Vector3 begin, Vector3 end, bool remove)
        {
            var stream = net.GetOutgoingStream();
            stream.Write((int)PacketType.Designation);
            stream.Write(designation.Name);
            stream.Write(remove);
            stream.Write((int)SelectionType.Box);
            stream.Write(begin);
            stream.Write(end);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var typeName = r.ReadString();
            var designation = DesignationDef.Dictionary[typeName];
            var remove = r.ReadBoolean();
            var selectionType = (SelectionType)r.ReadInt32();
            List<Vector3> positions;
            if (selectionType == SelectionType.Box)
            {
                var begin = r.ReadVector3();
                var end = r.ReadVector3();
                positions = new BoundingBox(begin, end).GetBox();
                if (net is Server)
                    Send(net, designation, begin, end, remove);
            }
            else if (selectionType == SelectionType.List)
            {
                var list = r.ReadListTargets();
                positions = list.Select(t => t.Global).ToList();
                if (net is Server)
                    Send(net, designation, list, remove);
            }
            else
                throw new Exception();

            net.EventOccured(Components.Message.Types.ZoneDesignation, designation, positions, remove);
        }
    }
}
