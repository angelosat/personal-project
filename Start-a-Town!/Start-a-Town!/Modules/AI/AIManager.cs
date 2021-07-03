using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Net.Packets;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.GameEvents;
using Start_a_Town_.UI;
using Start_a_Town_.Modules.AI.Net.Packets;

namespace Start_a_Town_.AI
{
    class AIManager : GameComponent
    {
        public override void Initialize()
        {
            Server.Instance.RegisterPacketHandler(Net.PacketType.AI, new AIPacketHandler());
            Client.Instance.RegisterPacketHandler(Net.PacketType.AI, new AIPacketHandler());

            Client.RegisterPacketHandler(PacketType.AITaskUpdate, Start_a_Town_.Modules.AI.Net.PacketTaskUpdate.Receive);
            Client.RegisterPacketHandler(PacketType.NeedModifyValue, Start_a_Town_.Components.Needs.PacketNeedModify.Receive);
            Client.RegisterPacketHandler(PacketType.AILogWrite, PacketAILogWrite.Receive);

            PacketForceTask.Init();

            AITask.Initialize();
        }
        static public void AIMove(GameObject entity, Vector3 dir)
        {
            var acc = entity.Acceleration;
            entity.Direction = dir;
            if (acc == 0)
            {
                AIToggleWalk(entity, false);
                AIStartMove(entity);
            }
        }
        static public void AIStopMove(GameObject entity)
        {
            var acc = entity.Acceleration;
            if (acc == 0)
                return;
            AIState.GetState(entity).Path = null;//.Stack.Clear();
            entity.GetComponent<MobileComponent>().Stop(entity);
            //byte[] data = PacketEntity.Write(entity.InstanceID).Compress();
            byte[] data = Network.Serialize(new PacketEntity(entity.RefID).Write);
            Server.Instance.Enqueue(PacketType.PlayerStopMoving, data, SendType.OrderedReliable, entity.Global, true);
        }
        static public void AIStopMoveNew(GameObject entity)
        {
            var acc = entity.Acceleration;
            if (acc == 0)
                return;
            entity.GetComponent<MobileComponent>().Stop(entity);
            //byte[] data = PacketEntity.Write(entity.InstanceID).Compress();
            byte[] data = Network.Serialize(new PacketEntity(entity.RefID).Write);
            Server.Instance.Enqueue(PacketType.PlayerStopMoving, data, SendType.OrderedReliable, entity.Global, true);
        }
        static public void AIStartMove(GameObject entity)
        {

            entity.GetComponent<MobileComponent>().Start(entity);
            //byte[] data = new PacketEntityStartMove(entity.InstanceID).Write().Compress();
            //byte[] data = PacketEntity.Write(entity.InstanceID).Compress();
            byte[] data = Network.Serialize(new PacketEntity(entity.RefID).Write);
            Server.Instance.Enqueue(PacketType.PlayerStartMoving, data, SendType.OrderedReliable, entity.Global, true);
        }
        static public void AIToggleWalk(GameObject entity, bool toggle)
        {
            entity.GetComponent<MobileComponent>().ToggleWalk(toggle);
            byte[] data = Network.Serialize(new PacketEntityBoolean(entity.RefID, toggle).Write);
            Server.Instance.Enqueue(PacketType.PlayerToggleWalk, data, SendType.OrderedReliable, entity.Global, true);
        }
        //public override void HandlePacket(Client client, Packet msg)
        //{
        //    Handle(client, msg);
        //}
        public override void HandlePacket(Server server, Packet p)
        {
            switch (p.PacketType)
            {
                case PacketType.AIGenerateNpc:
                    p.Payload.Deserialize(r =>
                    {
                        //var senderid = r.ReadInt32();
                        //var senderentity = server.GetNetworkObject(senderid);
                        var npc = GenerateNpc(server.GetRandom());
                        npc.Global = p.Player.ControllingEntity.Global + Vector3.UnitZ;
                        server.Spawn(npc);
                    });
                    break;

                default:
                    HandlePacket(server, p);
                    break;
            }
        }

        public static void HandlePacket(IObjectProvider net, Packet p)
        {
            
        }

        static public GameObject GenerateNpc(RandomThreaded random)
        {
            //var npc = GameObject.Objects[GameObject.Types.Npc].Clone();
            var npc = GameObject.Create(GameObject.Types.Npc);
            AIState state = AIState.GetState(npc);
            state.Generate(npc, random);
            npc.Name = NpcComponent.GetRandomFullName();
            return npc;
        }

        public override void OnGameEvent(GameEvent e)
        {
            if (e.Net is Client)
                return;
            switch (e.Type)
            {
                case Message.Types.EntityAttacked:
                    var attacker = e.Parameters[0] as GameObject;
                    var target = e.Parameters[1] as GameObject;
                    var dmg = (int)e.Parameters[2];
                    if (!target.HasComponent<AIComponent>())
                        break;
                    var st = AIState.GetState(target);
                    if (st != null)
                    {
                        //var thr = new Behaviors.Threat(target, dmg, attacker);
                        Behaviors.Threat thr = st.Threats.FirstOrDefault(t => t.Entity == attacker);
                        if (thr == null)
                        {
                            thr = new Behaviors.Threat(target, dmg, attacker);
                            st.Threats.Add(thr);
                        }
                        else
                            thr.Value += dmg;
                    }
                    break;

                default:
                    break;
            }
        }

        static public void SyncLogWrite(GameObject agent, string entry)
        {
            throw new NotImplementedException();
        }

        public override void OnTooltipCreated(ITooltippable item, UI.Tooltip t)
        {
            //var target = item as TargetArgs;
            //if (target == null)
            //    return;
            if (item is not TargetArgs target)
                return;
            if (target.Type != TargetType.Entity)
                return;
            var obj = target.Object;
            if (obj == null)
                return;
        }

        internal static void Interact(GameObject entity, Interaction action, TargetArgs target)
        {
            if (entity.Net is Net.Server) // interactions only initiated server-side?
            {
                entity.TryGetComponent<WorkComponent>(c => c.Perform(entity, action, target));
                PacketEntityInteract.Send(Server.Instance, entity, action, target);
                //var p = new PacketEntityInteractionTarget(entity, action, target);
                //byte[] data = Network.Serialize(p.Write);
                //(entity.Net as Server).Enqueue(PacketType.EntityInteract, data, SendType.OrderedReliable, entity.Global, true);
            }
        }

        internal static void EndInteraction(GameObject entity, bool success = false)
        {
            WorkComponent.End(entity, success);
            PacketEntityInteract.EndInteraction(Server.Instance, entity, success);
        }
    }
}
