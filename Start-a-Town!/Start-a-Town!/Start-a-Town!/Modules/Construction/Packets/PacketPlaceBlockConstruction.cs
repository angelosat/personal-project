using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Modules.Construction
{
    class PacketPlaceBlockConstruction : Packet
    {
        static public void Handle(Server server, Packet msg)
        {
            msg.Payload.Deserialize(r =>
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
            });
        }

        static public void Handle(Client client, Packet msg)
        {
            msg.Payload.Deserialize(r =>
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
                });
        }
    }
}
