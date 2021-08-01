﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityInstantiate
    {
        static readonly int PckType;
        static readonly int PckTypeNew;
        static PacketEntityInstantiate()
        {
            PckType = Network.RegisterPacketHandler(Receive);
            PckTypeNew = Network.RegisterPacketHandler(ReceiveTemplate);
        }
        [Obsolete]
        static public void Send(INetwork net, GameObject entity)
        {
            Send(net, new GameObject[] { entity });
        }
        static public void SendFromTemplate(INetwork net, int templateID, GameObject entity)
        {
            if (net is Client)
                throw new Exception();
            var strem = net.GetOutgoingStream();
            strem.Write(PckTypeNew);
            strem.Write(templateID);
            var data = entity.Serialize(); // why send it compressed?
            strem.Write(data.Length);
            strem.Write(data);
        }
        static public void ReceiveTemplate(INetwork net, BinaryReader r)
        {
            if (net is Server)
                throw new Exception();
            var templateID = r.ReadInt32();
            var length = r.ReadInt32();
            var data = r.ReadBytes(length);
            var entity = Network.Deserialize(data, reader=> GameObject.CloneTemplate(templateID, reader));
            entity.Instantiate(net.Instantiator); // move this to client class
            //if (entity.Exists)// move this to client class
            // TODO spawn it with a separate packet
                //entity.Spawn(net.Map);// move this to client class
        }
        [Obsolete]
        static public void Send(INetwork net, IEnumerable<GameObject> entities)
        {
            if (net is Client)
                throw new Exception();
            var strem = net.GetOutgoingStream();
            strem.Write(PckType);
            strem.Write(entities.Count());
            foreach(var entity in entities)
            {
                if (entity.RefID != 0)
                    throw new Exception();
                net.Instantiate(entity);
                entity.Spawn(net.Map);
                var data = entity.Serialize();
                strem.Write(data.Length);
                strem.Write(data);
            }
        }
        static public void Receive(INetwork net, BinaryReader r)
        {
            if (net is Server)
                throw new Exception();
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var length = r.ReadInt32();
                var data = r.ReadBytes(length);
                var entity = Network.Deserialize<GameObject>(data, GameObject.Create);
                entity.Instantiate(net.Instantiator);//.ObjectCreated();
                if (entity.Exists)
                    entity.Spawn(net.Map);
            }
        }
    }
}
