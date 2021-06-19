using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    class PacketHandling
    {
        //private static void HandleMessage(PacketMessage msg)
        //{
        //    switch (msg.PacketType)
        //    {
        //        case PacketType.RequestPlayerID:
        //            //PlayerData temp = Network.Deserialize<PlayerData>(msg.Payload, PlayerData.Read);

        //            string name = Network.Deserialize<string>(msg.Payload, r =>
        //            {
        //                return r.ReadString();
        //                //Encoding.ASCII.GetString();
        //            });

        //            Server.Console.Write(Color.Lime, "SERVER", name + " connected from " + msg.Connection.IP);

        //            PlayerData pl = msg.Connection.Player;//  new PlayerData(msg.Sender) { Socket = Listener, Name = name, ID = PlayerID };
        //            pl.Name = name;
        //            pl.ID = PlayerID;
        //            msg.Player = pl;




        //            Enqueue(pl, PacketMessage.Create(msg.Player.PacketSequence, msg.PacketType, Network.Serialize(w =>
        //            {
        //                w.Write(msg.Player.ID);
        //            }), pl, SendType.Reliable | SendType.Ordered));
        //            UdpConnection state = new UdpConnection(pl.Name + " listener", pl.IP) { Buffer = new byte[PacketMessage.Size], Player = pl };
        //            EndPoint ip = state.IP;
        //            Listener.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ref ip, ReceiveMessage, state);
        //            foreach (var player in from p in Players.GetList() select p)
        //                //player.Outgoing.Enqueue(PacketMessage.Create(msg.Player.PacketSequence, PacketType.PlayerConnecting, Network.Serialize(msg.Player.Write), player));
        //                Enqueue(player, PacketMessage.Create(msg.Player.PacketSequence, PacketType.PlayerConnecting, Network.Serialize(msg.Player.Write), player, SendType.Reliable | SendType.Ordered));

        //            // add to players when entering world instead?
        //            Players.Add(pl);
        //            return;

        //        // the server sends back a copy of the player character
        //        case PacketType.PlayerEnterWorld:
        //            GameObject obj = Network.Deserialize<GameObject>(msg.Payload, GameObject.CreatePrefab);//CreateCustomObject);

        //            // let obj instantiate itself
        //            //Instance.Instantiate(obj);
        //            obj.Instantiate(Instance.Instantiator);
        //            // msg.Player.Character = obj;
        //            //Instance.Spawn(obj);
        //            msg.Player.CharacterID = obj.NetworkID;
        //            msg.Player.Character = obj;


        //            //add player to list of active players (whose character is in the world and must receive world updates
        //            //Players.Add(msg.Player);


        //            SendWorldInfo(msg.Player);
        //            SendMapInfo(msg.Player);
        //            msg.Player.IsActive = true;
        //            //load chunks around player
        //            //Vector2.Zero.GetSpiral(3).ForEach(ChunksToLoad.Add);

        //            Vector2 chunkCoords = obj.Global.GetChunkCoords();
        //            LoadChunk(chunkCoords);

        //            int z = Map.MaxHeight - 2;
        //            while (!(Vector3.UnitZ * z).IsSolid(Instance.Map))
        //                z--;
        //            obj.Global = Vector3.Zero + Vector3.UnitZ * (z + 1);

        //            //instantiate character across network
        //            Instance.SyncInstantiate(obj);
        //            //spawn character actor network
        //            Instance.SyncSpawn(obj);

        //            // send a message to the newly connected client to own their character
        //            //msg.Player.Outgoing.Enqueue(PacketMessage.Create(msg.Player.PacketSequence, PacketType.AssignCharacter, Network.Serialize(obj.Write), msg.Player, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));//.Send(msg.Sender);
        //            Enqueue(msg.Player, PacketMessage.Create(msg.Player.PacketSequence, PacketType.AssignCharacter, Network.Serialize(obj.Write), msg.Player, SendType.Ordered | SendType.Reliable));//.Send(msg.Sender);

        //            foreach (var p in Players.GetList().Where(p => p != msg.Player))
        //            {
        //                Enqueue(p, PacketMessage.Create(p, PacketType.PlayerEnterWorld, Network.Serialize(msg.Player.Write), SendType.Ordered | SendType.Reliable));//.Send(msg.Sender);
        //            }

        //            //OutgoingMessages.Enqueue(Packet.Create(PacketID, PacketType.AssignCharacter, Network.Serialize(writer => writer.Write(obj.NetworkID)), msg.Player, 5));//.Send(msg.Sender);
        //            SyncTime();
        //            return;


        //        //case PacketType.PlayerExitWorld:
        //        //    GameObject plChar;
        //        //    if(!Instance.TryGetNetworkObject(msg.Player.CharacterID, out plChar))
        //        //        throw new Exception("Could not remove player character");

        //        //    // player exited so stop sending him world updates
        //        //    //Players.Remove(msg.Player);
        //        //    msg.Player.IsActive = false;

        //        //    Instance.Despawn(plChar);
        //        //    Instance.DisposeObject(plChar);
        //        //    foreach (var player in Players.GetList())
        //        //        if (player.IsActive)
        //        //            Enqueue(player, PacketMessage.Create(msg.Player, PacketType.PlayerDisconnected, Network.Serialize(w => msg.Player.Write(w)), SendType.Reliable | SendType.Ordered));
        //        //        //Enqueue(player, PacketMessage.Create(msg.Player.PacketSequence, PacketType.PlayerExitWorld, Network.Serialize(w => w.Write(msg.Player.ID)), player, SendType.Reliable | SendType.Ordered));
        //        //    break;

        //        case PacketType.PlayerDisconnected:
        //            CloseConnection(msg.Connection);
        //            break;

        //        case PacketType.PlayerKick:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                var plid = r.ReadInt32();
        //                KickPlayer(plid);
        //            });
        //            break;

        //        case PacketType.PlayerData:
        //            "playerdata".ToConsole();
        //            break;

        //        case PacketType.PlayerList:
        //            "playerlist".ToConsole();
        //            break;

        //        case PacketType.RequestMapInfo:
        //            SendMapInfo(msg.Player);
        //            return;


        //        case PacketType.RequestWorldInfo:
        //            SendWorldInfo(msg.Player);
        //            return;

        //        case PacketType.RequestChunk:
        //            Vector2 vec2 = Network.Deserialize<Vector2>(msg.Payload, reader =>
        //            {
        //                return reader.ReadVector2();
        //            });
        //            HandleChunkRequest(msg.Player, vec2);
        //            return;


        //        //// the server sends back a copy of the player character
        //        //case PacketType.PlayerEnteredWorld:
        //        //    GameObject obj = Network.Deserialize<GameObject>(msg.Payload, GameObject.CreatePrefab);//CreateCustomObject);

        //        //    // let obj instantiate itself
        //        //    //Instance.Instantiate(obj);
        //        //    obj.Instantiate(Instance.Instantiator);
        //        //    // msg.Player.Character = obj;
        //        //    Instance.Spawn(obj);
        //        //    msg.Player.CharacterID = obj.NetworkID;

        //        //    // send only playerdata because it includes their character object and will be serialized along with other info
        //        //    //Packet.Create(PacketType.ObjectCreate, Net.Network.Serialize(obj.Write)).Send(from p in Players select p.Socket); // send to sender too to create their character
        //        //    //Packet.Create(msg.PacketType, Net.Network.Serialize(msg.Player.Write)).Send(from p in Players select p.Socket);
        //        //    foreach (var player in
        //        //        from p in Players.GetList()// select p)
        //        //        where p != msg.Player
        //        //        select p)
        //        //    {
        //        //        //Enqueue(Packet.Create(player.PacketSequence, PacketType.InstantiateObject, Net.Network.Serialize(obj.Write), player, 5)); // send to sender too to create their character
        //        //        Enqueue(Packet.Create(player.PacketSequence, PacketType.InstantiateAndSpawnObject, Net.Network.Serialize(obj.Write), player, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable)); // send to sender too to create their character
        //        //        Enqueue(Packet.Create(player.PacketSequence, msg.PacketType, Net.Network.Serialize(msg.Player.Write), player, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
        //        //    }
        //        //    // send a message to the newly connected client to own their character
        //        //    Outgoing.Enqueue(Packet.Create(msg.Player.PacketSequence, PacketType.AssignCharacter, Network.Serialize(obj.Write), msg.Player, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));//.Send(msg.Sender);
        //        //    //OutgoingMessages.Enqueue(Packet.Create(PacketID, PacketType.AssignCharacter, Network.Serialize(writer => writer.Write(obj.NetworkID)), msg.Player, 5));//.Send(msg.Sender);
        //        //    SyncTime();
        //        //    return;

        //        case PacketType.PlayerServerCommand:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                Command(r.ReadASCII());
        //            });
        //            break;

        //        case PacketType.Chat:
        //            /* log chat text here
        //            /
        //             */

        //            // check if server command


        //            Enqueue(PacketType.Chat, msg.Payload, SendType.OrderedReliable);
        //            break;

        //        case PacketType.PlayerInput:
        //            // handle player input differently ?
        //            //GameObject plObj = Instance.GetNetworkObject(Players.GetList()[msg.Player.ID].CharacterID);
        //            GameObject plObj;
        //            if (!Instance.TryGetNetworkObject(msg.Player.CharacterID, out plObj))
        //            {
        //                Console.Write(Color.Red, "SERVER", "Error processing packet " + msg + " (player character doesn't exist)");
        //                return;
        //            };

        //            msg.Payload.Deserialize(r =>
        //            {
        //                //TimeSpan playerInputTime = TimeSpan.FromMilliseconds(r.ReadDouble());
        //                double timestamp = r.ReadDouble();

        //                // TODO: make receiving player input args as a separate playerinput class, instead of objecteventargs
        //                //ObjectEventArgs a = ObjectEventArgs.Create(Instance, r);
        //                //Instance.PostLocalEvent(plObj, a);

        //                TargetArgs recipient = TargetArgs.Read(Instance, r);
        //                ObjectEventArgs a = ObjectEventArgs.Create(Instance, r);
        //                Instance.PostLocalEvent(recipient.Object, a);

        //                /*
        //                 * check for validity of input around here somewhere
        //                */

        //                // forward input to players (as an objectevent packet?)
        //                // SEND EVENT RELIABLY
        //                byte[] data = Network.Serialize(w =>
        //                {
        //                    w.Write(timestamp);
        //                    recipient.Write(w);// w.Write(msg.Player.CharacterID);
        //                    w.Write((int)a.Type);
        //                    w.Write(a.Data.Length);
        //                    w.Write(a.Data);
        //                    //ObjectLocalEventArgs.Create(a.Type, new TargetArgs(plObj),
        //                });
        //                foreach (var p in Players.GetList())
        //                    //p.Outgoing.Enqueue(PacketMessage.Create(p.PacketSequence, PacketType.ObjectEvent, data, p, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable)); //
        //                    Enqueue(p, PacketMessage.Create(p.PacketSequence, PacketType.ObjectEvent, data, p, SendType.Ordered | SendType.Reliable)); //
        //                //Instance.EventsSinceLastSnapshot.Enqueue(new EventSnapshot(plObj, msg.Payload));
        //            });
        //            return;


        //        case PacketType.PlayerInventoryOperation:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                double timestamp = r.ReadDouble();
        //                TargetArgs recipient = TargetArgs.Read(Instance, r);
        //                Components.ArrangeChildrenArgs invArgs = Components.ArrangeChildrenArgs.Translate(Instance, r);
        //                Instance.InventoryOperation(recipient.Object, invArgs);
        //                byte[] data = Network.Serialize(w =>
        //                {
        //                    w.Write(timestamp);
        //                    recipient.Write(w);
        //                    invArgs.Write(w);
        //                });
        //                foreach (var p in Players.GetList())
        //                    //p.Outgoing.Enqueue(PacketMessage.Create(p.PacketSequence, PacketType.PlayerInventoryOperation, data, p, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
        //                    Enqueue(p, PacketMessage.Create(p.PacketSequence, PacketType.PlayerInventoryOperation, data, p, SendType.Ordered | SendType.Reliable));

        //                //double timestamp = r.ReadDouble();
        //                //TargetArgs recipient = TargetArgs.Read(Instance, r);
        //                //ObjectEventArgs a = ObjectEventArgs.Create(Instance, r);
        //                //Instance.PostLocalEvent(recipient.Object, a);
        //                //byte[] data = Network.Serialize(w =>
        //                //{
        //                //    w.Write(timestamp);
        //                //    recipient.Write(w);
        //                //    w.Write((int)a.Type);
        //                //    w.Write(a.Data.Length);
        //                //    w.Write(a.Data);
        //                //});
        //                //foreach (var p in Players.GetList())
        //                //    Enqueue(Packet.Create(p.PacketSequence, PacketType.ObjectEvent, data, p, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
        //            });

        //            return;

        //        case PacketType.PlayerSlotClick:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                // GameObject plChar = Instance.NetworkObjects[msg.Player.CharacterID];
        //                TargetArgs actor = TargetArgs.Read(Instance, r);
        //                TargetArgs target = TargetArgs.Read(Instance, r);
        //                Instance.PostLocalEvent(target.Slot.Parent, Components.Message.Types.SlotInteraction, actor.Object, target.Slot);
        //            });
        //            foreach (var p in Players.GetList())
        //                //p.Outgoing.Enqueue(PacketMessage.Create(p.PacketSequence, PacketType.PlayerSlotClick, msg.Payload, p, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
        //                Enqueue(p, PacketMessage.Create(p.PacketSequence, PacketType.PlayerSlotClick, msg.Payload, p, SendType.Ordered | SendType.Reliable));

        //            return;

        //        case PacketType.PlayerSetBlock:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                Instance.SyncSetBlock(r.ReadVector3(), (Start_a_Town_.Block.Types)r.ReadInt32(), r.ReadByte(), r.ReadInt32());
        //            });
        //            return;
        //        case PacketType.PlayerRemoveBlock:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                Instance.SyncSetBlock(r.ReadVector3(), Start_a_Town_.Block.Types.Air);
        //            });
        //            return;

        //        case PacketType.PlayerStartMoving:
        //            //msg.Player.Character.GetComponent<ControlComponent>().StartScript(Script.Types.Walk, new ScriptArgs(Instance, msg.Player.Character));
        //            msg.Player.Character.GetComponent<MobileComponent>().Start(msg.Player.Character);
        //            Enqueue(PacketType.PlayerStartMoving, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            return;

        //        case PacketType.PlayerStopMoving:
        //            //msg.Player.Character.GetComponent<ControlComponent>().FinishScript(Script.Types.Walk);
        //            msg.Player.Character.GetComponent<MobileComponent>().Stop(msg.Player.Character);
        //            Enqueue(PacketType.PlayerStopMoving, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            return;

        //        case PacketType.PlayerJump:
        //            //msg.Player.Character.GetComponent<ControlComponent>().StartScript(Script.Types.Jumping, new ScriptArgs(Instance, msg.Player.Character));
        //            msg.Player.Character.GetComponent<MobileComponent>().Jump(msg.Player.Character);
        //            Enqueue(PacketType.PlayerJump, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            return;

        //        case PacketType.PlayerChangeDirection:
        //            //Vector3 direction = Network.Deserialize<Vector3>(msg.Payload, r => r.ReadVector3());
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                Vector3 direction = r.ReadVector3();
        //                msg.Player.Character.Direction = direction;
        //                Enqueue(PacketType.PlayerChangeDirection, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerToggleWalk:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                bool toggle = r.ReadBoolean();
        //                //msg.Player.Character.GetComponent<MobileComponent>().CurrentState = toggle ? MobileComponent.State.Walking : MobileComponent.State.Running;
        //                msg.Player.Character.GetComponent<MobileComponent>().ToggleWalk(toggle);
        //                Enqueue(PacketType.PlayerToggleWalk, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerToggleSprint:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                bool toggle = r.ReadBoolean();
        //                //msg.Player.Character.GetComponent<MobileComponent>().CurrentState = toggle ? MobileComponent.State.Sprinting : MobileComponent.State.Running;
        //                msg.Player.Character.GetComponent<MobileComponent>().ToggleSprint(toggle);
        //                Enqueue(PacketType.PlayerToggleSprint, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerInteract:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                TargetArgs target = TargetArgs.Read(Instance, r);
        //                //msg.Player.Character.GetComponent<WorkComponent>().Perform(target.GetAvailableTasks(Instance).FirstOrDefault(), target);
        //                msg.Player.Character.GetComponent<WorkComponent>().UseTool(msg.Player.Character, target);
        //                Enqueue(PacketType.PlayerInteract, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerUse:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                TargetArgs target = TargetArgs.Read(Instance, r);
        //                msg.Player.Character.GetComponent<WorkComponent>().Perform(target.GetAvailableTasks(Instance).FirstOrDefault(), target);
        //                Enqueue(PacketType.PlayerUse, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerUseHauled:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                TargetArgs target = TargetArgs.Read(Instance, r);
        //                var hauled = msg.Player.Character.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling].Object;
        //                if (hauled.IsNull())
        //                    return;
        //                msg.Player.Character.GetComponent<WorkComponent>().Perform(hauled.GetHauledActions().FirstOrDefault(), target);
        //                Enqueue(PacketType.PlayerUseHauled, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerDropHauled:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                TargetArgs target = TargetArgs.Read(Instance, r);
        //                msg.Player.Character.GetComponent<GearComponent>().Throw(Vector3.Zero, msg.Player.Character);
        //                Enqueue(PacketType.PlayerDropHauled, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerDropInventory:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                byte slotid = r.ReadByte();
        //                int amount = r.ReadInt32();
        //                Instance.PostLocalEvent(msg.Player.Character, Message.Types.DropInventoryItem, (int)slotid, amount);
        //                Enqueue(PacketType.PlayerDropInventory, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerRemoteCall:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                TargetArgs target = TargetArgs.Read(Instance, r);
        //                Message.Types call = (Message.Types)r.ReadInt32();

        //                int dataLength = (int)(r.BaseStream.Length - r.BaseStream.Position);
        //                byte[] args = r.ReadBytes(dataLength);

        //                target.Object.HandleRemoteCall(ObjectEventArgs.Create(call, args));

        //                Enqueue(PacketType.PlayerRemoteCall, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerPickUp:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                TargetArgs target = TargetArgs.Read(Instance, r);
        //                //switch (target.Type)
        //                //{
        //                //    case TargetType.Position:
        //                //        // check if hauling and drop at target position
        //                //        GameObject held = msg.Player.Character.GetComponent<GearComponent>().Holding.Take();
        //                //        if (held.IsNull())
        //                //            return;
        //                //        //held.Global = target.FinalGlobal;
        //                //        Instance.Spawn(held, target.FinalGlobal);
        //                //        break;
        //                //    case TargetType.Entity:
        //                //        //pickup item or switch places with held item
        //                //        Instance.PostLocalEvent(msg.Player.Character, Message.Types.Insert, target.Object.ToSlot());
        //                //        break;
        //                //    default:
        //                //        break;
        //                //}
        //                msg.Player.Character.GetComponent<WorkComponent>().Perform(new Components.Scripts.PickUp(), target);
        //                Enqueue(PacketType.PlayerPickUp, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerHaul:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                TargetArgs target = TargetArgs.Read(Instance, r);
        //                msg.Player.Character.GetComponent<ControlComponent>().StartScript(Script.Types.Hauling, new ScriptArgs(Instance, msg.Player.Character, target));
        //                Enqueue(PacketType.PlayerHaul, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerEquip:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                TargetArgs target = TargetArgs.Read(Instance, r);
        //                msg.Player.Character.GetComponent<ControlComponent>().StartScript(Script.Types.Equipping, new ScriptArgs(Instance, msg.Player.Character, target));
        //                Enqueue(PacketType.PlayerEquip, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerThrow:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                var dir = r.ReadVector3();
        //                msg.Player.Character.GetComponent<GearComponent>().Throw(msg.Player.Character, dir);
        //                Enqueue(PacketType.PlayerThrow, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerStartAttack:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                msg.Player.Character.GetComponent<ControlComponent>().StartScript(Script.Types.Attack, new ScriptArgs(Instance, msg.Player.Character, TargetArgs.Empty));
        //                Enqueue(PacketType.PlayerStartAttack, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerFinishAttack:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                var dir = r.ReadVector3();
        //                msg.Player.Character.GetComponent<ControlComponent>().FinishScript(Script.Types.Attack, new ScriptArgs(Instance, msg.Player.Character, TargetArgs.Empty, w => w.Write(dir)));
        //                Enqueue(PacketType.PlayerFinishAttack, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerStartBlocking:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                msg.Player.Character.GetComponent<ControlComponent>().StartScript(Script.Types.Block, new ScriptArgs(Instance, msg.Player.Character, TargetArgs.Empty));
        //                Enqueue(PacketType.PlayerStartBlocking, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlayerFinishBlocking:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int netid = r.ReadInt32();
        //                msg.Player.Character.GetComponent<ControlComponent>().FinishScript(Script.Types.Block, new ScriptArgs(Instance, msg.Player.Character, TargetArgs.Empty));
        //                Enqueue(PacketType.PlayerFinishBlocking, msg.Payload, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        //case PacketType.PlayerSlotClick:
        //        //    Network.Deserialize(msg.Payload, r =>
        //        //    {
        //        //        double timestamp = r.ReadDouble();
        //        //        TargetArgs actor = TargetArgs.Read(Instance, r);
        //        //        TargetArgs slot = TargetArgs.Read(Instance, r);
        //        //        Components.ArrangeChildrenArgs invArgs = Components.ArrangeChildrenArgs.Translate(Instance, r);
        //        //        Instance.InventoryOperation(recipient.Object, invArgs);
        //        //        byte[] data = Network.Serialize(w =>
        //        //        {
        //        //            w.Write(timestamp);
        //        //            recipient.Write(w);
        //        //            invArgs.Write(w);
        //        //        });
        //        //        foreach (var p in Players.GetList())
        //        //            Enqueue(Packet.Create(p.PacketSequence, PacketType.PlayerInventoryOperation, data, p, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
        //        //    });
        //        //    return;

        //        case PacketType.PlayerCraft:
        //            Network.Deserialize(msg.Payload, r =>
        //            {
        //                int chid = r.ReadInt32();
        //                Components.Crafting.Reaction.Product.ProductMaterialPair product = new Components.Crafting.Reaction.Product.ProductMaterialPair(r);
        //                //Instance.PopLoot(product.Product, Instance.NetworkObjects[chid]);

        //                // TODO: check here if crafting is legal (check available materials etc)
        //                var container = msg.Player.Character.GetComponent<PersonalInventoryComponent>().Children;
        //                if (!product.MaterialsAvailable(container))
        //                    return;
        //                product.ConsumeMaterials(Instance, container);

        //                Instance.SyncInstantiate(product.Product);
        //                Instance.PostLocalEvent(msg.Player.Character, Message.Types.Insert, product.Product.ToSlot());
        //                byte[] newData = Network.Serialize(w => { w.Write(chid); w.Write(product.Product.NetworkID); });
        //                Enqueue(PacketType.PlayerCraft, newData, SendType.Ordered | SendType.Reliable);
        //            });
        //            return;

        //        case PacketType.PlaceBlockConstruction:
        //            Network.Deserialize(msg.Payload, r =>
        //            {
        //                //Block.Types type = (Block.Types)r.ReadInt32();
        //                //byte blockdata = r.ReadByte();
        //                Components.Crafting.BlockConstruction.ProductMaterialPair product = new Components.Crafting.BlockConstruction.ProductMaterialPair(r);
        //                Vector3 global = r.ReadVector3();
        //                //Instance.SyncSpawn(Instance.SyncCreate(GameObject.Types.Construction));
        //                //obj = Instance.SyncCreate(GameObject.Types.ConstructionBlock);
        //                obj = GameObject.Create(GameObject.Types.ConstructionBlock);
        //                obj.Global = global;
        //                obj.GetComponent<ConstructionComponent>().Product = product;
        //                //obj.GetComponent<ConstructionComponent>().BlockType = product.Product.ty;
        //                //obj.GetComponent<ConstructionComponent>().BlockData = blockdata;
        //                Instance.SyncInstantiate(Instance.Instantiate(obj));
        //                Instance.SyncSpawn(obj);
        //            });
        //            return;

        //        case PacketType.PlaceConstruction:
        //            Network.Deserialize(msg.Payload, r =>
        //            {
        //                //GameObject.Types type = (GameObject.Types)r.ReadInt32();
        //                Components.Crafting.Reaction.Product.ProductMaterialPair product = new Components.Crafting.Reaction.Product.ProductMaterialPair(r);
        //                Vector3 global = r.ReadVector3();
        //                obj = GameObject.Create(GameObject.Types.Construction);
        //                obj.Global = global;
        //                obj.GetComponent<StructureComponent>().Product = product;
        //                Instance.SyncInstantiate(Instance.Instantiate(obj));
        //                Instance.SyncSpawn(obj);
        //            });
        //            return;

        //        case PacketType.InstantiateAndSpawnObject:
        //            Network.Deserialize(msg.Payload, r =>
        //            {
        //                obj = GameObject.CreatePrefab(r);
        //                Position pos = Position.Create(Instance.Map, r);
        //                //Instance.Spawn(obj, pos);
        //                obj.Instantiate(Instance.Instantiator);
        //                obj.GetPosition().Position = pos;
        //                Instance.SyncInstantiate(obj);
        //                Instance.SyncSpawn(obj);
        //            });
        //            return;

        //        case PacketType.InstantiateObject:
        //            obj = Network.Deserialize<GameObject>(msg.Payload, GameObject.CreateCustomObject);
        //            Instance.InstantiateObject(obj);
        //            return;

        //        /// received input from player to directly destroy an object (for example in editing mode)
        //        /// 
        //        case PacketType.DisposeObject:

        //            //if (Instance.NetworkObjects.TryRemove(Network.Deserialize<GameObject>(msg.Payload, GameObject.Create).NetworkID, out obj))
        //            //    obj.Remove();

        //            //Instance.DestroyObject(Network.Deserialize<GameObject>(msg.Payload, GameObject.Create).NetworkID);
        //            TargetArgs tar = Network.Deserialize<TargetArgs>(msg.Payload, r => TargetArgs.Read(Instance, r));

        //            // maybe add a playercomponent to player controller objects to check them faster
        //            if ((from p in Players.GetList()
        //                 where p.CharacterID == tar.Object.NetworkID
        //                 select p).Count() > 0)
        //                break;

        //            Instance.SyncDisposeObject(tar.Object);
        //            //foreach (var player in Players.GetList())
        //            //    Enqueue(msg.Copy(msg.Player.PacketSequence, player, 5));

        //            break;

        //        case PacketType.Ack:
        //            Network.Deserialize(msg.Payload, r =>
        //            {
        //                //WaitingForAck.Remove(r.ReadInt64());

        //                long ackID = r.ReadInt64();
        //                PacketMessage existing;
        //                if (msg.Player.WaitingForAck.TryRemove(ackID, out existing))
        //                {
        //                    if (existing.PacketType == PacketType.Ping)
        //                        msg.Connection.Ping.Stop();
        //                    existing.RTT.Stop();
        //                    msg.Connection.RTT = TimeSpan.FromMilliseconds(existing.RTT.ElapsedMilliseconds);
        //                    msg.Player.Ping = TimeSpan.FromMilliseconds(existing.RTT.ElapsedMilliseconds).Milliseconds;
        //                    if (msg.Player.OrderedPackets.Count > 0)
        //                        if (msg.Player.OrderedPackets.Peek().ID == ackID)
        //                            msg.Player.OrderedPackets.Dequeue();
        //                }
        //            });
        //            break;

        //        case PacketType.RequestNewObject:
        //            Network.Deserialize(msg.Payload, r =>
        //            {
        //                GameObject toDrag = GameObject.CreatePrefab(r);//.Instantiate(Instance.Instantiator);
        //                Instance.InstantiateObject(toDrag);
        //                byte amount = r.ReadByte();
        //                foreach (var player in Players.GetList())
        //                    //player.Outgoing.
        //                    Enqueue(player, PacketMessage.Create(msg.Player.PacketSequence, PacketType.RequestNewObject, Network.Serialize(w =>
        //                    {
        //                        TargetArgs.Write(w, toDrag);
        //                        w.Write(amount);
        //                    }), player, SendType.Ordered | SendType.Reliable));
        //            });
        //            break;

        //        case PacketType.JobCreate:
        //            Network.Deserialize(msg.Payload, r =>
        //            {
        //                TownJob job = TownJob.Read(r, Instance);
        //                Instance.Map.Town.Jobs.Add(job);
        //                throw new NotImplementedException();
        //                //Enqueue(Players.GetList(), msg.PacketType, Network.Serialize(job.Write));
        //            });
        //            break;

        //        case PacketType.JobDelete:
        //            Network.Deserialize(msg.Payload, r =>
        //            {
        //                int jobID = r.ReadInt32();
        //                Instance.Map.Town.Jobs.Remove(jobID);
        //                throw new NotImplementedException();
        //                //Enqueue(Players.GetList(), msg.PacketType, msg.Payload);
        //            });
        //            break;


        //        default:
        //            break;
        //    }

        //    // send world state changes periodically (delta states)
        //    //foreach (var p in Players)
        //    //    msg.BeginSendTo(p.Socket, p.IP);
        //}

    }
}
