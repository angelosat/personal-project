using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Combat;
using Start_a_Town_.Net.Packets;
using Start_a_Town_.AI;

namespace Start_a_Town_.Net
{
    public partial class Server
    {
        public class AI
        {
            public void AIStartMove(GameObject entity)
            {
                entity.GetComponent<MobileComponent>().Start(entity);
                //byte[] data = new PacketEntityStartMove(entity.Network.ID).Write().Compress();
                //byte[] data = PacketEntity.Write(entity.Network.ID).Compress();
                byte[] data = Network.Serialize(new PacketEntity(entity.Network.ID).Write);
                Instance.Enqueue(PacketType.PlayerStartMoving, data, SendType.OrderedReliable, entity.Global, true);
            }
            public void AIStopMove(GameObject entity)
            {
                entity.GetComponent<MobileComponent>().Stop(entity);
                //byte[] data = PacketEntity.Write(entity.Network.ID).Compress();
                byte[] data = Network.Serialize(new PacketEntity(entity.Network.ID).Write);
                Instance.Enqueue(PacketType.PlayerStopMoving, data, SendType.OrderedReliable, entity.Global, true);
            }
            public void AIChangeDirection(GameObject entity, Vector3 direction)
            {
                entity.Direction = direction;
                byte[] data = Network.Serialize(new PacketEntityVector3(entity.Network.ID, direction).Write);
                Instance.Enqueue(PacketType.PlayerChangeDirection, data, SendType.OrderedReliable, entity.Global, true);
            }
            public void AIToggleWalk(GameObject entity, bool toggle)
            {
                entity.GetComponent<MobileComponent>().ToggleWalk(toggle);
                byte[] data = Network.Serialize(new PacketEntityBoolean(entity.Network.ID, toggle).Write);
                Instance.Enqueue(PacketType.PlayerToggleWalk, data, SendType.OrderedReliable, entity.Global, true);
            }
            public void AIStartAttack(GameObject entity)
            {
                entity.GetComponent<AttackComponent>().Start(entity);
                byte[] data = Network.Serialize(new PacketEntity(entity.Network.ID).Write);
                Instance.Enqueue(PacketType.PlayerStartAttack, data, SendType.OrderedReliable, entity.Global, true);
            }
            public void AIFinishAttack(GameObject entity, Vector3 direction)
            {
                entity.GetComponent<AttackComponent>().Finish(entity);
                byte[] data = Network.Serialize(new PacketEntityVector3(entity.Network.ID, direction).Write);
                Instance.Enqueue(PacketType.PlayerFinishAttack, data, SendType.OrderedReliable, entity.Global, true);
            }
            public void AIStartBlock(GameObject entity)
            {
                entity.GetComponent<BlockingComponent>().Start(entity);
                byte[] data = Network.Serialize(new PacketEntity(entity.Network.ID).Write);
                Instance.Enqueue(PacketType.PlayerStartBlocking, data, SendType.OrderedReliable, entity.Global, true);
            }
            public void AIStopBlock(GameObject entity)
            {
                entity.GetComponent<BlockingComponent>().Stop(entity);
                byte[] data = Network.Serialize(new PacketEntity(entity.Network.ID).Write);
                Instance.Enqueue(PacketType.PlayerFinishBlocking, data, SendType.OrderedReliable, entity.Global, true);
            }
            public void AICancelAttack(GameObject entity)
            {
                entity.GetComponent<AttackComponent>().Cancel(entity);
                byte[] data = Network.Serialize(new PacketEntity(entity.Network.ID).Write);
                Instance.Enqueue(PacketType.EntityCancelAttack, data, SendType.OrderedReliable, entity.Global, true);
            }
            public void AIConversationFinish(GameObject entity, string text)
            {
                //entity.GetComponent<SpeechComponent>().FinishConversation(entity, text);
                entity.Net.PostLocalEvent(entity, Message.Types.ConversationFinish, text);

                byte[] data = Network.Serialize(w =>
                {
                    w.Write(entity.Network.ID);
                    w.Write(text);
                });
                Instance.Enqueue(PacketType.ConversationFinish, data, SendType.OrderedReliable, entity.Global, true);
            }
            public void AIConversationStart(GameObject entity, GameObject partner)
            {
                entity.Net.PostLocalEvent(entity, Message.Types.ConversationStart);

                byte[] data = Network.Serialize(w =>
                {
                    w.Write(entity.Network.ID);
                    w.Write(partner.Network.ID);
                });
                Instance.Enqueue(PacketType.ConversationStart, data, SendType.OrderedReliable, entity.Global, true);
            }

            //public void AIInteract(GameObject entity, string interactionName, TargetArgs target)
            //{
            //    Interaction action = target.GetInteraction(Instance, interactionName);
            //    if (action == null)
            //        throw new ArgumentException();
            //    entity.TryGetComponent<WorkComponent>(c => c.Perform(entity, action, target));

            //    byte[] data = Network.Serialize(new PacketEntityInteraction(entity.Network.ID, interactionName, target).Write);
            //    Instance.Enqueue(PacketType.EntityInteract, data, SendType.OrderedReliable, entity.Global);
            //}
            public void AIInteract(GameObject entity, Interaction action, TargetArgs target)
            {
                var inters= target.GetInteractions(entity.Net);
                if (!inters.ContainsKey(action.Name))
                    throw new Exception();

                entity.TryGetComponent<WorkComponent>(c => c.Perform(entity, action, target));
                var p  = new PacketEntityInteractionTarget(entity, action.Name, target);
                byte[] data = Network.Serialize(p.Write);
                Instance.Enqueue(PacketType.EntityInteract, data, SendType.OrderedReliable, entity.Global, true);
            }
            public void AIInterrupt(GameObject entity)
            {
                WorkComponent.Stop(entity);
                var p = new PacketEntity(entity);
                byte[] data = Network.Serialize(p.Write);
                Instance.Enqueue(PacketType.EntityInterrupt, data, SendType.OrderedReliable, entity.Global, true);
            }
            internal void AIThrow(GameObject entity, Vector3 dir, bool all)
            {
                HaulComponent.ThrowHauled(entity, dir, all);
                byte[] data = Network.Serialize(w =>
                    {
                        w.Write(entity.InstanceID);
                        w.Write(dir);
                        w.Write(all);
                    });

                Instance.Enqueue(PacketType.EntityThrow, data, SendType.OrderedReliable, entity.Global, true);
            }
            internal void AIJobAccepted(GameObject parent, AIJob newJob)
            {
                //throw new NotImplementedException();
            }

            internal void AIDialog(GameObject parent, GameObject target, string text, IEnumerable<string> dialogOptions)//, Progress attention)
            {
                //byte[] data = Network.Serialize(w =>
                //{
                //    w.Write(parent.Network.ID);
                //    w.Write(target.Network.ID);
                //    w.Write(text);
                //    w.Write(dialogOptions.Count());
                //    foreach (var item in dialogOptions)
                //        w.Write(item);
                //});
                var pack = new PacketDialogueOptions(parent, target, text, dialogOptions.ToList());//, attention);
                Instance.Enqueue(PacketType.Conversation, pack.Write(), SendType.OrderedReliable, parent.Global, true);
            }



        }

        
    }
}
