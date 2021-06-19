using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Modules.Construction
{
    class ConstructionPacketHandler : IServerPacketHandler, IClientPacketHandler
    {
        public void HandlePacket(Server server, Packet msg)
        {
            switch (msg.PacketType)
            {
                case PacketType.PlaceBlockConstruction:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        var netid = r.ReadInt32();
                        Components.Crafting.BlockConstruction.ProductMaterialPair product = new Components.Crafting.BlockConstruction.ProductMaterialPair(r);
                        //Vector3 global = r.ReadVector3();
                        TargetArgs target = TargetArgs.Read(server, r);
                        var global = target.FaceGlobal;
                        bool designate = r.ReadBoolean();
                        bool remove = r.ReadBoolean();
                        if (remove)
                        {
                            //server.SyncSetBlock(global, Block.Types.Air);
                            server.Map.RemoveBlock(target.Global);
                        }
                        else
                        {
                            BlockDesignation.Place(server.Map, global, 0, 0, 0, product);
                            if (!designate) 
                                WorkComponent.Start(msg.Player.Character, new BlockDesignation.InteractionBuild(), new TargetArgs(global));
                        }
                        server.Enqueue(PacketType.PlaceBlockConstruction, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                        return;

                        //if (designate)
                        //{
                            var constr = GameObjectDb.ConstructionBlock.SetGlobal(global);
                            var component = constr.GetComponent<ConstructionComponent>();
                            component.SetProduct(product);
                            server.SyncInstantiate(constr);
                            server.SyncSpawn(constr);
                            if (!designate)
                            {
                                msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, new ConstructionComponent.InteractionBuild2(component), new TargetArgs(constr));
                                server.RemoteProcedureCall(new TargetArgs(constr), Message.Types.PlayerStartConstruction, BitConverter.GetBytes(msg.Player.Character.InstanceID), global);
                            }
                        return;
                        //}
                        //else
                        //{
                        msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, new InteractionConstruct(product), new TargetArgs(global));
                        server.Enqueue(PacketType.PlaceBlockConstruction, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global);
                        //}

                        //var netid = r.ReadInt32();
                        //Components.Crafting.BlockConstruction.ProductMaterialPair product = new Components.Crafting.BlockConstruction.ProductMaterialPair(r);
                        //var global = r.ReadVector3();
                        //var obj = GameObjectDb.ConstructionBlock;// GameObject.Create(GameObject.Types.ConstructionBlock);
                        //obj.Global = global;
                        //obj.GetComponent<ConstructionComponent>().SetProduct(product);
                        //server.SyncInstantiate(server.Instantiate(obj));
                        //server.SyncSpawn(obj);
                    });
                    return;

                case PacketType.PlaceConstruction:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        var netid = r.ReadInt32();
                        //GameObject.Types type = (GameObject.Types)r.ReadInt32();
                        Components.Crafting.Reaction.Product.ProductMaterialPair product = new Components.Crafting.Reaction.Product.ProductMaterialPair(r);
                        Vector3 global = r.ReadVector3();
                        msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, new Modules.Construction.InteractionConstruct(product), new TargetArgs(global));
                        server.Enqueue(PacketType.PlaceConstruction, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global);
                        return;

                        var obj = GameObject.Create(GameObject.Types.Construction);
                        obj.Global = global;
                        //obj.GetComponent<StructureComponent>().Product = product;
                        obj.GetComponent<StructureComponent>().SetProduct(product);
                        server.SyncInstantiate(server.Instantiate(obj));
                        server.SyncSpawn(obj);
                    });
                    return;

                default:
                    break;
            }
        }
        public void HandlePacket(Client client, Packet msg)
        {
            switch(msg.PacketType)
            {
                case PacketType.PlaceBlockConstruction:
                    
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int netid = r.ReadInt32();
                        GameObject obj;
                        if (!client.TryGetNetworkObject(netid, out obj))
                        {
                            client.RequestEntityFromServer(netid);
                            return;
                        }
                        Components.Crafting.BlockConstruction.ProductMaterialPair product = new Components.Crafting.BlockConstruction.ProductMaterialPair(r);
                        //Vector3 global = r.ReadVector3();
                        TargetArgs target = TargetArgs.Read(client, r);
                        var global = target.FaceGlobal;
                        bool designate = r.ReadBoolean();
                        bool remove = r.ReadBoolean();
                        if (remove)
                        {
                            client.Map.RemoveBlock(target.Global);
                            return;
                        }
                        //Block.Designation.Place(client.Map, global, 0, 0, 0);
                        BlockDesignation.Place(client.Map, global, 0, 0, 0, product);
                        if (!designate) WorkComponent.Start(obj, new BlockDesignation.InteractionBuild(), new TargetArgs(global));
                        return;

                        obj.GetComponent<WorkComponent>().Perform(obj, new InteractionConstruct(product), new TargetArgs(global));
                        return;
                    });
                    return;

                case PacketType.PlaceConstruction:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        var netid = r.ReadInt32();
                        //GameObject.Types type = (GameObject.Types)r.ReadInt32();
                        Components.Crafting.Reaction.Product.ProductMaterialPair product = new Components.Crafting.Reaction.Product.ProductMaterialPair(r);
                        Vector3 global = r.ReadVector3();
                        GameObject obj;
                        if (!client.TryGetNetworkObject(netid, out obj))
                        {
                            client.RequestEntityFromServer(netid);
                            return;
                        }
                        obj.GetComponent<WorkComponent>().Perform(obj, new Modules.Construction.InteractionConstruct(product), new TargetArgs(global));
                        return;
                    });
                    return;

                default:
                    break;
            }
        }
    }
}
