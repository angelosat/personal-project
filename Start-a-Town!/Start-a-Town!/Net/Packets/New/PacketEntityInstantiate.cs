using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketEntityInstantiate
    {
        static int PckType;
        static int PckTypeNew;

        static public void Init()
        {
            PckType = Network.RegisterPacketHandler( Receive);
            PckTypeNew = Network.RegisterPacketHandler(ReceiveTemplate);
        }
        static public void Send(IObjectProvider net, GameObject entity)
        {
            Send(net, new GameObject[] { entity });
        }
        static public void SendFromTemplate(IObjectProvider net, int templateID, GameObject entity)
        {
            if (net is Client)
                throw new Exception();
            var strem = net.GetOutgoingStream();
            strem.Write((int)PckTypeNew);
            strem.Write(templateID);
            var data = entity.Serialize();
            strem.Write(data.Length);
            strem.Write(data);
        }
        static public void ReceiveTemplate(IObjectProvider net, BinaryReader r)
        {
            if (net is Server)
                throw new Exception();
            var templateID = r.ReadInt32();
            var length = r.ReadInt32();
            var data = r.ReadBytes(length);
            var entity = Network.Deserialize<GameObject>(data, reader=> GameObject.CloneTemplate(templateID, reader));
            entity.Instantiate(net.Instantiator); // move this to client class
            if (entity.IsSpawned)// move this to client class
                entity.Spawn(net);// move this to client class
        }
        static public void Send(IObjectProvider net, IEnumerable<GameObject> entities)
        {
            if (net is Client)
                throw new Exception();
            var strem = net.GetOutgoingStream();
            strem.Write((int)PckType);
            strem.Write(entities.Count());
            foreach(var entity in entities)
            {
                if (entity.RefID != 0)
                    throw new Exception();
                net.Instantiate(entity);
                entity.Spawn(net);
                var data = entity.Serialize();
                strem.Write(data.Length);
                strem.Write(data);
            }
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            if (net is Server)
                throw new Exception();
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var length = r.ReadInt32();
                var data = r.ReadBytes(length);
                var entity = Network.Deserialize<GameObject>(data, GameObject.CreatePrefab);
                entity.Instantiate(net.Instantiator);//.ObjectCreated();
                if (entity.IsSpawned)
                    entity.Spawn(net);
            }
        }
    }
}
