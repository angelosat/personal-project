using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Net.Packets;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using Start_a_Town_.Modules.AI.Net.Packets;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Modules.AI.Net;

namespace Start_a_Town_.AI
{
    class AIManager : GameComponent
    {
        public override void Initialize()
        {
            PacketAILogWrite.Init();
            PacketForceTask.Init();
            PacketNeedModify.Init();
            PacketTaskUpdate.Init();
            AITask.Initialize();
        }
        [Obsolete]
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
        [Obsolete]
        static public void AIStopMove(GameObject entity)
        {
            var acc = entity.Acceleration;
            if (acc == 0)
                return;
            AIState.GetState(entity).Path = null;
            entity.GetComponent<MobileComponent>().Stop(entity);
            byte[] data = Network.Serialize(new PacketEntity(entity.RefID).Write);
            Server.Instance.Enqueue(PacketType.PlayerStopMoving, data, SendType.OrderedReliable, entity.Global, true);
        }
        [Obsolete]
        static public void AIStopMoveNew(GameObject entity)
        {
            var acc = entity.Acceleration;
            if (acc == 0)
                return;
            entity.GetComponent<MobileComponent>().Stop(entity);
            byte[] data = Network.Serialize(new PacketEntity(entity.RefID).Write);
            Server.Instance.Enqueue(PacketType.PlayerStopMoving, data, SendType.OrderedReliable, entity.Global, true);
        }
        [Obsolete]
        static public void AIStartMove(GameObject entity)
        {
            entity.GetComponent<MobileComponent>().Start(entity);
            byte[] data = Network.Serialize(new PacketEntity(entity.RefID).Write);
            Server.Instance.Enqueue(PacketType.PlayerStartMoving, data, SendType.OrderedReliable, entity.Global, true);
        }
        [Obsolete]
        static public void AIToggleWalk(GameObject entity, bool toggle)
        {
            entity.GetComponent<MobileComponent>().ToggleWalk(toggle);
            byte[] data = Network.Serialize(new PacketEntityBoolean(entity.RefID, toggle).Write);
            Server.Instance.Enqueue(PacketType.PlayerToggleWalk, data, SendType.OrderedReliable, entity.Global, true);
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

        public override void OnTooltipCreated(ITooltippable item, Tooltip t)
        {
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
            if (entity.Net is Server) // interactions only initiated server-side?
            {
                entity.TryGetComponent<WorkComponent>(c => c.Perform(entity, action, target));
                PacketEntityInteract.Send(Server.Instance, entity, action, target);
            }
        }

        internal static void EndInteraction(GameObject entity, bool success = false)
        {
            WorkComponent.End(entity, success);
            PacketEntityInteract.EndInteraction(Server.Instance, entity, success);
        }
    }
}
