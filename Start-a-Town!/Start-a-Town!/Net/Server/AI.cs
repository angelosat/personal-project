using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Net.Packets;
using Start_a_Town_.AI;

namespace Start_a_Town_.Net
{
    public partial class Server
    {
        public class AI
        {
            [Obsolete]
            public void AIToggleWalk(GameObject entity, bool toggle)
            {
                entity.GetComponent<MobileComponent>().ToggleWalk(toggle);
                byte[] data = Network.Serialize(new PacketEntityBoolean(entity.RefID, toggle).Write);
                Instance.Enqueue(PacketType.PlayerToggleWalk, data, SendType.OrderedReliable, entity.Global, true);
            }
            [Obsolete]
            public void AIStartAttack(GameObject entity)
            {
                entity.GetComponent<AttackComponent>().Start(entity);
                byte[] data = Network.Serialize(new PacketEntity(entity.RefID).Write);
                Instance.Enqueue(PacketType.PlayerStartAttack, data, SendType.OrderedReliable, entity.Global, true);
            }
            [Obsolete]
            public void AIFinishAttack(GameObject entity, Vector3 direction)
            {
                entity.GetComponent<AttackComponent>().Finish(entity);
                byte[] data = Network.Serialize(new PacketEntityVector3(entity.RefID, direction).Write);
                Instance.Enqueue(PacketType.PlayerFinishAttack, data, SendType.OrderedReliable, entity.Global, true);
            }
          
            public void AIInteract(GameObject entity, Interaction action, TargetArgs target)
            {
                AIManager.Interact(entity, action, target);
            }
        }
    }
}
