﻿using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketForceTask
    {
        static readonly PacketType PType = PacketType.ForceTask;
        internal static void Init()
        {
            Server.RegisterPacketHandler(PType, Receive);
        }
        public static void Send(TaskGiver def, Actor actor, TargetArgs target)
        {
            var client = actor.Map.Net as Client;
            var w = client.GetOutgoingStream();
            w.Write(PType);
            w.Write(actor.RefID);
            w.Write(def.GetType().FullName);
            target.Write(w);
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
        {
            var actor = net.GetNetworkObject(r.ReadInt32()) as Actor;
            var typeName = r.ReadString();
            var taskGiver = Activator.CreateInstance(Type.GetType(typeName)) as TaskGiver;
            var target = TargetArgs.Read(actor.Map, r);

            actor.ForceTask(taskGiver, target);
        }
    }
}
